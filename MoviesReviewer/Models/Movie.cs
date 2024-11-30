using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MoviesReviewer.Models
{
    public class Movie
    {
        public int Id { get; set; }

        [Display(Name = "ROK PRODUKCJI")]
        public int Year { get; set; }

        [Display(Name = "TYTUŁ")]
        public string Title { get; set; }

        [Display(Name = "REŻYSER")]
        public string Author { get; set; }

        public string? UserId { get; set; }

        public IdentityUser? User { get; set; }
    }
}
