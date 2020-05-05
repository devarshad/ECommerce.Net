using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using SendGrid.Helpers.Mail;

namespace AzureFunctions
{
    public static class CleanerFunction
    {
        [FunctionName("Cleaner")]
        public static void Run([TimerTrigger("0 */3 * * * *")]TimerInfo myTimer,
            [Blob("images", Connection = "AzureWebJobsStorage")]CloudBlobContainer images,
            [Blob("thumbnails", Connection = "AzureWebJobsStorage")]CloudBlobContainer thumbnails,
            [SendGrid(ApiKey = "SendgridConfiguration", From = "marshad4uk@gmail.com", To = "marshad4uk@gmail.com", Subject = "Azure Cleanup process")]out SendGridMessage message,
            ILogger log)
        {
            message = new SendGridMessage();
            var listAttachment = new List<Attachment>();
            listAttachment.AddRange(CleanUp(images, log).ConfigureAwait(false).GetAwaiter().GetResult());
            listAttachment.AddRange(CleanUp(thumbnails, log).ConfigureAwait(false).GetAwaiter().GetResult());
            if (listAttachment.Count > 0)
            {
                message.AddAttachments(listAttachment);
                message.AddContent("text/html", "Please find the attached list of files after cleanup.");
            }
            else
            {
                message.AddContent("text/html", "No file foud for cleanup.");
            }
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        }

        private static async Task<IEnumerable<Attachment>> CleanUp(CloudBlobContainer container, ILogger log)
        {
            var list = (await container.ListBlobsSegmentedAsync(null)).Results;
            
            var listAttachment = new List<Attachment>();
            var stream = new MemoryStream();
            foreach (var item in list)
            {
                var name = item.Uri.Segments.Last();
                var blockBlob = container.GetBlockBlobReference(name);
                await blockBlob.DownloadToStreamAsync(stream);
                listAttachment.Add(new Attachment() { Content = Convert.ToBase64String(stream.ToArray()), Filename = name });

                log.LogInformation($"Deleting: {item.Uri}");
                await blockBlob.DeleteAsync();
            }
            return listAttachment;
        }
    }
}
