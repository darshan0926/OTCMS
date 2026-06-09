using Microsoft.AspNetCore.Mvc;

using Microsoft.EntityFrameworkCore;
using OTCMS.Dal;
using OTCMS.Components;
using OTCMS.Utility;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OTCMS.Controllers
{
    public class CourseController : Controller
    {
        private readonly OtcmsDbContext _context;

        public CourseController(OtcmsDbContext context)
        {
            _context = context;
        }


        // GET: Course
        public async Task<IActionResult> Index(
            string sortOrder,
            string currentFilter,
            string searchString,
            int? pageNumber)
        {
            // ✅ Session Validation
            string loggedInUser = HttpContext.Session.GetString("loggedinuser");
            string loggedinuserRole = HttpContext.Session.GetString("loggedinuserRole");

            if (string.IsNullOrEmpty(loggedInUser) || loggedinuserRole != "Admin")
            {
                return RedirectToAction("Login", "User");
            }

            ViewBag.loggedInUserId = loggedInUser;

            // ✅ Sorting setup
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSort"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";

            // ✅ Pagination reset logic when searching
            if (!string.IsNullOrEmpty(searchString))
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            // ✅ Query (build once)
            IQueryable<Course> courses = _context.courses.AsQueryable();

            // ✅ Search
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                courses = courses.Where(c => c.NameOfTheCourse.Contains(searchString));
            }

            // ✅ Sort
            courses = sortOrder == "name_desc"
                ? courses.OrderByDescending(c => c.NameOfTheCourse)
                : courses.OrderBy(c => c.NameOfTheCourse);

            // ✅ "No course found" message only when user searched AND there are no results
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                bool anyFound = await courses.AsNoTracking().AnyAsync();
            }

            // ✅ Pagination
            int pageSize = 3;

            return View(await PaginatedList<Course>.CreateAsync(
                courses.AsNoTracking(),
                pageNumber ?? 1,
                pageSize));
        }

        // GET: Course/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var course = await _context.courses
                .FirstOrDefaultAsync(m => m.CourseId == id);

            if (course == null) return NotFound();

            return View(course);
        }

        // GET: Course/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Course/Create
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("CourseId,NameOfTheCourse,CourseDescription,Status,CourseLogoImage")] Course course)
        //{
        //    if (course.CourseLogoImage == null ||
        //        course.CourseLogoImage.Length == 0 ||
        //        course.CourseLogoImage.Length > 524288 ||
        //        (course.CourseLogoImage.ContentType != "image/jpeg" && course.CourseLogoImage.ContentType != "image/png"))
        //    {
        //        ModelState.AddModelError("CourseLogoImage", "Please select a valid image (JPEG/PNG, max 512KB).");
        //        return View(course);
        //    }

        //    // Generate unique file name
        //    var fileName = Guid.NewGuid().ToString() + "_" + course.CourseLogoImage.FileName;
        //    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

        //    using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
        //    {
        //        await course.CourseLogoImage.CopyToAsync(fs);
        //        course.CourseLogoImagePath = "/images/" + fileName;
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(course);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }

        //    return View(course);
        //}

        // GET: Course/Edit/5
       

        // POST: Course/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CourseId,NameOfTheCourse,CourseDescription,Status,CourseLogoImage,CourseContentFile")] Course course)
        {
            // ✅ 2) Duplicate BatchId check BEFORE saving (prevents DB exception)
            bool courseIdExists = await _context.courses
                .AsNoTracking()
                .AnyAsync(b => b.CourseId == course.CourseId);

            if (courseIdExists)
            {
                ModelState.AddModelError(nameof(course.CourseId), "Course Id already exists. Please enter a unique Course Id.");
            }
            // Validate Course Logo
            if (course.CourseLogoImage == null ||
                course.CourseLogoImage.Length == 0 ||
                course.CourseLogoImage.Length > 524288 ||
                (course.CourseLogoImage.ContentType != "image/jpeg" && course.CourseLogoImage.ContentType != "image/png"))
            {
                ModelState.AddModelError("CourseLogoImage", "Please select a valid image (JPEG/PNG, max 512KB).");
                return View(course);
            }

            // Save Course Logo
            var logoFileName = Guid.NewGuid().ToString() + "_" + course.CourseLogoImage.FileName;
            var logoFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", logoFileName);

            using (var fs = new FileStream(logoFilePath, FileMode.Create, FileAccess.Write))
            {
                await course.CourseLogoImage.CopyToAsync(fs);
                course.CourseLogoImagePath = "/images/" + logoFileName;
            }

            // ✅ Validate Course Content PDF
            if (course.CourseContentFile != null)
            {
                if (course.CourseContentFile.ContentType != "application/pdf")
                {
                    ModelState.AddModelError("CourseContentFile", "Only PDF files are allowed.");
                    return View(course);
                }

                if (course.CourseContentFile.Length > 3048576) // 1 MB limit (adjust as needed)
                {
                    ModelState.AddModelError("CourseContentFile", "PDF file size must be less than 3 MB.");
                    return View(course);
                }

                // Save PDF file
                var pdfFileName = Guid.NewGuid().ToString() + "_" + course.CourseContentFile.FileName;
                var pdfFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/coursecontent", pdfFileName);

                using (var fs = new FileStream(pdfFilePath, FileMode.Create, FileAccess.Write))
                {
                    await course.CourseContentFile.CopyToAsync(fs);
                    course.CourseContentFilePath = "/coursecontent/" + pdfFileName;
                }
            }

            if (ModelState.IsValid)
            {
                _context.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(course);
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var course = await _context.courses.FindAsync(id);
            if (course == null) return NotFound();

            return View(course);
        }

        // POST: Course/Edit/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, [Bind("CourseId,NameOfTheCourse,CourseDescription,Status,CourseLogoImage,CourseLogoImagePath")] Course course)
        //{
        //    if (id != course.CourseId) return NotFound();

        //    if (course.CourseLogoImage != null) // replace old logo with new one
        //    {
        //        if (course.CourseLogoImage.Length > 524288 ||
        //            (course.CourseLogoImage.ContentType != "image/jpeg" && course.CourseLogoImage.ContentType != "image/png"))
        //        {
        //            ModelState.AddModelError("CourseLogoImage", "Please select a valid image (JPEG/PNG, max 512KB).");
        //            return View(course);
        //        }


        //        var fileName = Guid.NewGuid().ToString() + "_" + course.CourseLogoImage.FileName;
        //        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

        //        using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
        //        {
        //            await course.CourseLogoImage.CopyToAsync(fs);
        //            course.CourseLogoImagePath = "/images/" + fileName;
        //        }
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(course);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!CourseExists(course.CourseId)) return NotFound();
        //            else throw;
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }

        //    return View(course);
        //}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CourseId,NameOfTheCourse,CourseDescription,Status,CourseLogoImage,CourseLogoImagePath,CourseContentFile,CourseContentFilePath")] Course course)
        {
            if (id != course.CourseId) return NotFound();

            // ✅ Handle Logo Replacement
            if (course.CourseLogoImage != null)
            {
                if (course.CourseLogoImage.Length > 524288 ||
                    (course.CourseLogoImage.ContentType != "image/jpeg" && course.CourseLogoImage.ContentType != "image/png"))
                {
                    ModelState.AddModelError("CourseLogoImage", "Please select a valid image (JPEG/PNG, max 512KB).");
                    return View(course);
                }

                var logoFileName = Guid.NewGuid().ToString() + "_" + course.CourseLogoImage.FileName;
                var logoFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", logoFileName);

                using (var fs = new FileStream(logoFilePath, FileMode.Create, FileAccess.Write))
                {
                    await course.CourseLogoImage.CopyToAsync(fs);
                    course.CourseLogoImagePath = "/images/" + logoFileName;
                }
            }

            // ✅ Handle PDF Replacement
            if (course.CourseContentFile != null)
            {
                if (course.CourseContentFile.ContentType != "application/pdf")
                {
                    ModelState.AddModelError("CourseContentFile", "Only PDF files are allowed.");
                    return View(course);
                }

                if (course.CourseContentFile.Length > 4048576) // 1 MB limit (adjust as needed)
                {
                    ModelState.AddModelError("CourseContentFile", "PDF file size must be less than 4 MB.");
                    return View(course);
                }

                var pdfFileName = Guid.NewGuid().ToString() + "_" + course.CourseContentFile.FileName;
                var pdfFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/coursecontent", pdfFileName);

                using (var fs = new FileStream(pdfFilePath, FileMode.Create, FileAccess.Write))
                {
                    await course.CourseContentFile.CopyToAsync(fs);
                    course.CourseContentFilePath = "/coursecontent/" + pdfFileName;
                }
                
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(course);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CourseExists(course.CourseId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            return View(course);
        }

        // GET: Course/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var course = await _context.courses
                .FirstOrDefaultAsync(m => m.CourseId == id);

            if (course == null) return NotFound();

            return View(course);
        }

        // POST: Course/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.courses.FindAsync(id);
            if (course != null)
            {
                _context.courses.Remove(course);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CourseExists(int id)
        {
            return _context.courses.Any(e => e.CourseId == id);
        }
    }
}
