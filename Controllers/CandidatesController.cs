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
        public IActionResult GetAll()
        {
            var candidates = _context.Candidates.ToList();
            return Ok(candidates);
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
