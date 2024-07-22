using BlogManagementApp.Security;
using Microsoft.EntityFrameworkCore;
using BlogManagementApp.Models;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace BlogManagementApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // dependency 
            builder.Services.AddDbContext<BlogManagementSystemContext>(options =>
               options.UseSqlServer(builder.Configuration.GetConnectionString("dbConn"))
                      .EnableSensitiveDataLogging());

            builder.Services.AddSingleton<DataSecurityProvider>();
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(o=>o.LoginPath="/Account/Login"); // o=>o. is lamda expression
            // session add
            builder.Services.AddSession(o =>
            {
                o.IdleTimeout = TimeSpan.FromMinutes(1);
                o.Cookie.HttpOnly = true;
            });

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSession();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Static}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
