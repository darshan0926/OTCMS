using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OTCMS.Dal;
using OTCMS.Components;
using OTCMS.Components.ViewModels;

namespace OTCMS.Controllers
{
    public class AdminController : Controller
    {
        private readonly OtcmsDbContext _context;

        public AdminController(OtcmsDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            string loggedInUser = HttpContext.Session.GetString("loggedinuser");
            string loggedinuserRole = HttpContext.Session.GetString("loggedinuserRole");
            if (!string.IsNullOrEmpty(loggedInUser) && loggedinuserRole == "Admin")
            {

                //var cm = new Course
                //{
                //    CourseId = await _context.courses.CountAsync()
                //};
                //ViewBag.countcourse = cm.CourseId;
                var bm = new Batch
                {
                    BatchId = await _context.batches.CountAsync(),
                    CourseId = await _context.courses.CountAsync()
                    
                };
                var bc = new BatchContent
                {
                    BCId = await _context.batchContents.CountAsync(),
                };
                var sl = new Slider
                {
                    SliderId = await _context.sliders.CountAsync(),
                };
                var vg = new Video
                {
                    CourseVideoId = await _context.videos.CountAsync(),
                };
                ViewBag.countcourse = bm.CourseId;
                ViewBag.countbatch = bm.BatchId;
                ViewBag.countbc = bc.BCId;
                ViewBag.countsl = sl.SliderId;
                ViewBag.countvg = vg.CourseVideoId;
                ViewBag.loggedInUserId = loggedInUser;
                return View();
            }
            return RedirectToAction("Login", "User");
        }

        // GET: /Admin/Profile
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var loggedInUserId = HttpContext.Session.GetString("loggedinuser");
            var loggedInUserRole = HttpContext.Session.GetString("loggedinuserRole");

            if (string.IsNullOrEmpty(loggedInUserId) || loggedInUserRole != "Admin")
                return RedirectToAction("Login", "User");

            var adminUser = await _context.users
                .Include(u => u.role) // If you renamed to Role, change here
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == loggedInUserId
                                           && u.role != null
                                           && u.role.RoleName == "Admin");

            if (adminUser == null)
            {
                TempData["Error"] = "Admin profile not found or access denied.";
                return RedirectToAction("Index");
            }

            return View(adminUser);
        }


        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var loggedInUserId = HttpContext.Session.GetString("loggedinuser");
            var loggedInUserRole = HttpContext.Session.GetString("loggedinuserRole");

            if (string.IsNullOrEmpty(loggedInUserId) || loggedInUserRole != "Admin")
                return RedirectToAction("Login", "User");

            var user = await _context.users
                .Include(u => u.role)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == loggedInUserId
                                           && u.role != null
                                           && u.role.RoleName == "Admin");

            if (user == null)
            {
                TempData["Error"] = "Admin profile not found or access denied.";
                return RedirectToAction("Index");
            }

            var vm = new EditProfileViewModel
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Mobile = user.Mobile
            };

            return View(vm);
        }

        // POST: /Admin/EditProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            var loggedInUserId = HttpContext.Session.GetString("loggedinuser");
            var loggedInUserRole = HttpContext.Session.GetString("loggedinuserRole");

            if (string.IsNullOrEmpty(loggedInUserId) || loggedInUserRole != "Admin")
                return RedirectToAction("Login", "User");

            // Ensure the model is for the same logged-in user (avoid editing others)
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (!string.Equals(model.UserId, loggedInUserId, StringComparison.Ordinal))
            {
                TempData["Error"] = "Invalid profile edit request.";
                return RedirectToAction("Index");
            }

            var user = await _context.users
                .Include(u => u.role)
                .FirstOrDefaultAsync(u => u.UserId == loggedInUserId
                                           && u.role != null
                                           && u.role.RoleName == "Admin");

            if (user == null)
            {
                TempData["Error"] = "Admin profile not found or access denied.";
                return RedirectToAction("Index");
            }

            // Update only allowed fields
            user.FirstName = model.FirstName?.Trim() ?? string.Empty;
            user.LastName = model.LastName?.Trim() ?? string.Empty;
            user.Mobile = model.Mobile?.Trim() ?? string.Empty;

            try
            {
                await _context.SaveChangesAsync();
                HttpContext.Session.SetString(
                            "loggedinuserFullName",
                            $"{user.FirstName} {user.LastName}".Trim()
                        );

                TempData["Success"] = "Profile updated successfully.";
                return RedirectToAction(nameof(Profile));
            }
            catch (DbUpdateConcurrencyException)
            {
                TempData["Error"] = "Profile update failed due to a concurrency conflict. Please try again.";
                return View(model);
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "An error occurred while updating your profile. Please try again.";
                return View(model);
            }
        }

        public async Task<IActionResult> StudentDetails()
        {
            var students = await _context.studentBatchDetails
                .Include(sbd => sbd.User)
                    .ThenInclude(u => u.role)
                .Include(sbd => sbd.Batch)
                    .ThenInclude(b => b.Course)   // ✅ include course for each batch
                .Include(sbd => sbd.Payment)
                    .ThenInclude(p => p.PaymentType)
                .Where(sbd => sbd.User.role.RoleName == "Student")
                .GroupBy(sbd => sbd.User)   // group by student
                .Select(g => new
                {
                    Student = g.Key,
                    BatchDetails = g.ToList()
                })
                .ToListAsync();

            return View(students);
        }




    }
}