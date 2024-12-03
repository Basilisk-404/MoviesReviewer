using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MoviesReviewer.Data;
using MoviesReviewer.Enums;
using MoviesReviewer.Models;

namespace MoviesReviewer.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReviewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /*
         * 
         * Wszystkie istniejące opinie - możemy wyświetlać wszystko
         * 
         * TODO: Dodać DTO żeby nie przekazywać USER ID
         * 
         */
        // GET: Reviews
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Review.Include(r => r.Movie).Include(r => r.User);
            return View(await applicationDbContext.ToListAsync());
        }

        /*
         * 
         * Szczegóły opinii - dostępne publicznie dla każdego
         * 
         * TODO: Dodać DTO żeby nie przekazywać USER ID
         */
        // GET: Reviews/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var review = await _context.Review
                .Include(r => r.Movie)
                .Include(r => r.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (review == null)
            {
                return NotFound();
            }

            return View(review);
        }

        /*
         * 
         * Lista moich opinii
         * 
         */
        [Authorize]
        public async Task<IActionResult> My()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var applicationDbContext = _context.Review
                .Include(m => m.User)
                .Include(m => m.Movie)
                .Where(m => m.UserId == userId);
            return View(await applicationDbContext.ToListAsync());
        }

        /*
         * 
         * Tworzenie opinii - tylko dla zalogowanych, którzy obejrzeli dany film
         * 
         */
        // GET: Reviews/Create/{movieId}
        [Authorize]
        public IActionResult Create(int? id)
        {

            var movie = _context.Movie.Find(id);

            if(movie == null)
            {
                ViewBag.ErrorMessage = "Film o podanym ID nie istnieje";
                return View("CustomErrorView");
            }

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var preferenceExists = _context.Preference.Where(
                p => p.MovieId == id
                && p.UserId == userId
                && p.Type == PreferenceType.WATCHED
                ).Count();

            if (preferenceExists == 0)
            {
                ViewBag.ErrorMessage = "Wybrany film nie został przez Ciebie obejrzany, więc nie możesz wystawić opinii";
                return View("CustomErrorView");
            }

            var reviewExists = _context.Review.Where(p =>
                p.MovieId == movie.Id
                && p.UserId == userId
                ).Count();

            if (reviewExists > 0)
            {
                ViewBag.ErrorMessage = "Wybrany film został już przez Ciebie oceniony";
                return View("CustomErrorView");
            }

            //ViewData["MovieId"] = new SelectList(_context.Movie, "Id", "Id");
            ViewData["MovieId"] = movie.Id;
            ViewData["MovieTitle"] = movie.Title;
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        /*
         * 
         * Tworzenie opinii - tylko dla zalogowanych, którzy obejrzeli dany film
         * 
         */
        // POST: Reviews/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("Id,Value,Comment,UserId,MovieId")] Review review)
        {
            var movie = _context.Movie.Find(review.MovieId);

            if (movie == null)
            {
                ViewBag.ErrorMessage = "Film o podanym ID nie istnieje";
                return View("CustomErrorView");
            }

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var preferenceExists = _context.Preference.Where(p =>
                 p.MovieId == movie.Id
                && p.UserId == userId
                && p.Type == PreferenceType.WATCHED
                ).Count();

            if (preferenceExists == 0)
            {
                ViewBag.ErrorMessage = "Wybrany film nie został przez Ciebie obejrzany, więc nie możesz wystawić opinii";
                return View("CustomErrorView");
            }

            var reviewExists = _context.Review.Where(p =>
                p.MovieId == movie.Id
                && p.UserId == userId
                ).Count();

            if (reviewExists > 0)
            {
                ViewBag.ErrorMessage = "Wybrany film został już przez Ciebie oceniony";
                return View("CustomErrorView");
            }

            review.UserId = userId;
            review.CreatedAt = DateTime.Now;

            if (ModelState.IsValid)
            {
                _context.Add(review);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(My));
            }

            ViewData["MovieId"] = movie.Id;
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", review.UserId);
            return View(review);
        }

        /*
         * 
         * Edycja opinii - dostępna tylko dla zalogowanych
         * 
         */
        // GET: Reviews/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var review = await _context.Review.FindAsync(id);
            
            if (review == null || review.UserId != userId)
            {
                return NotFound();
            }

            review.Movie = await _context.Movie.FindAsync(review.MovieId);

            if (review.Movie == null)
            {
                ViewBag.ErrorMessage = "Film recenzowany tą opinią nie istnieje";
                return View("CustomErrorView");
            }

            ViewBag.MovieTitle = review.Movie.Title;
            return View(review);
        }

        /*
         * 
         * Edycja opinii - dostępna tylko dla zalogowanych
         * 
         */
        // POST: Reviews/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Value,Comment,CreatedAt,UserId,MovieId")] Review review)
        {
            if (id != review.Id)
            {
                return NotFound();
            }

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (review == null || review.UserId != userId)
            {
                return NotFound();
            }

            var correspondingMovie = await _context.Movie.FindAsync(review.MovieId);

            if (correspondingMovie == null)
            {
                ViewBag.ErrorMessage = "Film recenzowany tą opinią nie istnieje";
                return View("CustomErrorView");
            }

            review.UserId = userId;                      // Zapobiegnięcie zgubienia ID użytkownika
            //review.CreatedAt = existingReview.CreatedAt; // Zapobiegnięcie zgubienia daty utworzenia

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(review);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReviewExists(review.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(My));
            }
            ViewData["MovieId"] = review.MovieId;
            ViewData["UserId"] = userId;
            ViewBag.MovieTitle = correspondingMovie.Title;
            return View(review);
        }

        /*
         * 
         * Usuwanie opinii tylko dla zalogowanych (można usunąć tylko swoją)
         * 
         */
        // GET: Reviews/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var review = await _context.Review
                .Include(r => r.Movie)
                .Include(r => r.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (review == null || review.UserId != userId)
            {
                return NotFound();
            }

            return View(review);
        }

        /*
         * 
         * Usuwanie opinii tylko dla zalogowanych (można usunąć tylko swoją)
         * 
         */
        // POST: Reviews/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var review = await _context.Review.FindAsync(id);
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (review != null && review.UserId == userId)
            {
                _context.Review.Remove(review);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(My));
        }

        private bool ReviewExists(int id)
        {
            return _context.Review.Any(e => e.Id == id);
        }
    }
}
