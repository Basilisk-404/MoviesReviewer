using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;
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
         * TODO: Dodać dto, aby nie przekazywać użytkownika
         * 
         */
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Movie.Include(m => m.User);
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
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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

        //// GET: Movies/Edit/5
        //public async Task<IActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var movie = await _context.Movie.FindAsync(id);
        //    if (movie == null)
        //    {
        //        return NotFound();
        //    }
        //    ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", movie.UserId);
        //    return View(movie);
        //}

        //// POST: Movies/Edit/5
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, [Bind("Id,Year,Title,Author,UserId")] Movie movie)
        //{
        //    if (id != movie.Id)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(movie);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!MovieExists(movie.Id))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //    ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", movie.UserId);
        //    return View(movie);
        //}

        //// GET: Movies/Delete/5
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var movie = await _context.Movie
        //        .Include(m => m.User)
        //        .FirstOrDefaultAsync(m => m.Id == id);
        //    if (movie == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(movie);
        //}

        //// POST: Movies/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    var movie = await _context.Movie.FindAsync(id);
        //    if (movie != null)
        //    {
        //        _context.Movie.Remove(movie);
        //    }

        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        private bool MovieExists(int id)
        {
            return _context.Movie.Any(e => e.Id == id);
        }
    }
}
