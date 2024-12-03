using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MoviesReviewer.Models
{
    public class Review
    {
        public int Id { get; set; }

        [Display(Name = "OCENA")]
        [Range(1, 5)]
        public int Value { get; set; }

        [Display(Name = "RECENZJA")]
        [MaxLength(1000)]
        [MinLength(3)]
        public string? Comment { get; set; }

        [Display(Name = "DATA RECENZJI")]
        public DateTime? CreatedAt { get; set; }

        public string? UserId { get; set; }

        public IdentityUser? User { get; set; }

        public int? MovieId { get; set; }

        public Movie? Movie { get; set; }
    }
}
