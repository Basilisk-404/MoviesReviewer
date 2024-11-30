using Microsoft.AspNetCore.Identity;

namespace MoviesReviewer.Models
{
    public class Movie
    {
        public int Id { get; set; }
        public int Year { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }

        public string? UserId { get; set; }

        public IdentityUser? User { get; set; }
    }
}
