using System;
using System.Text;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

public class Record
{
    public string PartitionKey { get; set; }  //Range of the File Name 1-30/30-70/70-100
    public string RowKey       { get; set; }  //GUID of the File Name
    public string RecordName   { get; set; }  //Original File Name
    public string RecordUrl    { get; set; }  //Storage Account + Storage Account
}

public static void Run(string queuedMessage, Stream inputBlob, ICollector<Record> outputTable, TraceWriter log)
{
    string uploadSAContainerName = "processing";
    string archivedSAContainerName = "processed";
    Guid archivedFileName = Guid.NewGuid();
    
    CloudStorageAccount uploadSAccount;
    CloudBlobContainer  uploadSAContainer;
    CloudBlobClient     uploadBlobClient;
    string uploadSAConnectionString = System.Environment.GetEnvironmentVariable("bjdecmuploadsa002_STORAGE", EnvironmentVariableTarget.Process);
    
    uploadSAccount = CloudStorageAccount.Parse(uploadSAConnectionString);
    uploadBlobClient = uploadSAccount.CreateCloudBlobClient();
    uploadSAContainer = uploadBlobClient.GetContainerReference(uploadSAContainerName);
    
    int index;
    string partition = queuedMessage.Split('.')[0];
    bool success = int.TryParse(partition, out index);

    int key = 2;
    if( index < 30 ) {
        key = 0;
    }
    else if( index > 31 && index < 69 ) {
        key = 1;
    }

    CloudStorageAccount archiveAccount;
    CloudBlobContainer  archiveSAContainer;
    CloudBlobClient     archiveBlobClient;
	string archiveSAConnectionString;
	
    StringBuilder builder = new StringBuilder();
    string appSetting = (builder.AppendFormat("bjdecmarchivesa00{0}_STORAGE", key)).ToString();
    
    archiveSAConnectionString = System.Environment.GetEnvironmentVariable(appSetting, EnvironmentVariableTarget.Process);
    archiveAccount = CloudStorageAccount.Parse(archiveSAConnectionString);
    archiveBlobClient = archiveAccount.CreateCloudBlobClient();
    archiveSAContainer = archiveBlobClient.GetContainerReference(archivedSAContainerName);

    StringBuilder url = new StringBuilder();
    url.AppendFormat("{0}{1}/{2}", archiveAccount.FileStorageUri.PrimaryUri, archivedSAContainerName, archivedFileName.ToString());
    
    CloudBlockBlob destBlob = archiveSAContainer.GetBlockBlobReference(archivedFileName.ToString());
    var task = destBlob.UploadFromStreamAsync(inputBlob);
    task.Wait();
    
    outputTable.Add(
        new Record() {
            PartitionKey = partition,
            RowKey = archivedFileName.ToString(),
            RecordName = queuedMessage,
            RecordUrl = url.ToString()
        }
    );

    CloudBlockBlob srcBlob = uploadSAContainer.GetBlockBlobReference(queuedMessage);
    task = srcBlob.DeleteIfExistsAsync();
    task.Wait();
}

