using Microsoft.EntityFrameworkCore;
using OTCMS.Components;

namespace OTCMS.Dal
{
    public class OtcmsDbContext : DbContext
    {
        public OtcmsDbContext(DbContextOptions<OtcmsDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Course>().ToTable("Course");
            modelBuilder.Entity<Batch>().ToTable("Batch");
            modelBuilder.Entity<Slider>().ToTable("Slider");
            modelBuilder.Entity<Role>().ToTable("Role");
            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<Video>().ToTable("Video");
            modelBuilder.Entity<BatchContent>().ToTable("BatchContent");
            modelBuilder.Entity<UserLog>().ToTable("UserLog");
            modelBuilder.Entity<Payment>().ToTable("Payments");
            modelBuilder.Entity<PaymentCard>().ToTable("PaymentCards");
            modelBuilder.Entity<PaymentType>().ToTable("PaymentTypes");
            modelBuilder.Entity<StudentBatchDetail>().ToTable("StudentBatchDetails");
            modelBuilder.Entity<Feedback>()
                .ToTable("Feedback")
                .Property(f => f.Status)
                .HasConversion<string>()
                .HasMaxLength(15);
        }

        public DbSet<Course> courses { get; set; }
        public DbSet<Batch> batches { get; set; }
        public DbSet<Slider> sliders { get; set; }
        public DbSet<Role> roles { get; set; }
        public DbSet<User> users { get; set; }
        public DbSet<Video> videos { get; set; }
        public DbSet<BatchContent> batchContents { get; set; }
        public DbSet<UserLog> userlogs { get; set; }
        public DbSet<OTCMS.Components.UserLogin> UserLogin { get; set; } = default!;
        public DbSet<Feedback> feedbacks { get; set; } = null!;
        public DbSet<Payment> payments { get; set; } = null!;
        public DbSet<PaymentCard> paymentCards { get; set; } = null!;
        public DbSet<PaymentType> paymentTypes { get; set; } = null!;
        public DbSet<StudentBatchDetail> studentBatchDetails { get; set; } = null!;
        public object Users { get; internal set; }
    }
}