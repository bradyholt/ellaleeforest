using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PetaPoco;

namespace bude.server
{
    public class TransactionImporter
    {
        private static NLog.Logger s_logger = NLog.LogManager.GetLogger("*");
        private static string INITIAL_BALANCE_TRANSACTION_ID = "99999999";
        private static int NEW_TRANSACTIONS_ENVELOPE_GROUP_ID = 998;

        private Database m_db;

        public TransactionImporter(Database db)
        {
            m_db = db;
        }

        public void ImportAllPending()
        {
            List<TransactionImport> pendingImports = m_db.Fetch<TransactionImport>("where transaction_imports_status_id = @0", (int)TransactionImportsStatusEnum.NOT_IMPORTED);
            foreach (var import in pendingImports)
            {
                ImportOfx(import);
            }
        }

        public void  ImportOfx(TransactionImport import)
        {
            Account acct = m_db.Single<Account>(import.account_id);
            ImportOfx(import, acct);
        }

        public void ImportOfx(TransactionImport import, Account acct)
        {
            if (import.transaction_imports_status_id == (int)TransactionImportsStatusEnum.NOT_IMPORTED)
            {
                OfxParser parser = new OfxParser();
                OfxAccount ofxAcct = parser.LoadAccount(import.ofx_response);

                import.transaction_imports_status_id = (int)TransactionImportsStatusEnum.IN_PROCESS;
                m_db.Save(import);

                try
                {
                    using (var scope = m_db.GetTransaction())
                    {
                        List<Transaction> recordedTransactions = null;

                        long count = m_db.ExecuteScalar<long>("SELECT count(*) FROM accounts WHERE id = @0", acct.id);
                        if (count == 0)
                        {
                            s_logger.Info("Will process as INITIAL update.  No transactions found for this account.");
                            recordedTransactions = RecordInitialTransactions(acct, ofxAcct, import.id);
                        }
                        else
                        {
                            s_logger.Info("Will process as SUBSEQUENT update.");
                            recordedTransactions = RecordNewTransactions(acct, ofxAcct, import.id);
                        }

                        decimal newTransactionAmountTotal = recordedTransactions.Sum(trans => trans.amount);
                        acct.ofx_balance = ofxAcct.LedgerBalanceAmount;
                        acct.ofx_balance_date = ofxAcct.LedgerBalanceAsOfDate;
                        acct.ofx_last_updated = DateTime.UtcNow;
                        acct.ofx_error_last_update = false;
                        acct.ofx_error_last_update_message = string.Empty;

                        s_logger.Info("Updating account.");
                        m_db.Save(acct);

                        scope.Complete();
                    }

                    import.status_detail = null;
                    import.transaction_imports_status_id = (int)TransactionImportsStatusEnum.IMPORTED;
                    m_db.Save(import);

                }
                catch (Exception ex)
                {
                    import.transaction_imports_status_id = (int)TransactionImportsStatusEnum.ERROR;
                    import.status_detail = ex.Message; //TODO: change this error text.
                    m_db.Save(import);
                }

                //run transaction filters
                s_logger.Info(string.Format("Running transaction filters for user id: {0}...", acct.user_id.ToString()));
                TransactionsFilterEngine tfe = new TransactionsFilterEngine(m_db);
                tfe.RunFilters(acct.user_id);
            }
        }

