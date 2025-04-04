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

        [HttpGet("report")]
        public IActionResult GetReport()
        {
            var partySummary = _context.Candidates
                .GroupBy(c => c.Party)
                .Select(g => new
                {
                    Party = g.Key,
                    TotalCandidates = g.Count(),
                    ElectedCount = g.Count(c => c.Result.Contains("Elected"))
                })
                .OrderByDescending(p => p.TotalCandidates)
                .ToList();

            return Ok(new
            {
                TotalCandidates = _context.Candidates.Count(),
                TotalParties = partySummary.Count,
                Report = partySummary
            });
        }

        [HttpPost("scrape")]
        public IActionResult ScrapeAndSave([FromServices] CandidateScraper scraper)
        {
            var scraped = scraper.ScrapeFromWikipedia();
            if (scraped.Count == 0)
                return BadRequest("No candidates found.");

            _context.Candidates.AddRange(scraped);
            var saved = _context.SaveChanges();

            return Ok(new { message = $"{scraped.Count} scraped, {saved} saved." });
        }
    }
}
