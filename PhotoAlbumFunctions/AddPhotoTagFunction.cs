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
    public class AddPhotoTagFunction
    {
        private readonly PhotoAlbumContext _context;

        public AddPhotoTagFunction(PhotoAlbumContext context)
        {
            _context = context;
        }

        [FunctionName(nameof(AddPhotoTagFunction))]
        public async Task<PhotoResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "photos/{photoId}/tag/{tag}")] HttpRequest req,
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

            photo.Tags.Add(new Tag { Value = tag });
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
