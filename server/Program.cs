using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using NLog;
using System.ServiceProcess;
using PetaPoco;

namespace bude.server
{
    class Program
    {
        private static Logger s_logger = NLog.LogManager.GetLogger("*");

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            System.Net.ServicePointManager.ServerCertificateValidationCallback = CertificateValidator;

            Database s_db = new PetaPoco.Database(
                ConfigurationManager.ConnectionStrings["BUDE_PRODUCTION"].ConnectionString, 
                ConfigurationManager.ConnectionStrings["BUDE_PRODUCTION"].ProviderName);

            TransactionsFilterEngine tfe = new TransactionsFilterEngine(s_db);
            s_logger.Info("Running filters for all users...");
            tfe.RunFiltersForAllUsers();

            TransactionFetcher tf = new TransactionFetcher(s_db);
            s_logger.Info("Fetching transactions for all users...");
            tf.FetchAndSaveNewTransactions();

            Notifer notifier = new Notifer(s_db);
            notifier.SendNotificationForAllUsers();

            TransactionImporter importer = new TransactionImporter(s_db);
            s_logger.Info("Running all pending imports.");
            importer.ImportAllPending();
            
            Console.WriteLine("Done.");
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = null;
            if (e.ExceptionObject is Exception)
            {
                ex = (Exception)e.ExceptionObject;
            }

            s_logger.FatalException("Unhandled Exception in AppDomain", ex);
        }

        public static bool CertificateValidator(object sender,
      System.Security.Cryptography.X509Certificates.X509Certificate certificate,
      System.Security.Cryptography.X509Certificates.X509Chain chain,
      System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            //trust all certificates
            return true;
        }
    }
}
