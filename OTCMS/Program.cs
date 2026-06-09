using OTCMS.Dal;

using OTCMS.Filters;

using OTCMS.IRepository;

using OTCMS.Repository;

using Microsoft.EntityFrameworkCore;


namespace OTCMS
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddDbContext<OtcmsDbContext>(options =>
            {
                string cs = builder.Configuration.GetConnectionString("otcmss");
                options.UseSqlServer(cs);
            });
            builder.Services.AddSession();
            builder.Services.AddScoped<IFeedbackRepository, FeedbackRepository>();

            builder.Services.AddScoped<ActionLogFilter>();
            builder.Services.AddScoped<GlobalExceptionFilter>();

            builder.Services.AddControllersWithViews(options =>
            {
                options.Filters.Add<ActionLogFilter>();
                options.Filters.Add<GlobalExceptionFilter>();
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();
            app.UseSession();
            app.UseRouting();

            app.UseAuthorization();

            

            app.MapStaticAssets();
           
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
