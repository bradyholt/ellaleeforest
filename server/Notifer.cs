using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PetaPoco;
using bude.server;
using System.Net.Mail;
using System.Configuration;

namespace bude.server
{
    public class Notifer
    {
        private static NLog.Logger s_logger = NLog.LogManager.GetLogger("*");
        private static int NEW_TRANSACTIONS_ENVELOPE_GROUP_ID = 998;
        private static int UNASSIGNED_TRANSACTION_NOTIFY_COUNT = 15;
        private Database m_db;

        public Notifer(Database db)
        {
            m_db = db;
        }

        public void SendNotificationForAllUsers()
        {
            List<User> users = m_db.FetchAll<User>();
            foreach (User user in users)
            {
                if (string.IsNullOrEmpty(user.email))
                {
                    continue;
                }

                SendNotifications(user, UNASSIGNED_TRANSACTION_NOTIFY_COUNT);
            }
        }

        private void SendNotifications(User user, int unassignedTransactionNotifyCount)
        {

            Envelope newTransactionsEnvelope = m_db.SingleOrDefault<Envelope>(
              "WHERE user_id = @0 AND envelope_group_id = @1", user.id, NEW_TRANSACTIONS_ENVELOPE_GROUP_ID);

            List<Transaction> unassignedTransactions = m_db.Fetch<Transaction>("WHERE envelope_id = @0 AND parent_transaction_id IS NULL", newTransactionsEnvelope.id.ToString());

            if (unassignedTransactions.Count >= unassignedTransactionNotifyCount)
            {
                MailMessage mailMessage = new MailMessage(ConfigurationManager.AppSettings["emailFrom"], user.email);
                mailMessage.Subject = string.Format("You have {0} transactions needing assignment.", unassignedTransactions.Count.ToString());
                mailMessage.IsBodyHtml = true;
                string body = "<table><tr><td><strong>Date</strong></td><td><strong>Name</strong></td><td><strong>Amount</strong></td></tr>";
                foreach (Transaction trans in unassignedTransactions)
                {
                    body += string.Format("<tr><td>{0}</td><td>{1}</td><td>{2}</td></tr>", 
                        trans.date.ToShortDateString(), 
                        trans.name,
                        trans.amount.ToString("N"));
                }

                body += "</table>";
                body += string.Format("<p><a href=\"{0}\">Click Here To Login</a></p>", ConfigurationManager.AppSettings["webUrl"]);

                mailMessage.Body = body;

                SmtpClient mailClient = new SmtpClient();
                s_logger.Info("Sending notification to: " + user.email);

                try
                {
                    mailClient.Send(mailMessage);
                }
                catch (Exception ex)
                {
                    s_logger.ErrorException("Error sending email notification.", ex);
                }
            }
        }
    }
}
