using Microsoft.AspNetCore.Mvc;

using Microsoft.EntityFrameworkCore;

using OTCMS.Dal;

using OTCMS.Components;

using System.Linq;

using System.Threading.Tasks;

namespace OTCMS.Controllers

{

    public class UserLogsController : Controller

    {

        private readonly OtcmsDbContext _context;

        public UserLogsController(OtcmsDbContext context)

        {

            _context = context;

        }

        // Admin view: list all user login/logout logs

        public async Task<IActionResult> Index(string searchTerm)
        {
            var query = _context.userlogs
                .Include(l => l.User)
                    .ThenInclude(u => u.role)
                .Where(l => l.User.role.RoleName == "Student") // only students
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(l =>
                    (l.User.FirstName + " " + l.User.LastName).Contains(searchTerm) ||
                    l.User.Email.Contains(searchTerm));
            }

            var logs = await query
                .OrderByDescending(l => l.LoginTime)
                .ToListAsync();

            return View(logs);
        }



    }

}

