using BlogManagementApp.Models;
using BlogManagementApp.Security;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace BlogManagementApp.Controllers
{
    public class StaticController : Controller
    {
        private readonly BlogManagementSystemContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IDataProtector _protector;

        public StaticController(BlogManagementSystemContext context, IWebHostEnvironment env, DataSecurityProvider key, IDataProtectionProvider provider)
        {
            _context = context;
            _env = env;
            _protector = provider.CreateProtector(key.Key);
        }
        public async Task <IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            var blogs = await _context.BlogContents.Include(b => b.UploadUser).Select(e => new BlogContentEdit
            {
                Bid = e.Bid,
                SectionDescription = e.SectionDescription,
                SectionHeading = e.SectionHeading,
                SectionImage = e.SectionImage,
                UploadUserId = e.UploadUserId,
                UploadUserName = e.UploadUser.FullName,
                UserProfile = e.UploadUser.UsePhoto,
                Postdate = e.Postdate,
                EncId = _protector.Protect(e.Bid.ToString())
            }).ToListAsync();
            return View(blogs);
        }
    }
}
