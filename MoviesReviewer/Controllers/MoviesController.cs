using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MoviesReviewer.Data;
using MoviesReviewer.Enums;
using MoviesReviewer.Models;

namespace MoviesReviewer.Controllers
{
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MoviesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Movies
        /*
         * 
         * Public list of ALL MOVIES
         * 
         */
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Movie;
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Movies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // GET: Movies/Create
        /*
         * 
         * Tworzenie tylko dla zalogowanych - troll prevention
         * 
         */
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Movies/Create
        /*
         * 
         * Tworzenie tylko dla zalogowanych - troll prevention
         * 
         */
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Year,Title,Author")] Movie movie)
        {
            movie.Title = movie.Title.Trim();
            movie.Author = movie.Author.Trim();

            var similarMovieCount = _context.Movie.Where(m =>
                m.Year == movie.Year
                && m.Title == movie.Title
                && m.Author == movie.Author
            ).Count();

            if(similarMovieCount > 0)
            {
                ViewBag.ErrorMessage = "Film o podanych danych istnieje";
                ViewBag.Action = "Create";
                ViewBag.Controller = "Movies";
                return View("CustomErrorView");
            }

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            movie.UserId = userId;

            if (ModelState.IsValid)
            {
                _context.Add(movie);
                await _context.SaveChangesAsync();

                Preference preference = new Preference();
                preference.Type = PreferenceType.TO_WATCH;
                preference.UserId = userId;
                preference.MovieId = movie.Id;

                _context.Add(preference);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", movie.UserId);
            return View(movie);
        }
        private bool MovieExists(int id)
        {
            return _context.Movie.Any(e => e.Id == id);
        }
    }
}
