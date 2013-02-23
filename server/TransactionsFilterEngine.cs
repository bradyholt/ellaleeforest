using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PetaPoco;
using System.Transactions;

namespace bude.server
{
    public class TransactionsFilterEngine
    {
        private static NLog.Logger s_logger = NLog.LogManager.GetLogger("*");
        private static int NEW_TRANSACTIONS_ENVELOPE_GROUP_ID = 998;

        private Database m_db;

        public TransactionsFilterEngine(Database db)
        {
            m_db = db;
        }

        public void RunFilters(int userid)
        {
            s_logger.Info("Queuing transaction filters for userid=" + userid.ToString());

            Envelope newTransactionEnvelope = m_db.SingleOrDefault<Envelope>(
                 "WHERE user_id = @0 AND envelope_group_id = @1", userid, NEW_TRANSACTIONS_ENVELOPE_GROUP_ID);

            RunFilters(new List<Envelope>() { newTransactionEnvelope });
        }

        public void RunFiltersForAllUsers()
        {
            List<Envelope> newTransactionEnvelopes = m_db.Fetch<Envelope>(
                   "WHERE envelope_group_id = @0", NEW_TRANSACTIONS_ENVELOPE_GROUP_ID);

            RunFilters(newTransactionEnvelopes);
        }

        public void RunFilters(List<Envelope> envelopes)
        {
            s_logger.Info("Running transaction filters.");

            foreach (Envelope env in envelopes)
            {
                s_logger.Info("user id: " + env.user_id);

                List<Transaction> transactions = m_db.Fetch<Transaction>("WHERE envelope_id = @0", env.id);
                List<TransactionFilter> filters = m_db.Fetch<TransactionFilter>("WHERE user_id = @0", env.user_id);

                if (transactions.Count > 0 && filters.Count > 0)
                {
                    RunEnvelopeFilters(env, transactions, filters);
                }
            }
        }

        private List<Transaction> RunEnvelopeFilters(Envelope sourceEnvelope, List<Transaction> transactions, List<TransactionFilter> allFilters)
        {
            foreach (Transaction trans in transactions)
            {
                var matchFilters = allFilters.Where(f => trans.name.Contains(f.search_text));
                foreach (TransactionFilter filter in matchFilters)
                {
                    if (filter.amount == null || Math.Abs(filter.amount.Value) == Math.Abs(trans.amount))
                    {
                        s_logger.Info(string.Format("MATCH - '{0}' being assigned to envelope id: {1}", trans.name, filter.envelope_id));

                        trans.envelope_id = filter.envelope_id;
                        trans.notes = "Auto-assigned by filter";
                        m_db.Save(trans);

                        break;
                    }
                }
            }

            return transactions;
        }
    }

}
