using Microsoft.AspNetCore.Mvc;

using Microsoft.EntityFrameworkCore;

using OTCMS.Dal;

using OTCMS.Components;
using OTCMS.Components.ViewModels;

namespace OTCMS.Controllers

{

    public class PaymentsController : Controller

    {

        private readonly OtcmsDbContext _context;

        public PaymentsController(OtcmsDbContext context)

        {

            _context = context;

        }

        // GET: /PaymentsAdmin?status=Active

        // Lists payments based on status; default = Active (awaiting approval)

        public async Task<IActionResult> Index(string? status)
        {
            var loggedInUser = HttpContext.Session.GetString("loggedinuser");
            var role = HttpContext.Session.GetString("loggedinuserRole");

            if (loggedInUser == null || role != "Admin")
                return RedirectToAction("Login", "User");

            // ---------- COUNTS ----------
            var paymentCounts = await _context.payments
                .AsNoTracking()
                .GroupBy(p => p.PaymentStatus)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            ViewBag.ActiveCount = paymentCounts.FirstOrDefault(x => x.Status == PaymentStatus.Active)?.Count ?? 0;
            ViewBag.ApprovedCount = paymentCounts.FirstOrDefault(x => x.Status == PaymentStatus.Approved)?.Count ?? 0;
            ViewBag.RejectedCount = paymentCounts.FirstOrDefault(x => x.Status == PaymentStatus.Rejected)?.Count ?? 0;

            // ---------- FILTER ----------
            IQueryable<Payment> q = _context.payments
                .Include(p => p.User)
                .Include(p => p.Course)
                .Include(p => p.Batch)
                .Include(p => p.PaymentType)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(status) &&
                Enum.TryParse<PaymentStatus>(status, out var parsed))
            {
                q = q.Where(p => p.PaymentStatus == parsed);
            }
            else
            {
                q = q.Where(p => p.PaymentStatus == PaymentStatus.Active);
                status = PaymentStatus.Active.ToString();
            }

            var list = await q
                .OrderByDescending(p => p.PaymentOn)
                .ToListAsync();

            ViewBag.CurrentStatus = status;

            return View("Index", list);
        }


        // GET: /PaymentsAdmin/Details/5  (optional: view a single payment in detail)

        public async Task<IActionResult> Details(int id)

        {

            var loggedInUser = HttpContext.Session.GetString("loggedinuser");

            var role = HttpContext.Session.GetString("loggedinuserRole");

            if (loggedInUser == null || role != "Admin")

                return RedirectToAction("Login", "User");

            var payment = await _context.payments

                .Include(p => p.User)

                .Include(p => p.Course)

                .Include(p => p.Batch)

                .Include(p => p.PaymentType)

                .Include(p => p.PaymentCard)

                .FirstOrDefaultAsync(p => p.PaymentId == id);

            if (payment == null) return NotFound();

            return View("Details", payment);

        }

        // POST: /PaymentsAdmin/Approve/5

        public async Task<IActionResult> Approve(int id)

        {

            var role = HttpContext.Session.GetString("loggedinuserRole");

            if (role != "Admin")

                return RedirectToAction("Login", "User");

            var payment = await _context.payments.FirstOrDefaultAsync(p => p.PaymentId == id);

            if (payment == null) return NotFound();

            if (payment.PaymentStatus != PaymentStatus.Active)

            {

                TempData["msg"] = "Only payments in Active state can be approved.";

                return RedirectToAction(nameof(Index));

            }

            // Make the status change + enrollment atomic

            using var tx = await _context.Database.BeginTransactionAsync();

            try

            {

                // 1) Approve the payment

                payment.PaymentStatus = PaymentStatus.Approved;

                await _context.SaveChangesAsync();

                // 2) Enroll student into batch if not already enrolled

                var alreadyEnrolled = await _context.studentBatchDetails

                    .AnyAsync(e => e.UserId == payment.Id && e.BatchId == payment.BatchId);

                if (!alreadyEnrolled)

                {

                    var enroll = new StudentBatchDetail

                    {

                        UserId = payment.Id,

                        BatchId = payment.BatchId,

                        PaymentId = payment.PaymentId,

                        EnrolledOn = DateTime.UtcNow

                    };

                    _context.studentBatchDetails.Add(enroll);

                    await _context.SaveChangesAsync();

                }

                await tx.CommitAsync();

                TempData["msg"] = "Payment approved and student enrolled to the batch.";

                return RedirectToAction(nameof(Index));

            }

            catch

            {

                await tx.RollbackAsync();

                TempData["msg"] = "Failed to approve payment. Please try again.";

                return RedirectToAction(nameof(Index));

            }

        }


        // POST: /PaymentsAdmin/Reject/5

        [HttpPost]

        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Reject(int id)

        {

            var role = HttpContext.Session.GetString("loggedinuserRole");

            if (role != "Admin")

                return RedirectToAction("Login", "User");

            var payment = await _context.payments.FirstOrDefaultAsync(p => p.PaymentId == id);

            if (payment == null) return NotFound();

            if (payment.PaymentStatus != PaymentStatus.Active)

            {

                TempData["msg"] = "Only payments in Active state can be rejected.";

                return RedirectToAction(nameof(Index));

            }

            payment.PaymentStatus = PaymentStatus.Rejected;

            await _context.SaveChangesAsync();

            TempData["msg"] = "Payment rejected.";

            return RedirectToAction(nameof(Index));

        }

    }

}

