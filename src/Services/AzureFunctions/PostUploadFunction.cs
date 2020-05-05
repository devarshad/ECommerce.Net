using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AzureFunctions
{
    public static class PostUploadFunction
    {
        [FunctionName("PostUpload")]
        public static void Run([BlobTrigger("images/{name}", Connection = "AzureWebJobsStorage")]Stream myBlob,
            [Blob("thumbnails/{name}",FileAccess.Write,Connection = "AzureWebJobsStorage")]Stream outBlob,
            string name, ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

            Image image = Image.FromStream(myBlob);
            Image thumb = image.GetThumbnailImage(120, 120, () => false, IntPtr.Zero);
            try
            {
                var newStream = new MemoryStream();
                thumb.Save(newStream, image.RawFormat);
                outBlob.Write(newStream.ToArray(), 0, newStream.ToArray().Length);

            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
