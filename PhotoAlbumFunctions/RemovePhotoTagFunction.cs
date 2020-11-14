using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PhotoAlbumFunctions.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using PhotoAlbumFunctions.Models;

namespace PhotoAlbumFunctions
{
    public class RemovePhotoTagFunction
    {
        private readonly PhotoAlbumContext _context;

        public RemovePhotoTagFunction(PhotoAlbumContext context)
        {
            _context = context;
        }

        [FunctionName(nameof(RemovePhotoTagFunction))]
        public async Task<PhotoResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "photos/{photoId}/tag/{tag}")] HttpRequest req,
            int photoId,
            string tag,
            ILogger log
        )
        {
            var photo = await _context.Photos.FirstOrDefaultAsync(p => p.Id == photoId);

            if (photo == null)
            {
                return null;
            }

            var tagToRemove = await _context.Tags.FirstOrDefaultAsync(t => t.PhotoId == photoId && t.Value == tag);

            photo.Tags.Remove(tagToRemove);
            await _context.SaveChangesAsync();

            return new PhotoResult
            {
                Id = photo.Id,
                Url = photo.Url,
                CreatedAt = photo.CreatedAt,
                Tags = photo.Tags.Select(t => t.Value).ToList()
            };
        }
    }
}
