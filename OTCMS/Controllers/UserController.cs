using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OTCMS.Dal;
using OTCMS.Components;
using Microsoft.AspNetCore.Identity;

namespace OTCMS.Controllers
{
    public class UserController : Controller
    {
        private readonly OtcmsDbContext _context;

        public UserController(OtcmsDbContext context)
        {
            // _context = new OrganicVegDbContext();
            _context = context;
        }

        // --- AUTHENTICATION ACTIONS ---
        public IActionResult Login() // returns login.cshtml (view)
        {
            return View(); // Login.cshtml + _Layout.cshtml
        }
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(); // ForgotPassword.cshtml
        }

        [HttpPost]

        public IActionResult VerifyEmail(User model)

        {

            // 1. Check if email is null or empty before hitting the database

            if (string.IsNullOrEmpty(model.Email))

            {

                ModelState.AddModelError("Email", "Email is required");

                return View("ForgotPassword", model);

            }

            // 2. Look for the user in the database

            var user = _context.users.FirstOrDefault(u => u.Email.ToLower() == model.Email.ToLower());

            // 3. If user doesn't exist, stay on page and show error

            if (user == null)

            {

                ModelState.AddModelError("Email", "This email is not registered");

                return View("ForgotPassword", model);

            }

            // 4. SUCCESS: Redirect and pass ONLY the email string.

            // This creates a clean URL like: /User/ResetPassword?email=user@example.com

            return RedirectToAction("ResetPassword", new { email = user.Email });

        }

        [HttpGet]

        public IActionResult ResetPassword(string email)

        {

            if (string.IsNullOrEmpty(email))

            {

                return RedirectToAction("ForgotPassword");

            }

            //l/ Set ViewBag so the hidden input in your View gets the value

            ViewBag.Email = email;

            return View();

        }

        //[HttpPost]

        //public IActionResult ResetPasswordUser(string email, string newPassword, string confirmPassword)

        //{

        //    // 1. STRICT MATCH CHECK

        //    var user = _context.users.FirstOrDefault(u => u.Email.ToLower() == email.ToLower());

        //    // If they are NOT equal, we stop the process immediately.

        //    if (newPassword != confirmPassword)

        //    {

        //        ModelState.AddModelError("", "The new password and confirmation password do not match.");

        //        ViewBag.Email = email;

        //        // This 'return' stops the code from reaching the _context.SaveChanges() below.

        //        return View("ResetPassword");

        //    }

        //    // 2. SEARCH FOR USER


        //    else if (user != null)

        //    {



        //        // 3. SECURE SAVE

        //        // This part ONLY runs if the 'if' block above was skipped (meaning passwords matched).

        //        user.Password = newPassword;

        //        _context.SaveChanges();

        //        TempData["SuccessMessage"] = "Password reset successfully!";

        //        return RedirectToAction("Login");

        //    }

        //    // 4. FALLBACK

        //    ModelState.AddModelError("", "User session lost. Please try again.");

        //    return View("ResetPassword");

        //}
        //[HttpPost]

        //public IActionResult ResetPasswordUser(string email, string newPassword, string confirmPassword)

        //{

        //    // 0) Basic input guard (optional but good practice)

        //    if (string.IsNullOrWhiteSpace(email))

        //    {

        //        ModelState.AddModelError("", "Email is required.");

        //        return View("ResetPassword");

        //    }

        //    if (string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))

        //    {

        //        ModelState.AddModelError("", "Password fields cannot be empty.");

        //        ViewBag.Email = email;

        //        return View("ResetPassword");

        //    }

        //    // 1) STRICT MATCH CHECK

        //    if (newPassword != confirmPassword)

        //    {

        //        ModelState.AddModelError("", "The new password and confirmation password do not match.");

        //        ViewBag.Email = email;

        //        return View("ResetPassword");

        //    }

        //    // 2) SEARCH FOR USER

        //    var user = _context.users.FirstOrDefault(u => u.Email.ToLower() == email.ToLower());

        //    if (user == null)

        //    {

        //        ModelState.AddModelError("", "User not found.");

        //        return View("ResetPassword");

        //    }

        //    // 3) SECURE SAVE (HASH the password instead of storing plaintext)

        //    var hasher = new PasswordHasher<User>();

        //    user.Password = hasher.HashPassword(user, newPassword); // store hash into existing Password column

        //    _context.SaveChanges();

        //    TempData["SuccessMessage"] = "Reset successfully!";

        //    return RedirectToAction("Login");

        //}
        [HttpPost]
        public IActionResult ResetPasswordUser(string email, string newPassword, string confirmPassword)
        {
            // 0) Basic input guard
            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError("", "Email is required.");
                return View("ResetPassword");
            }

            if (string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                ModelState.AddModelError("", "Password fields cannot be empty.");
                ViewBag.Email = email;
                return View("ResetPassword");
            }

            // 1) LENGTH VALIDATION (Added: 6 to 15 characters)
            if (newPassword.Length < 6 || newPassword.Length > 15)
            {
                ModelState.AddModelError("", "Password must be 6 or more characters.");
                ViewBag.Email = email;
                return View("ResetPassword");
            }

