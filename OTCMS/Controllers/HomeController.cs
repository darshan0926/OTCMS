using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OTCMS.Dal;
using OTCMS.Components;
using System.Diagnostics;

namespace OTCMS.Controllers
{
    public class HomeController : Controller
    {
        private readonly OtcmsDbContext _context;

        public HomeController(OtcmsDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var courses = _context.courses.Where(c => c.Status).ToList();
            return View(courses);
        }
        public async Task<IActionResult> CourseList(int id)
        {
            var course = await _context.courses.Include(c => c.Batches.Where(b => b.Status)).FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null)
                return NotFound();
            return View(course);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
