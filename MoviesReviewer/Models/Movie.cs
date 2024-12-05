using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MoviesReviewer.Models
{
    public class Movie
    {
        public int Id { get; set; }

        [Display(Name = "ROK PRODUKCJI")]
        [Range(1888, 2100)]
        public int Year { get; set; }

        [Display(Name = "TYTUŁ")]
        [MinLength(2)]
        [MaxLength(300)]
        public string Title { get; set; }

        [Display(Name = "REŻYSER")]
        [MinLength(2)]
        [MaxLength(300)]
        public string Author { get; set; }

        public string? UserId { get; set; }

        public IdentityUser? User { get; set; }
    }
}
