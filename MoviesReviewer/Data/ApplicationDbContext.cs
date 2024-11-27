using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MoviesReviewer.Models;

namespace MoviesReviewer.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<MoviesReviewer.Models.Movie> Movie { get; set; } = default!;
        public DbSet<MoviesReviewer.Models.Review> Review { get; set; } = default!;
        public DbSet<MoviesReviewer.Models.Preference> Preference { get; set; } = default!;
    }
}
