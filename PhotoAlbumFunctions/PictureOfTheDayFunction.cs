using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using PhotoAlbumFunctions.Data;
using PhotoAlbumFunctions.Helpers;

namespace PhotoAlbumFunctions
{
    public class PictureOfTheDayFunction
    {
        private readonly PhotoAlbumContext _context;

        public PictureOfTheDayFunction(PhotoAlbumContext context)
        {
            _context = context;
        }

        [FunctionName(nameof(PictureOfTheDayFunction))]
        public async Task Run([TimerTrigger("0 0 0 * * *")] TimerInfo timer, ILogger log)
        {
            string imageUrlOfTheDay = await BingHelper.GetImageOfTheDayUrl();

            var photo = new Photo
            {
                Url = imageUrlOfTheDay,
                CreatedAt = DateTime.Now,
                Tags = new List<Tag> 
                { 
                    new Tag { Value = "bing" } 
                }
            };

            _context.Photos.Add(photo);
            await _context.SaveChangesAsync();
        }
    }
}
