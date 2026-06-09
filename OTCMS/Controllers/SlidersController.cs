using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OTCMS.Dal;
using OTCMS.Components;

namespace OTCMS.Controllers
{
    public class SlidersController : Controller
    {
        private readonly OtcmsDbContext _context;

        public SlidersController(OtcmsDbContext context)
        {
            _context = context;
        }

        // GET: Sliders
        public async Task<IActionResult> Index()
        {
            string loggedInUser = HttpContext.Session.GetString("loggedinuser");
            string loggedinuserRole = HttpContext.Session.GetString("loggedinuserRole");
            if (loggedInUser == null || loggedinuserRole != "Admin")
                return RedirectToAction("Login", "User");

            ViewBag.loggedInUserId = loggedInUser;
            return View(await _context.sliders.ToListAsync());
        }

        // GET: Sliders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var slider = await _context.sliders
                .FirstOrDefaultAsync(m => m.SliderId == id);
            if (slider == null)
            {
                return NotFound();
            }

            return View(slider);
        }

        // GET: Sliders/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Sliders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SliderId,SliderName,SliderStatus,SliderText,SliderDescriptionText,SliderOrderNo,CreatedOn,SliderUrl,SliderImage")] Slider slider)
        {
            //        if (slider.SliderImage == null || slider.SliderImage.Length == 0 ||
            //slider.SliderImage.Length > 524288 ||
            //Path.GetExtension(slider.SliderImage.FileName) != ".jpg" || slider.SliderImage.ContentType != "image/png")
            //        {
            //            ModelState.AddModelError("SliderImage", "Invalid Image");
            //            return View(slider); // Create.cshtml with slider object
            //        }

            //        // image save logic
            //        string fileName = slider.SliderImage.FileName;
            //        string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

            //        using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            //        {
            //            await slider.SliderImage.CopyToAsync(fs);
            //            slider.SliderImagePath = "/images" + fileName;
            //        }
            var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };

            if (slider.SliderImage == null ||

                slider.SliderImage.Length == 0 ||

                slider.SliderImage.Length > 13097152 || // maybe increase to 11MB for animations

                !allowedTypes.Contains(slider.SliderImage.ContentType))

            {

                ModelState.AddModelError("SliderImage", "Please select a valid image (JPEG/PNG/GIF/WebP, max 11MB).");

                return View(slider);

            }

            bool exists = await _context.sliders

                            .AnyAsync(x => x.SliderOrderNo == slider.SliderOrderNo);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(slider.SliderImage.FileName);

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))

            {

                await slider.SliderImage.CopyToAsync(fs);

                slider.SliderImagePath = "/images/" + fileName;

            }


            if (ModelState.IsValid)
            {
                _context.Add(slider);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(slider);
        }

        // GET: Sliders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var slider = await _context.sliders.FindAsync(id);
            if (slider == null)
            {
                return NotFound();
            }
            return View(slider);
        }

        // POST: Sliders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
     int id,
     [Bind("SliderId,SliderName,SliderStatus,SliderText,SliderDescriptionText,SliderOrderNo,CreatedOn,SliderUrl,SliderImage,SliderImagePath")]
    Slider slider)
        {
            if (id != slider.SliderId)
                return NotFound();

            // Load the existing entity (tracked) to avoid overposting and preserve fields not posted
            var existing = await _context.sliders.FirstOrDefaultAsync(s => s.SliderId == id);
            if (existing == null)
                return NotFound();

            // Validate duplicate order number (excluding this slider)
            bool duplicateOrderNo = await _context.sliders
                .AnyAsync(s => s.SliderOrderNo == slider.SliderOrderNo && s.SliderId != slider.SliderId);
            if (duplicateOrderNo)
            {
                ModelState.AddModelError(nameof(slider.SliderOrderNo), "Slider order number already exists.");
            }

            // If a NEW image is uploaded, validate & replace; otherwise keep the old image
            if (slider.SliderImage != null && slider.SliderImage.Length > 0)
            {
                var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
                const long maxBytes = 2 * 1024 * 1024; // 2 MB

                if (!allowedTypes.Contains(slider.SliderImage.ContentType))
                {
                    ModelState.AddModelError(nameof(slider.SliderImage), "Only JPEG/PNG/GIF/WebP images are allowed.");
                }
                if (slider.SliderImage.Length > maxBytes)
                {
                    ModelState.AddModelError(nameof(slider.SliderImage), "Image must be less than 2 MB.");
                }

                if (!ModelState.IsValid)
                {
                    // Ensure the view has the current path so the preview (if any) can still work
                    slider.SliderImagePath = existing.SliderImagePath;
                    return View(slider);
                }

                // Save new image
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(slider.SliderImage.FileName);
                var imagesRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                Directory.CreateDirectory(imagesRoot); // ensure folder exists
                var filePath = Path.Combine(imagesRoot, fileName);

                using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    await slider.SliderImage.CopyToAsync(fs);
                }

                // Optionally delete the old image file if it exists and path is local
                if (!string.IsNullOrWhiteSpace(existing.SliderImagePath))
                {
                    var oldPhysicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot",
                        existing.SliderImagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(oldPhysicalPath))
                    {
                        try { System.IO.File.Delete(oldPhysicalPath); } catch { /* log if needed */ }
                    }
                }

                // Set the new path
                existing.SliderImagePath = "/images/" + fileName;
            }
            else
            {
                // No new image uploaded: KEEP the existing image path
                // (Do nothing; we will not overwrite it)
            }

            // Update the rest of the editable fields
            existing.SliderName = slider.SliderName;
            existing.SliderStatus = slider.SliderStatus;
            existing.SliderText = slider.SliderText;
            existing.SliderDescriptionText = slider.SliderDescriptionText;
            existing.SliderOrderNo = slider.SliderOrderNo;
            existing.SliderUrl = slider.SliderUrl;

            // Preserve CreatedOn from existing or use whatever your domain dictates
            // existing.CreatedOn = existing.CreatedOn; // implicit, but make sure you don't overwrite it elsewhere

            if (!ModelState.IsValid)
            {
                // Populate the model passed back to the view with the image path so UI can render it
                slider.SliderImagePath = existing.SliderImagePath;
                return View(slider);
            }

            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.sliders.AnyAsync(s => s.SliderId == id))
                    return NotFound();
                throw;
            }
        }

        // GET: Sliders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var slider = await _context.sliders
                .FirstOrDefaultAsync(m => m.SliderId == id);
            if (slider == null)
            {
                return NotFound();
            }

            return View(slider);
        }

        // POST: Sliders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var slider = await _context.sliders.FindAsync(id);
            if (slider != null)
            {
                _context.sliders.Remove(slider);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SliderExists(int id)
        {
            return _context.sliders.Any(e => e.SliderId == id);
        }
    }
}
