using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace dispatcher
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: bude-dispatcher.exe [queueName] [messagebody]");
            }
            else
            {
                Console.Write("Connecting to broker...");
                try
                {
                    ConnectionFactory factory = new ConnectionFactory { HostName = ConfigurationManager.AppSettings["messagingBrokerHostName"] };
                    using (IConnection conn = new ConnectionFactory().CreateConnection())
                    {
                        Console.WriteLine("done.");

                        using (IModel ch = conn.CreateModel())
                        {
                            Console.Write(string.Format("Publishing message '{0}' to queue '{1}' ...", args[1], args[0]));

                            IBasicProperties props = ch.CreateBasicProperties();
                            ch.BasicPublish(string.Empty, args[0], props, System.Text.Encoding.UTF8.GetBytes(args[1]));

                            Console.WriteLine("done.");
                        }
                    }
                }
                catch (BrokerUnreachableException)
                {
                    Console.WriteLine(string.Format("Error: Broker could not be reached. (HostName={0})", ConfigurationManager.AppSettings["messagingBrokerHostName"]));
                }
                
            }
        }
    }
}
