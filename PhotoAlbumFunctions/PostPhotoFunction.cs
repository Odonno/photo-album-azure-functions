using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PhotoAlbumFunctions.Data;
using System.Linq;
using PhotoAlbumFunctions.Models;
using System.Collections.Generic;
using System;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;

namespace PhotoAlbumFunctions
{
    public class PostPhotoFunction
    {
        private readonly PhotoAlbumContext _context;
        private readonly string _blobStorageConnectionString;

        public PostPhotoFunction(PhotoAlbumContext context, IConfiguration configuration)
        {
            _context = context;
            _blobStorageConnectionString = configuration.GetConnectionString("BlobStorage");
        }

        [FunctionName(nameof(PostPhotoFunction))]
        public async Task<PhotoResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "photos")] HttpRequest req,
            ILogger log
        )
        {
            var file = req.Form.Files.First();

            // Create the container and return a container client object
            var containerClient = await GetPhotosBlobContainer();

            // Get a reference to a blob
            var blobClient = containerClient.GetBlobClient(file.FileName);

            // Open the file and upload its data            
            using var uploadFileStream = file.OpenReadStream();
            var blob = await blobClient.UploadAsync(uploadFileStream, true);
            uploadFileStream.Close();

            // Save photo in database
            var photo = new Photo
            {
                Url = blobClient.Uri.AbsoluteUri,
                CreatedAt = DateTime.Now,
                Tags = new List<Tag>()
            };

            _context.Photos.Add(photo);
            await _context.SaveChangesAsync();

            return new PhotoResult
            {
                Id = photo.Id,
                Url = photo.Url,
                CreatedAt = photo.CreatedAt,
                Tags = photo.Tags.Select(t => t.Value).ToList()
            };
        }

        private async Task<BlobContainerClient> GetPhotosBlobContainer()
        {
            // Create a BlobServiceClient object which will be used to create a container client
            var blobServiceClient = new BlobServiceClient(_blobStorageConnectionString);

            // Get or create a unique name for the container
            string containerName = "photos";

            try
            {
                var containerClient = await blobServiceClient.CreateBlobContainerAsync(containerName);
                await containerClient.Value.SetAccessPolicyAsync(PublicAccessType.Blob);

                return containerClient;
            }
            catch
            {
                return blobServiceClient.GetBlobContainerClient(containerName);
            }
        }
    }
}
