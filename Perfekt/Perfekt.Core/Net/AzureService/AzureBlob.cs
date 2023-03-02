using Azure;
using Azure.Storage.Blobs;
using System.Diagnostics;
using System.Net;

namespace Perfekt.Core.Net.AzureService
{
    public sealed class AzureBlob
    {
        public NetworkCredential Credentials { get; }

        public string Container { get; set; }

        public string EndpointSuffix { get; set; }

        public string WebProtocol { get; set; }


        public AzureBlob(string container)
        {
            Credentials = new NetworkCredential();

            EndpointSuffix = "core.windows.net";
            WebProtocol = "https";

            Container = container;
        }

        public AzureBlob(string user, string password, string container) :
            this(container)
        {
            Credentials.UserName = user;
            Credentials.Password = password;
        }


        public string GetConnectionString()
        {
            return $"DefaultEndpointsProtocol={WebProtocol};" +
                   $"AccountName={Credentials.UserName};" +
                   $"Key={Credentials.Password};" +
                   $"EndpointSuffix={EndpointSuffix};";
        }

        public bool Create()
        {
            return TryCreateConnection();
        }

        private bool TryCreateConnection()
        {
            try
            {
                BlobContainerClient container = GetContainerClient();

                return container.Exists();
            }
            catch (RequestFailedException ex)
            {
                // TODO: implement default log procedures.
                Debug.Print(ex.Message);
                return false;
            }
        }

        #region File Upload
        public bool Upload(BlobContainerClient blobContainerClient, FileInfo fileInfo, TimeSpan fileTimeout)
        {
            // TODO: implement debug trace info

            var blobClient = blobContainerClient.GetBlobClient(fileInfo.Name);
            var taskResult = Task.Run(async () => await blobClient.UploadAsync(fileInfo.Name, true));
            if (!taskResult.Wait(fileTimeout))
            {
                return false;
            }

            if (taskResult.Status == TaskStatus.RanToCompletion)
            {
                var archivePath = Path.Combine(Path.GetDirectoryName(fileInfo.FullName) ?? string.Empty, "Archive");
                Directory.CreateDirectory(archivePath);

                return TryFileMove(fileInfo, archivePath) != -1;
            }

            return false;
        }

        public void Upload(BlobContainerClient blobContainerClient, DirectoryInfo dirInfo, TimeSpan fileTimeout)
        {
            var files = dirInfo.EnumerateFiles("*.csv").Where(f => !f.Name.Contains("Uploaded"));
            foreach (var fInfo in files)
            {
                if (!Upload(blobContainerClient, fInfo, fileTimeout))
                {
                    // TODO: implement failed debug trace
                }
            }
        }

        private long TryFileMove(FileInfo fileInfo, string archivePath)
        {
            try
            {
                File.Move(fileInfo.FullName, Path.Combine(archivePath, "Uploaded-" + fileInfo.Name));

                return fileInfo.Length;
            }
            catch (Exception e)
            {
                // TODO: implement failed debug trace
            }

            return -1;
        }
        #endregion

        #region File Download

        public IEnumerable<(string, byte[])> GetBlobs(TimeSpan fileTimeout)
        {
            var blobs = ListContents();
            var containerClient = GetContainerClient();

            foreach (var blobName in blobs)
            {
                yield return GetBlob(containerClient, blobName, fileTimeout);
            }
        }

        public (string, byte[]) GetBlob(BlobContainerClient blobContainer, string file, TimeSpan fileTimeout)
        {
            var blobClient = blobContainer.GetBlobClient(file);
            if (blobClient.Exists())
            {
                var taskResult = Task.Run(async () => await blobClient.DownloadContentAsync());
                if (!taskResult.Wait(fileTimeout))
                {
                    // TODO: implement failed debug trace
                    return (file, Array.Empty<byte>());
                }

                if (taskResult.Status == TaskStatus.RanToCompletion)
                {
                    var blob = taskResult.Result.Value.Content.ToArray();
                    return (file, blob);
                }
            }

            return (string.Empty, Array.Empty<byte>());
        }

        #endregion

        public IEnumerable<string> ListContents()
        {
            var client = GetContainerClient();
            foreach (var blobItem in client.GetBlobs())
            {
                yield return blobItem.Name;
            }
        }

        private BlobContainerClient GetContainerClient()
        {
            if (string.IsNullOrEmpty(Container))
            {
                throw new NullReferenceException(nameof(Container) + " cannot be null.");
            }

            var serviceClient = new BlobServiceClient(GetConnectionString());
            return serviceClient.GetBlobContainerClient(Container);
        }
    }
}
