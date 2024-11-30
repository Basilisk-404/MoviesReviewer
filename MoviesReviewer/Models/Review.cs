using Microsoft.AspNetCore.Identity;

namespace MoviesReviewer.Models
{
    public class Review
    {
        public int Id { get; set; }

        public int Value { get; set; }

        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; }

        public string UserId { get; set; }

        public IdentityUser? User { get; set; }

        public int MovieId { get; set; }

        public Movie? Movie { get; set; }
    }
}
