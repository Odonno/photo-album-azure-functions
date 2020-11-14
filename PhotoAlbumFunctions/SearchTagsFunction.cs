using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PhotoAlbumFunctions.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;

namespace PhotoAlbumFunctions
{
    public class SearchTagsFunction
    {
        private readonly PhotoAlbumContext _context;

        public SearchTagsFunction(PhotoAlbumContext context)
        {
            _context = context;
        }

        [FunctionName(nameof(SearchTagsFunction))]
        public Task<List<string>> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "search/{search}")] HttpRequest req,
            string search,
            ILogger log
        )
        {
            string lowerSearch = search.ToLower();

            return _context.Tags
                .Select(t => t.Value)
                .Where(v => v.ToLower().StartsWith(lowerSearch))
                .Distinct()
                .Take(5)
                .ToListAsync();
        }
    }
}
