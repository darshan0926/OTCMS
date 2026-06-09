using OTCMS.Components;

namespace OTCMS.IRepository
{
    public interface IFeedbackRepository
    {
        Task<IEnumerable<Feedback>> GetAllAsync();
        Task<Feedback?> GetByIdAsync(int feedbackId);
        Task InsertAsync(Feedback feedback);
        Task UpdateAsync(Feedback feedback);
        Task DeleteAsync(int feedbackId);
        Task SaveAsync();

        Task<IEnumerable<Feedback>> GetByUserEmailAsync(string email);
    }
}