            // 2) STRICT MATCH CHECK
            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "The new password and confirmation password do not match.");
                ViewBag.Email = email;
                return View("ResetPassword");
            }

            // 3) SEARCH FOR USER
            var user = _context.users.FirstOrDefault(u => u.Email.ToLower() == email.ToLower());

            if (user == null)
            {
                ModelState.AddModelError("", "User not found.");
                return View("ResetPassword");
            }

            // 4) SECURE SAVE
            // Using Microsoft.AspNetCore.Identity.PasswordHasher
            var hasher = new PasswordHasher<User>();
            user.Password = hasher.HashPassword(user, newPassword);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "reset successfully!";
            return RedirectToAction("Login");
        }

        public IActionResult Logout() // removing the user logged in details from the session
        {
            // ✅ Find the logged-in user's Id from session

            var loggedUserId = _context.users

                .Where(u => u.UserId == HttpContext.Session.GetString("loggedinuser"))

                .Select(u => u.Id)

                .FirstOrDefault();

            if (loggedUserId != 0)

            {

                var log = _context.userlogs

                    .Where(l => l.Id == loggedUserId && l.LogoutTime == null)

                    .OrderByDescending(l => l.LoginTime)

                    .FirstOrDefault();

                if (log != null)

                {

                    log.LogoutTime = DateTime.Now;

                    _context.SaveChanges();

                }

            }

            HttpContext.Session.Remove("loggedinuser");
            HttpContext.Session.Remove("loggedinuserRole");

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]

        public IActionResult ValidateUser(UserLogin ul)

        {

            if (!ModelState.IsValid)

            {

                return View("Login", ul);

            }

            User? user = _context.users.FirstOrDefault(u => u.UserId.ToLower() == ul.UserId.ToLower());

            if (user == null)

            {

                ModelState.AddModelError(string.Empty, "The username you entered is not registered.");

                return View("Login", ul);

            }

            //if (user.Password != ul.Password)

            //{

            //    ModelState.AddModelError(string.Empty, "The password you entered is incorrect.");

            //    return View("Login", ul);

            //}

            //hashing
            var hasher = new PasswordHasher<User>();
            var result = hasher.VerifyHashedPassword(user, user.Password, ul.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(string.Empty, "The password you entered is incorrect.");
                return View("Login", ul);
            }

            if (user.Status == true)

            {

                // Find the role based on the RoleId

                Role? r = _context.roles.Find(user.RoleId);

                if (r != null)

                {

                    HttpContext.Session.SetString("loggedinuser", ul.UserId);

                    HttpContext.Session.SetString("loggedinuserRole", r.RoleName);

                    HttpContext.Session.SetString("loggedinuserFullName", user.FirstName +" "+ user.LastName);
                    // Store the actual role name

                    // // ✅ Capture IP and insert login log

                    //// string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

                    var log = new UserLog

                    {

                        Id = user.Id,

                        LoginTime = DateTime.Now,



                    };

                    _context.userlogs.Add(log);

                    _context.SaveChanges();

                    if (r.RoleName == "Admin")

                    {

                        return RedirectToAction("Index", "Admin"); // Index.cshtml + _AdminLayout.cshtml

                    }

                    else if (r.RoleName == "Student")

                    {

                        return RedirectToAction("Index", "Student");

                    }

                    else

                    {

                        ModelState.AddModelError(string.Empty, "Your account has an unrecognized role configuration.");

                        return View("Login", ul);

                    }

                }

                else

                {

                    ModelState.AddModelError(string.Empty, "Could not verify user role.");

                    return View("Login", ul);

                }

            }

            else

            {

                ModelState.AddModelError(string.Empty, "Your account is inactive. Please contact the administrator.");

                return View("Login", ul);

            }

        }




        // --- REGISTRATION & PROFILE ACTIONS (Omitted for brevity, assuming they are correct) ---
        public IActionResult Register()
        {
            Role? studentRole = _context.roles.FirstOrDefault(r => r.RoleName == "Student");

            var newUser = new User
            {
                RoleId = studentRole?.RoleId ?? 0,
            };

            if (studentRole != null)
            {
                ViewBag.DefaultRoleName = studentRole.RoleName;
            }

            return View(newUser);
        }

        //public IActionResult Register()
        //{


        //    return View(); // Register.cshtml + _Layout.cshtml
        //}


        [HttpPost]
        public IActionResult RegisterUser(User u)
        {
            // ✅ Server-side assignment only (no view involvement)
            u.Email = u.Email?.Trim();
            u.UserId = u.Email;

            // Always set status to Active by default
            u.Status = true;

            // If you have validation attributes on UserId and they cause ModelState errors,
            // remove the old UserId ModelState entry because user doesn't input it.
            ModelState.Remove("UserId");
            ModelState.Remove("Status"); // remove Status from validation since user doesn't input it

            if (!ModelState.IsValid)
                return View("Register", u);

            try
            {
                // ✅ Now this check is basically "Email already registered"
                if (_context.users.Any(user => user.UserId.ToLower() == u.UserId.ToLower()))
                {
                    // Since user only sees Email, show error on Email
                    ModelState.AddModelError("Email", "This Email is already registered.");
                    return View("Register", u);
                }

                Role? defaultRole = _context.roles.FirstOrDefault(r => r.RoleName == "Student");
                u.RoleId = defaultRole?.RoleId ?? 0;



                // HASH THE PASSWORD

                var hasher = new PasswordHasher<User>();

                u.Password = hasher.HashPassword(u, u.Password);

                _context.users.Add(u);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Registration successful! Please login with your new credentials.";
                return RedirectToAction("Login");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "An unexpected error occurred during registration.");
                return View("Register", u);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id)
        {
            var user = await _context.users.FindAsync(id);
            if (user != null)
            {
                user.Status = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index), "UserLogs");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            var user = await _context.users.FindAsync(id);
            if (user != null)
            {
                user.Status = false;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index), "UserLogs");

        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
