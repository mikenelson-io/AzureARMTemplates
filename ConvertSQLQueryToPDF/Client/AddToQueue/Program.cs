using System;
using System.Linq;
using System.Configuration;
using System.Data;
using System.Data.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using AddToQueue.Model;

namespace AddToQueue
{
    class Program
    {
        static string queueName = "messages";
        static string queueConnctionString;
        static CloudQueueClient queueClient;
        static CloudStorageAccount queueAccount;
        static CloudQueue queue;
        private static Random random = new Random();

        static void Main(string[] args)
        {
            if (args.Length != 1) {
                Console.WriteLine("You must provide a the number of items to add to the" + queueName + " queue.");
                return;
            }

            queueConnctionString = ConfigurationManager.AppSettings["QueueConnectionString"];
            queueAccount = CloudStorageAccount.Parse(queueConnctionString);
            queueClient = queueAccount.CreateCloudQueueClient();
            queue = queueClient.GetQueueReference(queueName);
            queue.CreateIfNotExistsAsync();

            for (int i = 0; i < Convert.ToInt32(args[0]); i++) 
            {
                AddtoAzureQueue();
            }
        }

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static decimal RandomDecimal()
        {
            var rnd = Math.Round((Convert.ToDouble(random.Next(100)) + random.NextDouble()),2);
            return Convert.ToDecimal(rnd);
        }

        static void AddtoAzureQueue()
        {
            var id = random.Next(0, 1000000);

            var con = ConfigurationManager.ConnectionStrings["SQLDB"].ConnectionString;
            using( DataContext db = new DataContext(con) ) {
				Table<OrderViewModel> tbl = db.GetTable<OrderViewModel>();
				var order = new OrderViewModel
				{
					OrderId = id,
					OrderDate = DateTime.Now,
					FirstName = RandomString(5),
					LastName = RandomString(15),
					Address = "1 Main Street",
					City = "Chicago",
					State = "IL",
					Country = "USA",
					PostalCode = "123456",
					Phone = "555-555-1234",
					Email = "a@b.com",
					Total = RandomDecimal()
				};
				tbl.InsertOnSubmit(order);
				db.SubmitChanges();
            }

            CloudQueueMessage message = new CloudQueueMessage(id.ToString());
			queue.AddMessage(message);            
        }
    }
}