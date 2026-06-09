using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OTCMS.Dal;
using OTCMS.Components;

namespace OTCMS.Controllers
{
    public class BatchContentsController : Controller
    {
        private readonly OtcmsDbContext _context;

        public BatchContentsController(OtcmsDbContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index(int? batchId, int? currentBatchId)
        {
            string loggedInUser = HttpContext.Session.GetString("loggedinuser");
            string loggedinuserRole = HttpContext.Session.GetString("loggedinuserRole");
           
            if (loggedInUser == null || loggedinuserRole != "Admin")
                return RedirectToAction("Login", "User");

            ViewBag.loggedInUserId = loggedInUser;
            // ✅ Preserve selection when navigating back/refresh
            if (batchId == null)
                batchId = currentBatchId;

            ViewData["CurrentBatchId"] = batchId;

            // ✅ Dropdown data (you can filter Active batches only if you have Status)

            var batches = await _context.batches
                .Where(b => b.Status)
                .OrderBy(b => b.BatchName)
                .ToListAsync();


            ViewData["BatchId"] = new SelectList(batches, "BatchId", "BatchName", batchId);

            // ✅ BatchContent query with Batch include
            var batchContentsQuery = _context.batchContents
                .Include(bc => bc.Batch)
                .AsQueryable();

            // ✅ Apply filter
            if (batchId.HasValue)
            {
                batchContentsQuery = batchContentsQuery
                    .Where(bc => bc.BatchId == batchId.Value);
            }

            return View(await batchContentsQuery.AsNoTracking().ToListAsync());
        }


        // GET: BatchContents/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var batchContent = await _context.batchContents
                .Include(b => b.Batch)
                .FirstOrDefaultAsync(m => m.BCId == id);
            if (batchContent == null)
            {
                return NotFound();
            }

            return View(batchContent);
        }

        // GET: BatchContents/Create
        public IActionResult Create()
        {
            ViewData["BatchId"] = new SelectList(_context.batches, "BatchId", "BatchName");
            return View();
        }


        // POST: BatchContents/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("BCId,BatchId,Topic,OnDate,VideoFile,NotesDocument")] BatchContent batchContent)
        {
            // ✅ 2) Duplicate BatchId check BEFORE saving (prevents DB exception)
            bool batchContentIdExists = await _context.batchContents
                .AsNoTracking()
                .AnyAsync(b => b.BCId == batchContent.BCId);

            if (batchContentIdExists)
            {
                ModelState.AddModelError(nameof(batchContent.BCId), " Batch Content Id already exists. Please enter a unique Batch Content Id.");
            }
            // ✅ Validate & Save Video (optional)
            if (batchContent.VideoFile != null)
            {
                if (batchContent.VideoFile.Length == 0 ||
                    batchContent.VideoFile.Length > 54857600 || // 100 MB
                    (batchContent.VideoFile.ContentType != "video/mp4" &&
                     batchContent.VideoFile.ContentType != "video/webm"))
                {
                    ModelState.AddModelError("VideoFile", "Please select a valid video (MP4/WEBM, max 50 MB).");
                    ViewData["BatchId"] = new SelectList(_context.batches, "BatchId", "BatchName", batchContent.BatchId);
                    return View(batchContent);
                }

                var videoFileName = Guid.NewGuid().ToString() + "_" + batchContent.VideoFile.FileName;
                var videoFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/batchcontentvideos", videoFileName);

                using (var fs = new FileStream(videoFilePath, FileMode.Create, FileAccess.Write))
                {
                    await batchContent.VideoFile.CopyToAsync(fs);
                    batchContent.VideoFilePath = "/batchcontentvideos/" + videoFileName;
                }
            }

            // ✅ Validate & Save Notes (optional, PDF)
            if (batchContent.NotesDocument != null)
            {
                if (batchContent.NotesDocument.ContentType != "application/pdf")
                {
                    ModelState.AddModelError("NotesDocument", "Only PDF files are allowed.");
                    ViewData["BatchId"] = new SelectList(_context.batches, "BatchId", "BatchName", batchContent.BatchId);
                    return View(batchContent);
                }

                if (batchContent.NotesDocument.Length > 5048576) // ~5 MB
                {
                    ModelState.AddModelError("NotesDocument", "PDF file size must be less than 5 MB.");
                    ViewData["BatchId"] = new SelectList(_context.batches, "BatchId", "BatchName", batchContent.BatchId);
                    return View(batchContent);
                }

                var notesFileName = Guid.NewGuid().ToString() + "_" + batchContent.NotesDocument.FileName;
                var notesFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/batchcontentnotes", notesFileName);

                using (var fs = new FileStream(notesFilePath, FileMode.Create, FileAccess.Write))
                {
                    await batchContent.NotesDocument.CopyToAsync(fs);
                    batchContent.NotesDocumentPath = "/batchcontentnotes/" + notesFileName;
                }
            }


            // ✅ Validate OnDate must be within Batch StartDate and EndDate
            var batch = await _context.batches
                .FirstOrDefaultAsync(b => b.BatchId == batchContent.BatchId);

            if (batch == null)
            {
                ModelState.AddModelError("BatchId", "Selected batch not found.");
            }
            else
            {
                // Assuming Batch.StartDate and Batch.EndDate are DateTime in your DB/model
                var startDate = batch.StartDate.Date;
                var endDate = batch.EndDate.Date;

                // ✅ OnDate is DateTime (non-nullable)
                var onDate = batchContent.OnDate.Date;

                if (startDate > endDate)
                {
                    ModelState.AddModelError("", "Batch StartDate cannot be greater than EndDate.");
                }
                else if (onDate < startDate || onDate > endDate)
                {
                    ModelState.AddModelError(
                        "OnDate",
                        $"OnDate must be between {startDate:dd-MMM-yyyy} and {endDate:dd-MMM-yyyy}."
                    );
                }
            }



            if (ModelState.IsValid)
            {
                _context.Add(batchContent);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["BatchId"] = new SelectList(_context.batches, "BatchId", "BatchName", batchContent.BatchId);
            return View(batchContent);
        }



        // GET: BatchContents/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var batchContent = await _context.batchContents.FindAsync(id);
            if (batchContent == null)
            {
                return NotFound();
            }
            ViewData["BatchId"] = new SelectList(_context.batches, "BatchId", "BatchName", batchContent.BatchId);
            return View(batchContent);
        }


