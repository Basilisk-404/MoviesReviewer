using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MoviesReviewer.Models
{
    public class Preference
    {
        public int Id { get; set; }

        [Display(Name = "OZNACZENIE")]
        public string Type { get; set; }

        public string? UserId { get; set; }

        public IdentityUser? User { get; set; }

        public int? MovieId { get; set; }

        public Movie? Movie { get; set; }
    }
}
