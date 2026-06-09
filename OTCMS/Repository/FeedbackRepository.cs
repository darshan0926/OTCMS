using OTCMS.Dal;

using OTCMS.IRepository;

using OTCMS.Components;

using Microsoft.EntityFrameworkCore;

namespace OTCMS.Repository

{

    public class FeedbackRepository : IFeedbackRepository

    {

        private readonly OtcmsDbContext _context;

        public FeedbackRepository(OtcmsDbContext context)

        {

            _context = context;

        }

        public async Task<IEnumerable<Feedback>> GetAllAsync()

        {

            return await _context.feedbacks.ToListAsync();

        }

        public async Task<Feedback?> GetByIdAsync(int feedbackId)

        {

            return await _context.feedbacks.FirstOrDefaultAsync(f => f.FeedbackId == feedbackId);

        }

        public async Task InsertAsync(Feedback feedback)

        {

            await _context.feedbacks.AddAsync(feedback);

        }

        public async Task UpdateAsync(Feedback feedback)

        {

            _context.feedbacks.Update(feedback);

            await Task.CompletedTask;

        }

        public async Task DeleteAsync(int feedbackId)

        {

            var f = await _context.feedbacks.FindAsync(feedbackId);

            if (f != null)

                _context.feedbacks.Remove(f);

        }

        public async Task SaveAsync()

        {

            await _context.SaveChangesAsync(); // ✅ this actually writes to DB

        }

        public async Task<IEnumerable<Feedback>> GetByUserEmailAsync(string email)

        {

            if (string.IsNullOrWhiteSpace(email))

                return new List<Feedback>();

            var normalized = email.Trim().ToLower();

            return await _context.feedbacks

                .Where(f => f.Email != null && f.Email.ToLower() == normalized)

                .OrderByDescending(f => f.CreatedOn)

                .ToListAsync();

        }

    }

}
