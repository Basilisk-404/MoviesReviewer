using Microsoft.AspNetCore.Identity;

namespace MoviesReviewer.Models
{
    public class Preference
    {
        public int Id { get; set; }

        public string Type { get; set; }

        public string? UserId { get; set; }

        public IdentityUser? User { get; set; }

        public int? MovieId { get; set; }

        public Movie? Movie { get; set; }
    }
}
