using INTEX_II_413.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Core;
using Microsoft.Extensions.Configuration;

namespace INTEX_II_413
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var services = builder.Services;
            var configuration = builder.Configuration;

            // Retrieve client ID and secret from Azure Key Vault
            var keyVaultUri = new Uri("https://intex-ii-keys.vault.azure.net/");
            var secretClient = new SecretClient(keyVaultUri, new DefaultAzureCredential());

            // Retrieve client ID and secret from Azure Key Vault
            var clientIdSecret = await secretClient.GetSecretAsync("client-id");
            var clientSecretSecret = await secretClient.GetSecretAsync("secret");

            // Add authentication using retrieved client ID and secret
            services.AddAuthentication().AddMicrosoftAccount(microsoftOptions =>
            {
                microsoftOptions.ClientId = clientIdSecret.Value.Value;
                microsoftOptions.ClientSecret = clientSecretSecret.Value.Value;
            });

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
                options.Password.RequiredLength = 16;
                options.Password.RequiredUniqueChars = 6;
            });

            // add cookie notifications
            builder.Services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential 
                // cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;

                options.MinimumSameSitePolicy = SameSiteMode.None;

                options.ConsentCookieValue = "true";
            });

            var app = builder.Build();

            app.Use(async (context, next) =>
            {
                // Generate a nonce
                string cookieNonce = Guid.NewGuid().ToString("N");
                string inlineScriptNonce1 = Guid.NewGuid().ToString("N");
                string inlineScriptNonce2 = Guid.NewGuid().ToString("N");

                // Add nonce to CSP header
                context.Response.Headers.Add("Content-Security-Policy", $"default-src 'self'; script-src 'self' 'nonce-{cookieNonce}' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self' https://www.thesun.co.uk https://www.lego.com https://images.brickset.com data: https://m.media-amazon.com https://www.brickeconomy.com https://localhost:7094; font-src 'self'; connect-src 'self' https://localhost:44337 http://localhost:40904 ws: wss:; frame-src 'self';");

                // Pass the nonce value to your view
                context.Items["CookieCspNonce"] = cookieNonce;

                await next();
            });





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

            app.UseCookiePolicy();

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
