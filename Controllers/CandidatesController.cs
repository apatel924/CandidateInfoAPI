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

        [HttpGet]
        public IActionResult GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 25)
        {
            var candidates = _context.Candidates
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var total = _context.Candidates.Count();

            return Ok(new
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = total,
                Results = candidates
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
