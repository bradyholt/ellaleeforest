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

#if DEBUG
            Service service = new Service();
          //  service.Startup();

            Database s_db = new PetaPoco.Database(
                ConfigurationManager.ConnectionStrings["BUDE_PRODUCTION"].ConnectionString.Replace("localhost", "www.veritasclass.com"), 
                ConfigurationManager.ConnectionStrings["BUDE_PRODUCTION"].ProviderName);
            
            //TransactionsFilterEngine tfe = new TransactionsFilterEngine(s_db);
            //tfe.RunFilters(2);

            //TransactionFetcher tf = new TransactionFetcher(s_db);
            //tf.FetchAndSaveNewTransactions();

            //Notifer notifier = new Notifer(s_db);
            //notifier.SendNotificationForAllUsers();

            TransactionImporter importer = new TransactionImporter(s_db);
            TransactionImport import = s_db.Single<TransactionImport>(3708);
            importer.ImportOfx(import);
         
            Console.WriteLine("Debug mode: Press any key to exit...");
            Console.ReadLine();
           // service.Shutdown();
#else
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
			{ 
				new Service() 
			};
            ServiceBase.Run(ServicesToRun);
#endif
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
    }
}
