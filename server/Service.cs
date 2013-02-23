using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using NLog;
using RabbitMQ.Client;
using PetaPoco;
using RabbitMQ.Client.Events;
using System.Threading;
using RabbitMQ.Client.Exceptions;
using System.Configuration;

namespace bude.server
{
    partial class Service : ServiceBase
    {
        private static Logger s_logger = NLog.LogManager.GetLogger("*");
        private const string QUEUE_TRANSACTIONS_FETCH = "bude_transactions";
        private const string QUEUE_FILTERS = "bude_filters";
        private const string QUEUE_NOTIFICATIONS = "bude_notifications";
        private const string QUEUE_TRANSACTION_IMPORT = "bude_import";
        private static PetaPoco.Database s_db;
        private static ConnectionFactory s_factory;
        private static IConnection s_connection;
        private static IModel s_channel;

        public Service()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Startup();
        }

        protected override void OnStop()
        {
            Shutdown();
        }

        public void Shutdown()
        {
            s_channel.Close();
            s_connection.Close();
        }

        public void Startup()
        {
            s_logger.Info("Initializing...");
            System.Net.ServicePointManager.ServerCertificateValidationCallback = CertificateValidator;

            s_db = new PetaPoco.Database(ConfigurationManager.ConnectionStrings["BUDE_PRODUCTION"].ConnectionString, ConfigurationManager.ConnectionStrings["BUDE_PRODUCTION"].ProviderName);
            s_factory = new ConnectionFactory { HostName = ConfigurationManager.AppSettings["messagingBrokerHostName"] };

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            InitializeChannel();
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            s_logger.Fatal(e.ExceptionObject);
        }

        void InitializeChannel()
        {
            try
            {
                s_connection = s_factory.CreateConnection();
                s_channel = s_connection.CreateModel();
                s_channel.ModelShutdown += new ModelShutdownEventHandler(channel_ModelShutdown);

                //filters
                string filterQueue = s_channel.QueueDeclare(QUEUE_FILTERS, true, false, false, null);
                EventingBasicConsumer filterConsumer = new EventingBasicConsumer();
                filterConsumer.Model = s_channel;
                filterConsumer.Received += new BasicDeliverEventHandler(filterConsumer_Received);
                s_channel.BasicConsume(filterQueue, true, filterConsumer);

                //transactions fetch
                string transactionsQueue = s_channel.QueueDeclare(QUEUE_TRANSACTIONS_FETCH, true, false, false, null);
                EventingBasicConsumer transactionConsumer = new EventingBasicConsumer();
                transactionConsumer.Model = s_channel;
                transactionConsumer.Received += new BasicDeliverEventHandler(transactionConsumer_Received);
                s_channel.BasicConsume(transactionsQueue, true, transactionConsumer);

                //notifications
                string notificationsQueue = s_channel.QueueDeclare(QUEUE_NOTIFICATIONS, true, false, false, null);
                EventingBasicConsumer notificationsConsumer = new EventingBasicConsumer();
                notificationsConsumer.Model = s_channel;
                notificationsConsumer.Received += new BasicDeliverEventHandler(notificationsConsumer_Received);
                s_channel.BasicConsume(notificationsQueue, true, notificationsConsumer);

                //transactions import
                string transactionImportQueue = s_channel.QueueDeclare(QUEUE_TRANSACTION_IMPORT, true, false, false, null);
                EventingBasicConsumer transactionImportConsumer = new EventingBasicConsumer();
                transactionImportConsumer.Model = s_channel;
                transactionImportConsumer.Received += new BasicDeliverEventHandler(transactionImportConsumer_Received);
                s_channel.BasicConsume(transactionImportQueue, true, transactionImportConsumer);

                s_logger.Info("Waiting for messages...");
            }
            catch (BrokerUnreachableException bu)
            {
                s_logger.ErrorException("Server could not be reached.", bu);
                s_logger.Info("Waiting 10 seconds to reconnect...");
                Thread.Sleep(10000);
                s_logger.Info("Attempting reconnect...");
                InitializeChannel();
            }
        }

