using OTCMS.Filters;

using OTCMS.IRepository;

using OTCMS.Components;

using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Mvc.Rendering;

using Microsoft.EntityFrameworkCore;


namespace OTCMS.Controllers

{

    [ServiceFilter(typeof(ActionLogFilter))]

    public class FeedbackController : Controller

    {

        private readonly IFeedbackRepository _feedbackRepository;

        public FeedbackController(IFeedbackRepository feedbackRepository)

        {

            _feedbackRepository = feedbackRepository;

        }

        // GET: feedbacks

        public async Task<IActionResult> Index()

        {

            /* int x, y, z;

             x = 0;

             y = 10;

             z = 0;

             x = y / z; */

            var feedbacks = await _feedbackRepository.GetAllAsync();

            return View(feedbacks);

        }

        // GET: Feedback/Details/1

        public async Task<IActionResult> Details(int? id)

        {

            if (id == null)

            {

                return NotFound();

            }

            var feedback = await _feedbackRepository.GetByIdAsync(Convert.ToInt32(id));

            if (feedback == null)

            {

                return NotFound();

            }

            return View(feedback);

        }

        // GET: Feedback/Create

        public IActionResult Create()

        {

            return View();

        }

        [HttpPost]

        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Create([Bind("Name,Subject,Description,Phone,Email")] Feedback feedback)

        {

            // ✅ Get logged-in userId (email) from session

            string? userId = HttpContext.Session.GetString("loggedinuser");

            if (string.IsNullOrWhiteSpace(userId))

                return RedirectToAction("Login", "User");

            // ✅ always store email from session (don’t take from UI)

            feedback.Email = userId;

            feedback.Status = FbStatus.Open;

            feedback.RemarksByAdmin = "";

            feedback.CreatedOn = DateTime.Now;

            // ✅ DEBUG: show validation errors if not valid

            if (!ModelState.IsValid)

            {

                // Optional: you can inspect errors in Debug output

                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();

                return View(feedback);

            }

            await _feedbackRepository.InsertAsync(feedback);

            await _feedbackRepository.SaveAsync();

            return RedirectToAction(nameof(MyFeedbacks));

        }

        [HttpGet]

        public async Task<ActionResult<Feedback>> Edit(int? id)

        {

            if (id == null)

            {

                return NotFound();

            }

            var feedback = await _feedbackRepository.GetByIdAsync(Convert.ToInt32(id));

            if (feedback == null)

            {

                return NotFound();

            }

            ViewBag.StatusList = new SelectList(

          Enum.GetValues(typeof(FbStatus)).Cast<FbStatus>()

              .Select(s => new { Id = (int)s, Name = s.ToString() }),

          "Id", "Name", (int)feedback.Status

      );

            return View(feedback); // Edit.cshtml + feedback object

        }

        [HttpPost]

        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Edit(int id, [Bind("FeedbackId,Name,Subject,Description,Email,Phone,Status, RemarksByAdmin, CreatedOn")] Feedback feedback)

        {

            if (id != feedback.FeedbackId)

            {

                return NotFound();

            }

            if (ModelState.IsValid)

            {

                try

                {

                    await _feedbackRepository.UpdateAsync(feedback);

                    await _feedbackRepository.SaveAsync();

                }

                catch (DbUpdateConcurrencyException)

                {

                    var emp = await _feedbackRepository.GetByIdAsync(feedback.FeedbackId);

                    if (emp == null)

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

            return View(feedback);

        }

        public async Task<IActionResult> Delete(int? id)

        {

            if (id == null)

            {

                return NotFound();

            }

            var feedback = await _feedbackRepository.GetByIdAsync(Convert.ToInt32(id));

            if (feedback == null)

            {

                return NotFound();

            }

            return View(feedback);

        }

        [HttpPost, ActionName("Delete")]

        [ValidateAntiForgeryToken]

        public async Task<IActionResult> DeleteConfirmed(int id)

        {

            var feedback = await _feedbackRepository.GetByIdAsync(id);

            if (feedback != null)

            {

                await _feedbackRepository.DeleteAsync(id);

                await _feedbackRepository.SaveAsync();

            }

            return RedirectToAction(nameof(Index));

        }

        [HttpGet]

        public async Task<IActionResult> MyFeedbacks()

        {

            // ✅ Student login stored in session as loggedinuser

            string? userId = HttpContext.Session.GetString("loggedinuser");

            if (string.IsNullOrWhiteSpace(userId))

            {

                // If not logged in, go to login page

                return RedirectToAction("Login", "User");

            }

            // userId is email in your system

            var myFeedbacks = await _feedbackRepository.GetByUserEmailAsync(userId);

            return View(myFeedbacks); // Views/Feedback/MyFeedbacks.cshtml

        }

    }


}

