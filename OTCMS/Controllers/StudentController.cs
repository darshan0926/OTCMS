
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OTCMS.Dal;
using OTCMS.Components;
using OTCMS.Components.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace OTCMS.Controllers
{
    public class StudentController : Controller
    {
        private readonly OtcmsDbContext _context;

        public StudentController(OtcmsDbContext context)
        {
            _context = context;
        }

        // GET: /Student
       


public async Task<IActionResult> Index(string? search, int? courseId)
    {
        var loggedInUser = HttpContext.Session.GetString("loggedinuser");
        var role = HttpContext.Session.GetString("loggedinuserRole");
            

            if (loggedInUser == null || role != "Student")
            return RedirectToAction("Login", "User");

        ViewBag.loggedInUserId = loggedInUser;

        // Base: active courses
        var courseQuery = _context.courses
            .Where(c => c.Status)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            courseQuery = courseQuery.Where(c => c.NameOfTheCourse.Contains(search));

        if (courseId.HasValue)
            courseQuery = courseQuery.Where(c => c.CourseId == courseId.Value);

        var courses = await courseQuery
            .OrderBy(c => c.NameOfTheCourse)
            .ToListAsync();

        // Related batches for the chosen courses only
        var courseIds = courses.Select(c => c.CourseId).ToList();

        var batches = await _context.batches
            .Include(b => b.Course) // optional, for Course.Name display
            .Where(b => b.Status && courseIds.Contains(b.CourseId))
            .OrderBy(b => b.BatchName)
            .AsNoTracking()
            .ToListAsync();

        // Optional: apply search to batches as well
        if (!string.IsNullOrWhiteSpace(search))
        {
            batches = batches
                .Where(b => b.BatchName.Contains(search)
                         || (b.Course != null && b.Course.NameOfTheCourse.Contains(search)))
                .ToList();
        }



            return View(courseQuery);
        }

        public async Task<IActionResult> CourseList(int id)

        {

            // Must be a logged-in Student

            var loggedInUser = HttpContext.Session.GetString("loggedinuser");

            var role = HttpContext.Session.GetString("loggedinuserRole");

            if (loggedInUser == null || role != "Student")

                return RedirectToAction("Login", "User");

            // Load the course with only active batches

            var course = await _context.courses

                .Include(c => c.Batches.Where(b => b.Status))

                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null)

                return NotFound();

            // Resolve current user

            var currentUser = await _context.users

                .FirstOrDefaultAsync(u => u.UserId == loggedInUser);

            if (currentUser == null)

                return Unauthorized();

            // Enrolled batchIds for this user

            var enrolledBatchIds = await _context.studentBatchDetails

                .Where(e => e.UserId == currentUser.Id)

                .Select(e => e.BatchId)

                .ToListAsync();

            // Batches for which user already has Active/Approved payment for this course

            var paidBatchIds = await _context.payments

                .Where(p => p.Id == currentUser.Id
      && p.CourseId == id
      && (p.PaymentStatus == PaymentStatus.Active || p.PaymentStatus == PaymentStatus.Approved))

                .Select(p => p.BatchId)

                .Distinct()

                .ToListAsync();

            ViewBag.EnrolledBatchIds = enrolledBatchIds;

            ViewBag.PaidBatchIds = paidBatchIds;

            return View(course);

        }

        public async Task<IActionResult> BatchDetails(int id)

        {
            string loggedInUser = HttpContext.Session.GetString("loggedinuser");
            string loggedinuserRole = HttpContext.Session.GetString("loggedinuserRole");
           
            if (loggedInUser == null || loggedinuserRole != "Student")
                return RedirectToAction("Login", "User");

            ViewBag.loggedInUserId = loggedInUser;

            var batch = await _context.batches

                .Include(b => b.Course)

                .FirstOrDefaultAsync(b => b.BatchId == id);

            if (batch == null)

                return NotFound();

            var contents = await _context.batchContents

                .Where(c => c.BatchId == id)

                .ToListAsync();

            var vm = new BatchDetailsViewModel

            {

                Course = batch.Course!,

                Batch = batch,

                Contents = contents

            };

            return View(vm);

        }
        public async Task<IActionResult> SelectCourseStudent()
        {
            string loggedInUser = HttpContext.Session.GetString("loggedinuser");
            string loggedinuserRole = HttpContext.Session.GetString("loggedinuserRole");
            
            if (loggedInUser == null || loggedinuserRole != "Student")
                return RedirectToAction("Login", "User");

            ViewBag.loggedInUserId = loggedInUser;
            var activeCourses = _context.courses
                .Where(c => c.Status == true)                // only active
                .OrderBy(c => c.NameOfTheCourse)       // optional: nice sorting
                .ToList();

            ViewData["CourseId"] = new SelectList(activeCourses, "CourseId", "NameOfTheCourse");

            return View();  // SelectTopic.cshtml 

        }

        [HttpPost]
        public async Task<IActionResult> GetAllVideosByCourseIdStudent(int CourseId)

        {
            string loggedInUser = HttpContext.Session.GetString("loggedinuser");
            string loggedinuserRole = HttpContext.Session.GetString("loggedinuserRole");
           
            if (loggedInUser == null || loggedinuserRole != "Student")
                return RedirectToAction("Login", "User");

            ViewBag.loggedInUserId = loggedInUser;
            List<Video> videoList = await _context.videos.Include("course").Where(v => v.CourseId == CourseId).ToListAsync();

            Course? tp = await _context.courses.FindAsync(CourseId);

            ViewBag.NameOfTheCourse = tp.NameOfTheCourse;

            return View("DisplayVideosStudent", videoList); // DisplayVideos.cshtml with listobject

        }



        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var loggedInUserId = HttpContext.Session.GetString("loggedinuser");
            var loggedInUserRole = HttpContext.Session.GetString("loggedinuserRole");

            if (string.IsNullOrEmpty(loggedInUserId) || loggedInUserRole != "Student")
                return RedirectToAction("Login", "User");

            var studentUser = await _context.users
                .Include(u => u.role)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == loggedInUserId
                                           && u.role != null
                                           && u.role.RoleName == "Student");

            if (studentUser == null)
            {
                TempData["Error"] = "Student profile not found or access denied.";
                return RedirectToAction("Index");
            }

            ViewBag.loggedInUserId = loggedInUserId;
            return View(studentUser);
        }

        // GET: /Student/EditProfile
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var loggedInUserId = HttpContext.Session.GetString("loggedinuser");
            var loggedInUserRole = HttpContext.Session.GetString("loggedinuserRole");

            if (string.IsNullOrEmpty(loggedInUserId) || loggedInUserRole != "Student")
                return RedirectToAction("Login", "User");

            var student = await _context.users
                .Include(u => u.role)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == loggedInUserId
                                           && u.role != null
                                           && u.role.RoleName == "Student");

            if (student == null)
            {
                TempData["Error"] = "Student profile not found or access denied.";
                return RedirectToAction("Index");
            }

            var vm = new EditProfileViewModel
            {
                UserId = student.UserId,
                FirstName = student.FirstName,
                LastName = student.LastName,
                Mobile = student.Mobile
            };

            ViewBag.loggedInUserId = loggedInUserId;
            return View(vm);
        }

        // POST: /Student/EditProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            var loggedInUserId = HttpContext.Session.GetString("loggedinuser");
            var loggedInUserRole = HttpContext.Session.GetString("loggedinuserRole");

            if (string.IsNullOrEmpty(loggedInUserId) || loggedInUserRole != "Student")
                return RedirectToAction("Login", "User");

            if (!ModelState.IsValid)
            {
                // Return the same view with validation messages
                return View(model);
            }

            // Ensure the posting user is the logged-in student
            if (!string.Equals(model.UserId, loggedInUserId, StringComparison.Ordinal))
            {
                TempData["Error"] = "Invalid profile edit request.";
                return RedirectToAction("Index");
            }

            var student = await _context.users
                .Include(u => u.role)
                .FirstOrDefaultAsync(u => u.UserId == loggedInUserId
                                           && u.role != null
                                           && u.role.RoleName == "Student");

            if (student == null)
            {
                TempData["Error"] = "Student profile not found or access denied.";
                return RedirectToAction("Index");
            }

            // Update only allowed fields
            student.FirstName = model.FirstName?.Trim() ?? string.Empty;
            student.LastName = model.LastName?.Trim() ?? string.Empty;
            student.Mobile = model.Mobile?.Trim() ?? string.Empty;

            try
            {
                await _context.SaveChangesAsync();


                HttpContext.Session.SetString(
                            "loggedinuserFullName",
                            $"{student.FirstName} {student.LastName}".Trim()
                        );


                TempData["Success"] = "Profile updated successfully.";
                return RedirectToAction(nameof(Profile)); // PRG: show message on Profile
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



        public async Task<IActionResult> Pay(int batchId, int courseId)

        {

            var loggedInUser = HttpContext.Session.GetString("loggedinuser");

            var role = HttpContext.Session.GetString("loggedinuserRole");

            if (loggedInUser == null || role != "Student")

                return RedirectToAction("Login", "User");

            var batch = await _context.batches

                .Include(b => b.Course)

                .AsNoTracking()

                .FirstOrDefaultAsync(b => b.BatchId == batchId && b.CourseId == courseId);

            if (batch == null || !batch.Status || batch.Course == null || !batch.Course.Status)

                return NotFound();

            var paymentTypes = await _context.paymentTypes

                .Where(t => t.PaymentTypeId == 1 || t.PaymentTypeId == 2) // Debit/Credit only

                .OrderBy(t => t.PaymentTypeId)

                .AsNoTracking()

                .ToListAsync();

            var vm = new PaymentCreateViewModel

            {

                CourseId = courseId,

                BatchId = batchId,

                PaymentAmount = batch.Fee,

                CourseName = batch.Course.NameOfTheCourse,

                BatchName = batch.BatchName,

                BatchFee = batch.Fee,

                PaymentTypeOptions = new SelectList(paymentTypes, "PaymentTypeId", "PaymentTypeName")

            };

            return View(vm); // Views/Student/Pay.cshtml

        }

        // ===========================

        // PAYMENT: POST (process)

        // URL: /Student/Pay

        // ===========================

        [HttpPost]

        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Pay(PaymentCreateViewModel vm)

        {

            var loggedInUser = HttpContext.Session.GetString("loggedinuser");

            var role = HttpContext.Session.GetString("loggedinuserRole");

            if (loggedInUser == null || role != "Student")

                return RedirectToAction("Login", "User");

            // Resolve current user (numeric Id) from session UserId (string)

            var currentUser = await _context.users.FirstOrDefaultAsync(u => u.UserId == loggedInUser);

            if (currentUser == null) return Unauthorized();

            // Validate batch & course

            var batch = await _context.batches

                .Include(b => b.Course)

                .FirstOrDefaultAsync(b => b.BatchId == vm.BatchId && b.CourseId == vm.CourseId);

            if (batch == null || batch.Course == null) return NotFound();

            // Basic input validations

            if (vm.PaymentAmount != batch.Fee)
            {
                var oldAmount = vm.PaymentAmount;

                // For the View: show banner
                ViewBag.FeeChanged = true;
                ViewBag.OldAmount = oldAmount;
                ViewBag.NewAmount = batch.Fee;

                // IMPORTANT: Update model and clear ModelState for PaymentAmount
                // so the hidden input re-renders with the NEW fee value
                ModelState.Remove(nameof(PaymentCreateViewModel.PaymentAmount));
                vm.PaymentAmount = batch.Fee;
                vm.BatchFee = batch.Fee;

                // Add model-level message shown in ValidationSummary
                ModelState.AddModelError(string.Empty,
                    $"Course fee has changed from ₹{oldAmount:N0} to ₹{batch.Fee:N0}. The amount has been updated below. Please review and submit again.");
            }
            if (vm.PaymentTypeId != 1 && vm.PaymentTypeId != 2)

                ModelState.AddModelError(nameof(vm.PaymentTypeId), "Invalid payment type.");

            if (string.IsNullOrWhiteSpace(vm.CardNumberPlain) || vm.CardNumberPlain.Length != 16)

                ModelState.AddModelError(nameof(vm.CardNumberPlain), "Card number must be 16 digits.");

            if (string.IsNullOrWhiteSpace(vm.ExpiryMMYY))

                ModelState.AddModelError(nameof(vm.ExpiryMMYY), "Expiry required.");
            else

            {

                // Validate MM/YY is in the future (card valid through the end of the month)

                // Expected formats passing your regex: "MM/YY" e.g., "10/25"

                try

                {

                    var parts = vm.ExpiryMMYY.Trim().Split('/');

                    var mm = int.Parse(parts[0]);

                    var yy = int.Parse(parts[1]);

                    // Normalize 2-digit year as 2000+YY (e.g., "25" -> 2025)

                    var year = 2000 + yy;

                    // Card is valid through the END of the expiry month.

                    // So we consider it expired only AFTER (year, month) month ends.

                    var expiryEndExclusive = new DateTime(year, mm, 1).AddMonths(1); // first day of next month

                    // Use UTC consistently, since you store PaymentOn in UTC

                    if (DateTime.UtcNow >= expiryEndExclusive)

                    {

                        ModelState.AddModelError(nameof(vm.ExpiryMMYY), "Card is expired.");

                    }

                }

                catch

                {

                    // Defensive: in case someone bypassed client-side regex

                    ModelState.AddModelError(nameof(vm.ExpiryMMYY), "Invalid expiry format.");

                }

            }


            if (string.IsNullOrWhiteSpace(vm.CCV))

                ModelState.AddModelError(nameof(vm.CCV), "CCV required.");

            // --- Duplicate guards (very important) ---

            // 1) Already enrolled? (Enrollment happens after Admin approval, but guard anyway.)

            var alreadyEnrolled = await _context.studentBatchDetails

                .AnyAsync(e => e.UserId == currentUser.Id && e.BatchId == vm.BatchId);

            if (alreadyEnrolled)

                ModelState.AddModelError("", "You are already enrolled in this batch.");

            // 2) Existing Active/Approved payment for this batch?

            var hasActiveOrApprovedPayment = await _context.payments.AnyAsync(p =>

                p.Id == currentUser.Id &&

                p.BatchId == vm.BatchId &&

                (p.PaymentStatus == PaymentStatus.Active || p.PaymentStatus == PaymentStatus.Approved));

            if (hasActiveOrApprovedPayment)

                ModelState.AddModelError("", "You already have a pending/approved payment for this batch.");

            if (!ModelState.IsValid)

            {

                var types = await _context.paymentTypes

                    .Where(t => t.PaymentTypeId == 1 || t.PaymentTypeId == 2)

                    .OrderBy(t => t.PaymentTypeId)

                    .AsNoTracking()

                    .ToListAsync();

                vm.PaymentTypeOptions = new SelectList(types, "PaymentTypeId", "PaymentTypeName");

                vm.CourseName = batch.Course.NameOfTheCourse;

                vm.BatchName = batch.BatchName;

                vm.BatchFee = batch.Fee;

                return View(vm);

            }

            // --- Create Payment as Inactive ---

            var payment = new Payment

            {

                // IMPORTANT: use UserId (not Id)

                Id = currentUser.Id,

                BatchId = vm.BatchId,

                CourseId = vm.CourseId,

                PaymentTypeId = vm.PaymentTypeId,

                PaymentAmount = vm.PaymentAmount,

                PaymentOn = DateTime.UtcNow,

                PaymentStatus = PaymentStatus.Inactive

            };

            _context.payments.Add(payment);

            await _context.SaveChangesAsync(); // Generates PaymentId

            // ----- DUMMY GATEWAY SUCCESS -----

            // Save masked card (store masked; never store plain)

            var last4 = vm.CardNumberPlain!.Substring(vm.CardNumberPlain.Length - 4);

            var masked = $"**** **** **** {last4}";

            var paymentCard = new PaymentCard

            {

                PaymentId = payment.PaymentId,

                CardNumber = masked,

                CardHolderName = vm.CardHolderName ?? string.Empty,

                ExpiryDate = vm.ExpiryMMYY!,

                CCV = vm.CCV // mapped in DB (you chose to store for dummy use)

            };

            _context.paymentCards.Add(paymentCard);

            // Mark payment as Active (student paid)

            payment.PaymentStatus = PaymentStatus.Active;

            await _context.SaveChangesAsync();

            // NOTE: DO NOT ENROLL HERE (enrollment occurs after Admin approves)

            return RedirectToAction(nameof(PaymentSuccess), new { id = payment.PaymentId });

        }


        // ===========================

        // PAYMENT: Success page

        // URL: /Student/PaymentSuccess/{id}

        // ===========================

        public async Task<IActionResult> PaymentSuccess(int id)

        {

            var loggedInUser = HttpContext.Session.GetString("loggedinuser");

            var role = HttpContext.Session.GetString("loggedinuserRole");

            if (loggedInUser == null || role != "Student")

                return RedirectToAction("Login", "User");

            var payment = await _context.payments

                .Include(p => p.Course)

                .Include(p => p.Batch)

                .Include(p => p.PaymentType)

                .Include(p => p.PaymentCard)

                .Include(p => p.User)

                .FirstOrDefaultAsync(p => p.PaymentId == id);

            if (payment == null) return NotFound();

            return View(payment); // Views/Student/PaymentSuccess.cshtml

        }


        public async Task<IActionResult> MyBatches()
        {
            var loggedInUser = HttpContext.Session.GetString("loggedinuser");
            var role = HttpContext.Session.GetString("loggedinuserRole");

            if (loggedInUser == null || role != "Student")
                return RedirectToAction("Login", "User");

            ViewBag.loggedInUserId = loggedInUser;

            // Resolve current user
            var currentUser = await _context.users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == loggedInUser);

            if (currentUser == null)
                return Unauthorized();

            // Get enrolled batches for this student
            // studentBatchDetails: (UserId -> currentUser.Id) and BatchId
            var enrolledBatchIds = await _context.studentBatchDetails
                .Where(e => e.UserId == currentUser.Id)
                .Select(e => e.BatchId)
                .Distinct()
                .ToListAsync();

            if (!enrolledBatchIds.Any())
            {
                // return empty view model list
                return View(new List<MyBatchItemViewModel>());
            }

            // Fetch batches + course + some useful computed info
            var myBatches = await _context.batches
                .Where(b => enrolledBatchIds.Contains(b.BatchId))
                .Include(b => b.Course)
                .AsNoTracking()
                .Select(b => new MyBatchItemViewModel
                {
                    BatchId = b.BatchId,
                    BatchName = b.BatchName,

                    CourseId = b.CourseId,
                    CourseName = b.Course != null ? b.Course.NameOfTheCourse : "",

                    StartDate = b.StartDate,
                    EndDate = b.EndDate,

                    Timings = b.Timings,
                    Duration = b.Duration,
                    Fee = b.Fee,

                    BatchStatus = b.Status,

                    ContentCount = _context.batchContents.Count(c => c.BatchId == b.BatchId),
                    NextClassDate = _context.batchContents
                        .Where(c => c.BatchId == b.BatchId && c.OnDate >= DateTime.Today)
                        .OrderBy(c => c.OnDate)
                        .Select(c => (DateTime?)c.OnDate)
                        .FirstOrDefault(),
                    LastClassDate = _context.batchContents
                        .Where(c => c.BatchId == b.BatchId && c.OnDate < DateTime.Today)
                        .OrderByDescending(c => c.OnDate)
                        .Select(c => (DateTime?)c.OnDate)
                        .FirstOrDefault()
                })
                .OrderBy(x => x.CourseName)
                .ThenBy(x => x.BatchName)
                .ToListAsync();

            return View(myBatches);
        }


        public async Task<IActionResult> MyPayments(string? search, PaymentStatus? status)
        {
            var loggedInUser = HttpContext.Session.GetString("loggedinuser");
            var role = HttpContext.Session.GetString("loggedinuserRole");

            if (loggedInUser == null || role != "Student")
                return RedirectToAction("Login", "User");

            ViewBag.loggedInUserId = loggedInUser;

            // resolve current student (numeric Id)
            var currentUser = await _context.users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == loggedInUser);

            if (currentUser == null)
                return Unauthorized();

            // Base query: only THIS student's payments
            var query = _context.payments
                .Where(p => p.Id == currentUser.Id)
                .Include(p => p.Course)
                .Include(p => p.Batch)
                .Include(p => p.PaymentType)
                .Include(p => p.PaymentCard)
                .AsNoTracking()
                .AsQueryable();

            // Optional: filter by status
            if (status.HasValue)
                query = query.Where(p => p.PaymentStatus == status.Value);

            // Optional: search (course/batch/paymentId)
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(p =>
                    (p.Course != null && p.Course.NameOfTheCourse.Contains(search)) ||
                    (p.Batch != null && p.Batch.BatchName.Contains(search)) ||
                    p.PaymentId.ToString().Contains(search)
                );
            }

            var payments = await query
                .OrderByDescending(p => p.PaymentOn)
                .Select(p => new MyPaymentItemViewModel
                {
                    PaymentId = p.PaymentId,

                    CourseId = p.CourseId,
                    CourseName = p.Course != null ? p.Course.NameOfTheCourse : "",

                    BatchId = p.BatchId,
                    BatchName = p.Batch != null ? p.Batch.BatchName : "",

                    PaymentTypeId = p.PaymentTypeId,
                    PaymentTypeName = p.PaymentType != null ? p.PaymentType.PaymentTypeName : "",

                    PaymentAmount = p.PaymentAmount,
                    PaymentOn = p.PaymentOn,
                    PaymentStatus = p.PaymentStatus,

                    MaskedCardNumber = p.PaymentCard != null ? p.PaymentCard.CardNumber : null,
                    CardHolderName = p.PaymentCard != null ? p.PaymentCard.CardHolderName : null
                })
                .ToListAsync();

            // pass filters back to view
            ViewData["CurrentSearch"] = search;
            ViewData["CurrentStatus"] = status?.ToString();

            return View(payments);
        }

    }
}