        public static bool CertificateValidator(object sender,
         System.Security.Cryptography.X509Certificates.X509Certificate certificate,
         System.Security.Cryptography.X509Certificates.X509Chain chain,
         System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            //trust all certificates
            return true;
        }

        void channel_ModelShutdown(IModel model, ShutdownEventArgs reason)
        {
            s_logger.Error(string.Format("Server shutdown (reason: {0})", reason));
            s_logger.Info("Attempting reconnect...");
            InitializeChannel();
        }

        protected void transactionConsumer_Received(IBasicConsumer sender, BasicDeliverEventArgs args)
        {
            string content = (Encoding.ASCII.GetString(args.Body));
            s_logger.Info(string.Format("Message '{0}' received on queue '{1}.'", content, args.RoutingKey));

            TransactionFetcher tf = new TransactionFetcher(s_db);

            if (content == "all")
            {
                s_logger.Info("Fetching transactions for all users...");
                tf.FetchAndSaveNewTransactions();
            }
            else
            {
                int accountid = Convert.ToInt32(content);
                s_logger.Info("Fetching transactions for accountid:" + accountid.ToString());
                tf.FetchAndSaveNewTransactions(accountid);
            }

            if (!string.IsNullOrEmpty(args.BasicProperties.ReplyTo))
            {
                lock (s_channel)
                {
                    s_channel.BasicPublish(string.Empty, args.BasicProperties.ReplyTo, null,
                        System.Text.Encoding.UTF8.GetBytes("done"));
                }
            }
        }

        protected void filterConsumer_Received(IBasicConsumer sender, BasicDeliverEventArgs args)
        {
            string content = (Encoding.ASCII.GetString(args.Body));
            s_logger.Info(string.Format("Message '{0}' received on queue '{1}.'", content, args.RoutingKey));

            TransactionsFilterEngine tfe = new TransactionsFilterEngine(s_db);

            if (content == "all")
            {
                s_logger.Info("Running filters for all users...");
                tfe.RunFiltersForAllUsers();
            }
            else
            {
                int userid = Convert.ToInt32(content);
                s_logger.Info("Running filters for userid:" + userid.ToString());
                tfe.RunFilters(userid);
            }

            if (!string.IsNullOrEmpty(args.BasicProperties.ReplyTo))
            {
                lock (s_channel)
                {
                    s_channel.BasicPublish(string.Empty, args.BasicProperties.ReplyTo, null,
                         System.Text.Encoding.UTF8.GetBytes("done"));
                }
            }
        }

        protected void notificationsConsumer_Received(IBasicConsumer sender, BasicDeliverEventArgs args)
        {
            string content = (Encoding.ASCII.GetString(args.Body));
            s_logger.Info(string.Format("Message '{0}' received on queue '{1}.'", content, args.RoutingKey));

            Notifer notifier = new Notifer(s_db);

            if (content == "all")
            {
                notifier.SendNotificationForAllUsers();
            }
        }

        protected void transactionImportConsumer_Received(IBasicConsumer sender, BasicDeliverEventArgs args)
        {
            string content = (Encoding.ASCII.GetString(args.Body));
            s_logger.Info(string.Format("Message '{0}' received on queue '{1}.'", content, args.RoutingKey));

            TransactionImporter importer = new TransactionImporter(s_db);

            if (content == "all")
            {
                s_logger.Info("Running all pending imports.");
                importer.ImportAllPending();

            }
            else
            {
                int id = Convert.ToInt32(content);
                s_logger.Info(string.Concat("Running transaction import for transaction_imports.id = ", id));
                TransactionImport import = s_db.Single<TransactionImport>(id);
                importer.ImportOfx(import);
            }

        }

    }
}