        // POST: BatchContents/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("BCId,BatchId,Topic,OnDate,VideoFilePath,NotesDocumentPath,VideoFile,NotesDocument")] BatchContent batchContent)
        {
            if (id != batchContent.BCId)
            {
                return NotFound();
            }

            // ✅ Handle Video replacement (optional)
            if (batchContent.VideoFile != null)
            {
                if (batchContent.VideoFile.Length == 0 ||
                    batchContent.VideoFile.Length > 54857600 || // 100 MB
                    (batchContent.VideoFile.ContentType != "video/mp4" &&
                     batchContent.VideoFile.ContentType != "video/webm"))
                {
                    ModelState.AddModelError("VideoFile", "Please select a valid video (MP4/WEBM, max 100 MB).");
                    ViewData["BatchId"] = new SelectList(_context.batches, "BatchId", "BatchName", batchContent.BatchId);
                    return View(batchContent);
                }

                var videoFileName = Guid.NewGuid().ToString() + "_" + batchContent.VideoFile.FileName;
                var videoFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/batchcontentvideos", videoFileName);

                using (var fs = new FileStream(videoFilePath, FileMode.Create, FileAccess.Write))
                {
                    await batchContent.VideoFile.CopyToAsync(fs);
                    batchContent.VideoFilePath = "/batchcontentvideos/" + videoFileName;
                }
            }

            // ✅ Handle Notes replacement (optional)
            if (batchContent.NotesDocument != null)
            {
                if (batchContent.NotesDocument.ContentType != "application/pdf")
                {
                    ModelState.AddModelError("NotesDocument", "Only PDF files are allowed.");
                    ViewData["BatchId"] = new SelectList(_context.batches, "BatchId", "BatchName", batchContent.BatchId);
                    return View(batchContent);
                }

                if (batchContent.NotesDocument.Length > 6048576) // ~6 MB on edit
                {
                    ModelState.AddModelError("NotesDocument", "PDF file size must be less than 6 MB.");
                    ViewData["BatchId"] = new SelectList(_context.batches, "BatchId", "BatchName", batchContent.BatchId);
                    return View(batchContent);
                }

                var notesFileName = Guid.NewGuid().ToString() + "_" + batchContent.NotesDocument.FileName;
                var notesFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/batchcontentnotes", notesFileName);

                using (var fs = new FileStream(notesFilePath, FileMode.Create, FileAccess.Write))
                {
                    await batchContent.NotesDocument.CopyToAsync(fs);
                    batchContent.NotesDocumentPath = "/batchcontentnotes/" + notesFileName;
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(batchContent);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BatchContentExists(batchContent.BCId))
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
            ViewData["BatchId"] = new SelectList(_context.batches, "BatchId", "BatchName", batchContent.BatchId);
            return View(batchContent);
        }


        // GET: BatchContents/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var batchContent = await _context.batchContents
                .Include(b => b.Batch)
                .FirstOrDefaultAsync(m => m.BCId == id);
            if (batchContent == null)
            {
                return NotFound();
            }

            return View(batchContent);
        }

        // POST: BatchContents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var batchContent = await _context.batchContents.FindAsync(id);
            if (batchContent != null)
            {
                _context.batchContents.Remove(batchContent);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BatchContentExists(int id)
        {
            return _context.batchContents.Any(e => e.BCId == id);
        }
    }
}
