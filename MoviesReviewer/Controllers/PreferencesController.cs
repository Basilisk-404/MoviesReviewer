using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MoviesReviewer.Data;
using MoviesReviewer.Dtos;
using MoviesReviewer.Enums;
using MoviesReviewer.Models;

namespace MoviesReviewer.Controllers
{
    /*
     * 
     * Cały moduł zarządzania listą filmów (preferencjami) ma być dostępny tylko dla użytkowników zalogowanych
     * 
     */
    [Authorize]
    public class PreferencesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PreferencesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Preferences
        public async Task<IActionResult> Index()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var applicationDbContext = _context.Preference.Include(p => p.Movie).Where(p => p.UserId == userId);

            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Preferences/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var preference = await _context.Preference
                .Include(p => p.Movie)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (preference == null || preference.UserId != userId)
            {
                return NotFound();
            }

            return View(preference);
        }

        // GET: Preferences/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var preference = await _context.Preference.FindAsync(id);

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (preference == null || preference.UserId != userId)
            {
                return NotFound();
            }

            preference.Movie = await _context.Movie.FindAsync(preference.MovieId);

            if (preference.Movie == null)
            {
                ViewBag.ErrorMessage = "Film, którego preferencję chcesz edytować, nie istnieje";
                ViewBag.Action = "Index";
                ViewBag.Controller = "Preferences";
                return View("CustomErrorView");
            }

            ViewData["MovieId"] = preference.MovieId;
            ViewData["UserId"] = preference.UserId;
            ViewBag.MovieTitle = preference.Movie.Title;

            List<PreferenceTypeDto> preferenceTypes = new List<PreferenceTypeDto>();

            preferenceTypes.Add(new PreferenceTypeDto(PreferenceType.WATCHED, "OBEJRZANY"));
            preferenceTypes.Add(new PreferenceTypeDto(PreferenceType.TO_WATCH, "DO OBEJRZENIA"));

            ViewData["Type"] = new SelectList(preferenceTypes, "Type", "Display", preference.Type);

            return View(preference);
        }

        // POST: Preferences/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Type,UserId,MovieId")] Preference preference)
        {
            if (preference.Type != PreferenceType.TO_WATCH && preference.Type != PreferenceType.WATCHED)
            {
                ViewBag.ErrorMessage = "Wybrany typ preferencji nie jest dozwolony";
                ViewBag.Action = "Index";
                ViewBag.Controller = "Movies";
                return View("CustomErrorView");
            }

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (id != preference.Id || preference.UserId != userId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(preference);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PreferenceExists(preference.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            preference.Movie = await _context.Movie.FindAsync(preference.MovieId);

            if (preference.Movie == null)
            {
                ViewBag.ErrorMessage = "Film, którego preferencję chcesz edytować, nie istnieje";
                ViewBag.Action = "Index";
                ViewBag.Controller = "Preferences";
                return View("CustomErrorView");
            }

            ViewData["MovieId"] = preference.MovieId;
            ViewData["UserId"] = preference.UserId;
            ViewBag.MovieTitle = preference.Movie.Title;

            return View(preference);
        }

        // GET: Preferences/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var preference = await _context.Preference
                .Include(p => p.Movie)
                .FirstOrDefaultAsync(m => m.Id == id);

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (preference == null || preference.UserId != userId)
            {
                return NotFound();
            }

            return View(preference);
        }

        // POST: Preferences/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var preference = await _context.Preference.FindAsync(id);

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (preference != null && preference.UserId == userId)
            {
                _context.Preference.Remove(preference);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Preferences/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPreference([Bind("Id, Type, MovieId")] Preference preference)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if(preference.Type != PreferenceType.TO_WATCH && preference.Type != PreferenceType.WATCHED)
            {
                ViewBag.ErrorMessage = "Wybrany typ preferencji nie jest dozwolony";
                ViewBag.Action = "Index";
                ViewBag.Controller = "Movies";
                return View("CustomErrorView");
            }

            var preferenceExists = _context.Preference
                .Where(p => p.MovieId == preference.MovieId && p.UserId == userId)
                .Count();

            if(preferenceExists != 0)
            {
                ViewBag.ErrorMessage = "Wybrany film istnieje w Twoich preferencjach";
                ViewBag.Action = "Index";
                ViewBag.Controller = "Movies";
                return View("CustomErrorView");
            }
          
            preference.UserId = userId;

            if (ModelState.IsValid)
            {
                _context.Add(preference);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.ErrorMessage = "Wystąpił błąd podczas przetwarzania żądania. Spróbuj ponownie później";
            ViewBag.Action = "Index";
            ViewBag.Controller = "Preferences";
            return View("CustomErrorView");
        }

        private bool PreferenceExists(int id)
        {
            return _context.Preference.Any(e => e.Id == id);
        }
    }
}
