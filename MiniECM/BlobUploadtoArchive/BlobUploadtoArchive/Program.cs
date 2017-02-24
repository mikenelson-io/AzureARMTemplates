using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Configuration;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.DataMovement;
using Microsoft.WindowsAzure.Storage.Queue;

namespace BlobUploadtoArchive
{
    class Program
    {
        static string containerName = "processing";
        static string queueName = "messages";

        static string uploadStorageConnectionString;
        static string queueConnctionString;

        static CloudBlobClient blobClient;
        static CloudQueueClient queueClient;

        static CloudStorageAccount archiveAccount;
        static CloudStorageAccount queueAccount;

        static CloudBlobContainer container;
        static CloudQueue queue;

        static void Main(string[] args)
        {
            if (args.Length != 1) {
                Console.WriteLine("You must provide a directory argument at the command line.");
                return;
            }

            uploadStorageConnectionString = ConfigurationManager.AppSettings["UploadConnectionString"];
            queueConnctionString = ConfigurationManager.AppSettings["QueueConnectionString"];

            archiveAccount = CloudStorageAccount.Parse(uploadStorageConnectionString);
            queueAccount = CloudStorageAccount.Parse(queueConnctionString);

            blobClient = archiveAccount.CreateCloudBlobClient();
            queueClient = queueAccount.CreateCloudQueueClient();

            container = blobClient.GetContainerReference(containerName);
            container.CreateIfNotExistsAsync();

            queue = queueClient.GetQueueReference(queueName);
            queue.CreateIfNotExistsAsync();

            TransferManager.Configurations.ParallelOperations = 64;

            var path = args[0].ToString();
            if (!Directory.Exists(path)) {
                throw new System.IO.DirectoryNotFoundException(path);
            }

            var files = Directory.EnumerateFiles(path);
            Parallel.ForEach(files, file => { UploadBlobToAzure(file); });
            /*foreach( string file in files)
            {
                UploadBlobToAzure(file);
            }*/

        }

        static void UploadBlobToAzure(string file)
        {
            FileInfo fileRef = new FileInfo(file);
            CloudBlockBlob destBlob = container.GetBlockBlobReference(fileRef.Name.ToLower());
            TransferContext context = new TransferContext();
            var task = TransferManager.UploadAsync(fileRef.FullName, destBlob, null, context, CancellationToken.None);
            task.Wait();

            CloudQueueMessage message = new CloudQueueMessage(fileRef.Name);
            task = queue.AddMessageAsync(message);
            task.Wait();
        }
    }
}
