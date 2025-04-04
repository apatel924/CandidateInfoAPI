using Microsoft.AspNetCore.Mvc;
using CandidateInfoAPI.Models;
using CandidateInfoAPI.Data;
using CandidateInfoAPI.Services;

namespace CandidateInfoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CandidatesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CandidatesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("filter")]
        public IActionResult Filter(
            [FromQuery] string? party,
            [FromQuery] string? riding,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 25)
        {
            var query = _context.Candidates.AsQueryable();

            if (!string.IsNullOrEmpty(party))
                query = query.Where(c => c.Party.Contains(party));

            if (!string.IsNullOrEmpty(riding))
                query = query.Where(c => c.Riding.Contains(riding));

            var total = query.Count();

            var results = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(new
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = total,
                Results = results
            });
        }

        [HttpPost("scrape")]
        public IActionResult ScrapeAndSave([FromServices] CandidateScraper scraper)
        {
            var scraped = scraper.ScrapeFromWikipedia();
            if (scraped.Count == 0)
                return BadRequest("No candidates found.");

            _context.Candidates.AddRange(scraped);
            _context.SaveChanges();
            return Ok(new { message = $"{scraped.Count} candidates saved." });
        }
    }
}
