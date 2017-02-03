#r "System.Data.Linq"

#load "OrderViewModel.csx"
#load "GenerateReceiptPDF.csx"
 
using System;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Net;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Configuration;

public static void Run(string PDFItem, TraceWriter log)
{
    log.Info($"C# Queue trigger function processed: {PDFItem}");
    var con = System.Environment.GetEnvironmentVariable("SQL_SERVER_CONNSTR", EnvironmentVariableTarget.Process);
    
    DataContext db = new DataContext(con);     
    Table<OrderViewModel> orderTable = db.GetTable<OrderViewModel>();        
    var order = (from OrderViewModel in orderTable
                where OrderViewModel.OrderId == Convert.ToInt32(PDFItem) 
                select OrderViewModel).FirstOrDefault();

    byte[] pdf = CreatePdfReport(order);

    string pdfSAContainerName = "processed";
    string pdfFileName = Guid.NewGuid().ToString() + ".pdf";

    CloudStorageAccount pdfSAccount;
    CloudBlobContainer  pdfSAContainer;
    CloudBlobClient     pdfBlobClient;
	string              pdfSAConnectionString;
	   
    pdfSAConnectionString = System.Environment.GetEnvironmentVariable("bjdpdffuncsa001_STORAGE", EnvironmentVariableTarget.Process);
    pdfSAccount = CloudStorageAccount.Parse(pdfSAConnectionString);
    pdfBlobClient = pdfSAccount.CreateCloudBlobClient();
    pdfSAContainer = pdfBlobClient.GetContainerReference(pdfSAContainerName);

    StringBuilder url = new StringBuilder();
    url.AppendFormat("{0}{1}/{2}", pdfSAccount.FileStorageUri.PrimaryUri, pdfSAContainerName, pdfFileName);
    
    CloudBlockBlob destBlob = pdfSAContainer.GetBlockBlobReference(pdfFileName);
    var task = destBlob.UploadFromByteArrayAsync(pdf);
    task.Wait();
}