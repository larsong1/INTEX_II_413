using INTEX_II_413.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace INTEX_II_413
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var services = builder.Services;
            var configuration = builder.Configuration;

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddDbContext<IntexContext>(options => options.UseSqlServer(builder.Configuration["ConnectionStrings:WaterConnection"]));

            builder.Services.AddScoped<IIntexRepository, EFIntexRepository>();

            builder.Services.AddRazorPages();

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession();

            builder.Services.AddScoped<Cart>(sp => SessionCart.GetCart(sp));
            builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // add identity user and roles
            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<IntexContext>();

            // Configure HSTS
            builder.Services.AddHsts(options =>
            {
                options.Preload = true;
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromDays(30);
            });

            // password preferences
            builder.Services.Configure<IdentityOptions>(options =>
            {
                // strong password settings.
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 14;
                options.Password.RequiredUniqueChars = 6;
            });

            // third party auth
            services.AddAuthentication().AddMicrosoftAccount(microsoftOptions =>
            {
                microsoftOptions.ClientId = configuration["Authentication:Microsoft:ClientId"];
                microsoftOptions.ClientSecret = configuration["Authentication:Microsoft:ClientSecret"];
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");

                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // redirect http to https
            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseSession();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapControllerRoute("pagenumandtype", "{projectType}/{pageNum}", new { Controller = "Home", action = "Products" });
            app.MapControllerRoute("pagination", "{pageNum}", new { Controller = "Home", Action = "Products", pageNum = 1 });
            app.MapControllerRoute("projecttype", "{projectType}", new { Controller = "Home", Action = "Products", pageNum = 1 });



            app.MapDefaultControllerRoute();

            app.MapRazorPages();

            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                var roles = new[] { "Admin", "Customer" };

                foreach (var role in roles)
                {
                    // checks if roles exists
                    if (!await roleManager.RoleExistsAsync(role))
                        await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            app.Run();

        }
    }
}