        private List<Transaction> RecordInitialTransactions(Account currentAcct, OfxAccount ofxAcct, int ofxImportId)
        {
            List<Transaction> recordedTransactions = new List<Transaction>();

            //clear transactions
            s_logger.Info("Deleting all transactions for this account.");
            m_db.Delete<Transaction>("WHERE account_id=@0", currentAcct.id);

            Envelope newTransactionsEnvelope = m_db.SingleOrDefault<Envelope>(
                  "WHERE user_id = @0 AND envelope_group_id = @1", currentAcct.user_id, NEW_TRANSACTIONS_ENVELOPE_GROUP_ID);

            //record initial transactions with $0.00 amount
            //so they won't be duplicated on subsequent fetches
            s_logger.Info("Recording initial balance transactions...");
            foreach (OfxTransaction trans in ofxAcct.Transactions)
            {
                Transaction initialBalanceTrans = new Transaction();
                initialBalanceTrans.account_id = currentAcct.id;
                initialBalanceTrans.user_id = currentAcct.user_id;
                initialBalanceTrans.transaction_imports_id = ofxImportId;
                initialBalanceTrans.ofx_transaction_id = trans.TransactionID;
                initialBalanceTrans.envelope_id = null;
                initialBalanceTrans.date = trans.Date;
                initialBalanceTrans.name = trans.Name;
                initialBalanceTrans.amount = 0.00M;

                s_logger.Info(string.Format("{0} {1} ({2}) - {3}", initialBalanceTrans.date, initialBalanceTrans.name, initialBalanceTrans.ofx_transaction_id, initialBalanceTrans.amount));
                m_db.Save(initialBalanceTrans);
                recordedTransactions.Add(initialBalanceTrans);
            }

            //ASSUMPTION: LedgerBalanceAmount is latest balance and includes 
            // amount of all ofxAcct.Transactions

            //record our official initial balance transaction
            Transaction initialTransaction = new Transaction();
            initialTransaction.account_id = currentAcct.id;
            initialTransaction.user_id = currentAcct.user_id;
            initialTransaction.envelope_id = newTransactionsEnvelope.id;
            initialTransaction.transaction_imports_id = ofxImportId;
            initialTransaction.ofx_transaction_id = INITIAL_BALANCE_TRANSACTION_ID;
            initialTransaction.date = ofxAcct.LedgerBalanceAsOfDate.Value;
            initialTransaction.name = "Initial Balance";
            initialTransaction.amount = ofxAcct.LedgerBalanceAmount.Value;

            s_logger.Info("Recording initial transaction.");
            s_logger.Info(string.Format("{0} {1} ({2}) - {3}", initialTransaction.date, initialTransaction.name, initialTransaction.ofx_transaction_id, initialTransaction.amount));

            m_db.Save(initialTransaction);

            recordedTransactions.Add(initialTransaction);

            return recordedTransactions;
        }

        private List<Transaction> RecordNewTransactions(Account currentAcct, OfxAccount ofxAcct, int ofxImportId)
        {
            List<Transaction> recordedTransactions = new List<Transaction>();
            if (ofxAcct.Transactions != null && ofxAcct.Transactions.Count() > 0)
            {
                s_logger.Info("Retrieving existing account transactions.");
                List<Transaction> existingTransactions = m_db.Fetch<Transaction>(
                    "WHERE account_id = @0 AND date >= @1", currentAcct.id, ofxAcct.Transactions.Min(t => t.Date.Date));

                Envelope newTransactionsEnvelope = m_db.SingleOrDefault<Envelope>(
                    "WHERE user_id = @0 AND envelope_group_id = @1", currentAcct.user_id, NEW_TRANSACTIONS_ENVELOPE_GROUP_ID);

                s_logger.Info("Queuing new transactions...");
                List<Transaction> newTransactions = new List<Transaction>();
                foreach (OfxTransaction ofxTrans in ofxAcct.Transactions)
                {
                    if (!existingTransactions.Exists(et => et.ofx_transaction_id == ofxTrans.TransactionID)) //prevent dupdes!
                    {
                        Transaction newTransaction = new Transaction();
                        newTransaction.account_id = currentAcct.id;
                        newTransaction.user_id = currentAcct.user_id;
                        newTransaction.envelope_id = newTransactionsEnvelope.id;
                        newTransaction.transaction_imports_id = ofxImportId;
                        newTransaction.ofx_transaction_id = ofxTrans.TransactionID;
                        newTransaction.date = ofxTrans.Date;
                        newTransaction.name = ofxTrans.Name;
                        newTransaction.amount = ofxTrans.Amount;

                        s_logger.Info(string.Format("{0} {1} ({2}) - {3}", newTransaction.date, newTransaction.name, newTransaction.ofx_transaction_id, newTransaction.amount));
                        newTransactions.Add(newTransaction);
                    }
                }

                s_logger.Info("Saving queued transactions...");
                foreach (Transaction newTrans in newTransactions)
                {
                    try
                    {
                        m_db.Save(newTrans);
                        recordedTransactions.Add(newTrans);
                    }
                    catch (Exception ex)
                    {
                        if (!ex.Message.Contains("Duplicate entry"))
                        {
                            //exception is not b/c of duplicate TransactionID from bank
                            s_logger.ErrorException(
                                string.Format("Exception when saving OFX_TransactionID:{0} on AccountID:{1} - {2}", newTrans.id, currentAcct.id, ex.Message),
                                ex);
                        }
                    }
                }

                s_logger.Info("Updating New Transactions envelope balance...");
            }

            return recordedTransactions;
        }
    }
}

