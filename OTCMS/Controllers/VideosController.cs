using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OTCMS.Dal;
using OTCMS.Components;

namespace OTCMS.Controllers
{

    public class VideosController : Controller

    {

        private readonly OtcmsDbContext _context;

        public VideosController(OtcmsDbContext context)

        {

            _context = context;

        }

        // GET: Videos

        public async Task<IActionResult> Index(string searchString)
        {
            string loggedInUser = HttpContext.Session.GetString("loggedinuser");
            string loggedinuserRole = HttpContext.Session.GetString("loggedinuserRole");
            if (loggedInUser == null || loggedinuserRole != "Admin")
                return RedirectToAction("Login", "User");

            ViewBag.loggedInUserId = loggedInUser;
            ViewData["CurrentFilter"] = searchString;

            var videos = _context.videos.Include(v => v.course).Where(v => v.course.Status==true).AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                videos = videos.Where(v =>
                    v.VideoTitle.Contains(searchString) ||
                    v.VideoDescription.Contains(searchString));
            }

            return View(await videos.AsNoTracking().ToListAsync());
        }


        // GET: Videos/Details/5

        public async Task<IActionResult> Details(int? id)

        {

            if (id == null)

            {

                return NotFound();

            }

            var video = await _context.videos

                .Include(v => v.course)

                .FirstOrDefaultAsync(m => m.CourseVideoId == id);

            if (video == null)

            {

                return NotFound();

            }

            return View(video);

        }

        // GET: Videos/Create

        public IActionResult Create()

        {
            var activeCourses = _context.courses
    .Where(c => c.Status == true)                // only active
    .OrderBy(c => c.NameOfTheCourse)       // optional: nice sorting
    .ToList();

            ViewData["CourseId"] = new SelectList(activeCourses, "CourseId", "NameOfTheCourse");

            return View();

        }

        // POST: Videos/Create

        // To protect from overposting attacks, enable the specific properties you want to bind to.

        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.

        [HttpPost]

        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Create([Bind("VideoTitle,VideoDescription,VideoLink,Status,CourseId")] Video video)

        {
            video.CreatedOn = DateTime.Now;
 
            if (ModelState.IsValid)

            {

                _context.Add(video);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));

            }

            ViewData["CourseId"] = new SelectList(_context.courses, "CourseId", "NameOfTheCourse", video.CourseId);

            return View(video);

        }

        // GET: Videos/Edit/5

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

            var video = await _context.videos.FindAsync(id);

            if (video == null)

            {

                return NotFound();

            }

            ViewData["CourseId"] = new SelectList(activeCourses, "CourseId", "NameOfTheCourse", video.CourseId);

            return View(video);

        }

        // POST: Videos/Edit/5

        // To protect from overposting attacks, enable the specific properties you want to bind to.

        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.

        [HttpPost]

        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Edit(int id, [Bind("CourseVideoId,VideoTitle,VideoDescription,VideoLink,Status,CreatedOn,CourseId")] Video video)

        {

            if (id != video.CourseVideoId)

            {

                return NotFound();

            }

            if (ModelState.IsValid)

            {

                try

                {

                    _context.Update(video);

                    await _context.SaveChangesAsync();

                }

                catch (DbUpdateConcurrencyException)

                {

                    if (!VideoExists(video.CourseVideoId))

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

            ViewData["CourseId"] = new SelectList(_context.courses, "CourseId", "NameOfTheCourse", video.CourseId);

            return View(video);

        }

        // GET: Videos/Delete/5

        public async Task<IActionResult> Delete(int? id)

        {

            if (id == null)

            {

                return NotFound();

            }

            var video = await _context.videos

                .Include(v => v.course)

                .FirstOrDefaultAsync(m => m.CourseVideoId == id);

            if (video == null)

            {

                return NotFound();

            }

            return View(video);

        }

        // POST: Videos/Delete/5

        [HttpPost, ActionName("Delete")]

        [ValidateAntiForgeryToken]

        public async Task<IActionResult> DeleteConfirmed(int id)

        {

            var video = await _context.videos.FindAsync(id);

            if (video != null)

            {

                _context.videos.Remove(video);

            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));

        }

        private bool VideoExists(int id)

        {

            return _context.videos.Any(e => e.CourseVideoId == id);

        }


        //to return all videos

        // GET: Videos/Details/5

        public async Task<IActionResult> GetAllVideos()

        {

            List<Video> videoList = await _context.videos.Include("course").ToListAsync();

            return View("DisplayVideos", videoList); // DisplayVideos.cshtml with listobject

        }

        public async Task<IActionResult> SelectCourse()

        {
            var activeCourses = _context.courses
                .Where(c => c.Status == true)                // only active
                .OrderBy(c => c.NameOfTheCourse)       // optional: nice sorting
                .ToList();

            ViewData["CourseId"] = new SelectList(activeCourses, "CourseId", "NameOfTheCourse");

            return View();  // SelectTopic.cshtml 

        }
       

        [HttpPost]

        public async Task<IActionResult> GetAllVideosByCourseId(int CourseId)

        {

            List<Video> videoList = await _context.videos.Include("course").Where(v => v.CourseId == CourseId).ToListAsync();

            Course? tp = await _context.courses.FindAsync(CourseId);

            ViewBag.NameOfTheCourse = tp.NameOfTheCourse;

            return View("DisplayVideos", videoList); // DisplayVideos.cshtml with listobject

        }

    }

}

