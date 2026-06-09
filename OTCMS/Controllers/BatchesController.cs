using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OTCMS.Dal;
using OTCMS.Components;
using OTCMS.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OTCMS.Controllers
{
    public class BatchesController : Controller
    {
        private readonly OtcmsDbContext _context;

        public BatchesController(OtcmsDbContext context)
        {
            _context = context;
        }


        // GET: Batches
        public async Task<IActionResult> Index(
            string sortOrder,
            string currentFilter,
            string searchString,
            int? courseId,
            int? currentCourseId,
            int? pageNumber)
        {
            string loggedInUser = HttpContext.Session.GetString("loggedinuser");
            string loggedinuserRole = HttpContext.Session.GetString("loggedinuserRole");
           
            if (loggedInUser == null || loggedinuserRole != "Admin")
                return RedirectToAction("Login", "User");

            ViewBag.loggedInUserId = loggedInUser;

            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSort"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";

            if (searchString != null || courseId != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
                courseId = currentCourseId;
            }

            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentCourseId"] = courseId;

            var activeCourses = await _context.courses
                .Where(c => c.Status)
                .OrderBy(c => c.NameOfTheCourse)
                .ToListAsync();

            ViewData["CourseId"] = new SelectList(activeCourses, "CourseId", "NameOfTheCourse", courseId);

            var batchesQuery = _context.batches
                .Include(b => b.Course)
                .Where(b => b.Course != null && b.Course.Status)
                .AsQueryable();

            if (courseId.HasValue)
            {
                batchesQuery = batchesQuery.Where(b => b.CourseId == courseId.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                batchesQuery = batchesQuery.Where(b =>
                    b.Course.NameOfTheCourse.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    batchesQuery = batchesQuery.OrderByDescending(b => b.BatchName);
                    ViewData["NameSort"] = "";
                    break;

                default:
                    batchesQuery = batchesQuery.OrderBy(b => b.BatchName);
                    break;
            }

            int pageSize = 4;
            return View(await PaginatedList<Batch>.CreateAsync(
                batchesQuery.AsNoTracking(),
                pageNumber ?? 1,
                pageSize));
        }




        // GET: Batches/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var batch = await _context.batches
                .Include(b => b.Course)
                .FirstOrDefaultAsync(m => m.BatchId == id);
            if (batch == null)
            {
                return NotFound();
            }

            return View(batch);
        }

        // GET: Batches/Create

        public IActionResult Create()
        {
            var activeCourses = _context.courses
                .Where(c => c.Status == true)                // only active
                .OrderBy(c => c.NameOfTheCourse)       // optional: nice sorting
                .ToList();

            ViewData["CourseId"] = new SelectList(activeCourses, "CourseId", "NameOfTheCourse");
            return View();
        }



        // POST: Batches/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BatchId,BatchName,StartDate,EndDate,HandledBy,Status,Timings,Duration,Fee,Description,CourseId")] Batch batch)
        {
            // ✅ Custom validation: EndDate must be greater than StartDate
            if (batch.EndDate <= batch.StartDate)
            {
                ModelState.AddModelError(nameof(batch.EndDate), "End Date must be greater than Start Date.");
            }

            // ✅ 2) Duplicate BatchId check BEFORE saving (prevents DB exception)
            bool batchIdExists = await _context.batches
                .AsNoTracking()
                .AnyAsync(b => b.BatchId == batch.BatchId);

            if (batchIdExists)
            {
                ModelState.AddModelError(nameof(batch.BatchId), "Batch Id already exists. Please enter a unique Batch Id.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(batch);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Repopulate dropdown when returning view
            ViewData["CourseId"] = new SelectList(_context.courses, "CourseId", "NameOfTheCourse", batch.CourseId);
            return View(batch);
        }

        // GET: Batches/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var activeCourses = _context.courses
                .Where(c => c.Status == true)                // only active
                .OrderBy(c => c.NameOfTheCourse)       // optional: nice sorting
                .ToList();
            if (id == null)
            {
                return NotFound();
            }

            var batch = await _context.batches.FindAsync(id);
            if (batch == null)
            {
                return NotFound();
            }
            ViewData["CourseId"] = new SelectList(activeCourses, "CourseId", "NameOfTheCourse", batch.CourseId);
            return View(batch);
        }

        // POST: Batches/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BatchId,BatchName,StartDate,EndDate,HandledBy,Status,Timings,Duration,Fee,Description,CourseId")] Batch batch)
        { 
            // ✅ Custom validation: EndDate must be greater than StartDate
            if (batch.EndDate <= batch.StartDate)
            {
                ModelState.AddModelError(nameof(batch.EndDate), "End Date must be greater than Start Date.");
            }


            if (id != batch.BatchId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(batch);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BatchExists(batch.BatchId))
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
            ViewData["CourseId"] = new SelectList(_context.courses, "CourseId", "NameOfTheCourse", batch.CourseId);
            return View(batch);
        }

        // GET: Batches/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var batch = await _context.batches
                .Include(b => b.Course)
                .FirstOrDefaultAsync(m => m.BatchId == id);
            if (batch == null)
            {
                return NotFound();
            }

            return View(batch);
        }

        // POST: Batches/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var batch = await _context.batches.FindAsync(id);
            if (batch != null)
            {
                _context.batches.Remove(batch);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BatchExists(int id)
        {
            return _context.batches.Any(e => e.BatchId == id);
        }
    }
}
