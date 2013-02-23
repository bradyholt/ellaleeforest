using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using PetaPoco;
using MySql.Data.MySqlClient;

namespace bude.server
{
    public class TransactionFetcher
    {
        private static NLog.Logger s_logger = NLog.LogManager.GetLogger("*");

        private Database m_db;

        public TransactionFetcher(Database db)
        {
            m_db = db;
        }

        public void FetchAndSaveNewTransactions()
        {
            List<Account> accts = m_db.FetchJoined<Account, Bank>("bank_id", "WHERE ofx_enabled = 1", null);
            s_logger.Info(string.Format("{0} accounts found to update.", accts.Count));
            FetchAndSaveNewTransactions(accts);
        }

        public void FetchAndSaveNewTransactions(int accountID)
        {
            List<Account> acct = m_db.FetchJoined<Account, Bank>("bank_id", "WHERE accounts.id = @0 AND ofx_enabled = 1", accountID);
            FetchAndSaveNewTransactions(new List<Account>() { acct.First() });
        }

        public void FetchAndSaveNewTransactions(List<Account> accts)
        {
            foreach (Account currentAcct in accts)
            {
                s_logger.Info(string.Format("Fetching for account id: {0}", currentAcct.id));

                try
                {
                    IFetchOfx fetcher = null;
                    switch (currentAcct.Bank.transaction_fetch_method_id)
                    {
                        case (int)TransactionFetchMethodEnum.OFX_Direct:
                            s_logger.Info("Using Direct fetcher.");
                            fetcher = new DirectFetcher(currentAcct);
                            break;
                        case (int)TransactionFetchMethodEnum.OFX_Scrape:
                            s_logger.Info("Using Scrape fetcher.");
                            fetcher = new ScrapeFetcher(currentAcct);
                            break;
                        default:
                            throw new NotImplementedException();
                    }

                    s_logger.Info("Starting fetch...");

                    OfxAccount ofxAcct = fetcher.FetchOfx(DateTime.UtcNow.AddDays(-7), DateTime.UtcNow);

                    TransactionImport ofxImport = new TransactionImport();
                    ofxImport.account_id = currentAcct.id;
                    ofxImport.user_id = currentAcct.user_id;
                    ofxImport.ofx_request = fetcher.Request;
                    ofxImport.ofx_response = fetcher.Response;

                    s_logger.Info(string.Format("Finished fetching. {0} transactions retrieved.", ofxAcct.Transactions.Count()));

                    TransactionImporter importer = new TransactionImporter(m_db);
                    importer.ImportOfx(ofxImport, currentAcct);
                }
                catch (FetchException fex)
                {
                    s_logger.ErrorException(
                       string.Format("FetchException when updating AccountID:{0} - {1}", currentAcct.id, fex.Message), fex);

                    currentAcct.ofx_error_last_update = true;
                    currentAcct.ofx_error_last_update_message = fex.Message;
                    m_db.Save(currentAcct);
                }
                catch (Exception ex)
                {
                    s_logger.ErrorException(
                        string.Format("Error when updating AccountID:{0} - {1}", currentAcct.id, ex.Message), ex);

                    currentAcct.ofx_error_last_update = true;
                    m_db.Save(currentAcct);
                }

                s_logger.Info(string.Format("Finished with account id: {0}", currentAcct.id));
            }
        }
    }
}
