using Microsoft.EntityFrameworkCore;
using CandidateInfoAPI.Models;

namespace CandidateInfoAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Candidate> Candidates => Set<Candidate>();
    }
}
