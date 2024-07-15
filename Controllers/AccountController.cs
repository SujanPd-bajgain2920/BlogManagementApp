using BlogManagementApp.Models;
using BlogManagementApp.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlogManagementApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly BlogManagementSystemContext _context;
        private readonly IDataProtector _protector;

        public AccountController(BlogManagementSystemContext context, DataSecurityProvider p, IDataProtectionProvider provider)
        {
            _context = context;
            _protector = provider.CreateProtector(p.Key);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(UserListEdit uEdit)
        {
            var users = _context.UserLists.ToList();
            if (users != null)
            {    

                var u = users.Where(x => x.EmailAddress.ToUpper().Equals(uEdit.EmailAddress.ToUpper()) && _protector.Unprotect(x.UserPassword).Equals(uEdit.UserPassword)).FirstOrDefault();
                if (u != null) 
                {
                    List<Claim> claims = new()
                    {
                        new Claim(ClaimTypes.Name,u.UserId.ToString()),
                        new Claim(ClaimTypes.Role,u.UserRole),
                        new Claim("FullName",u.FullName),
                        new Claim("image",u.UsePhoto),
                        new Claim("email",u.EmailAddress),
                        new Claim("address",u.CurrentAddress),
                    };

                    var identity = new ClaimsIdentity(claims,CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, 
                        new ClaimsPrincipal(identity));

                    return RedirectToAction("Dashboard");
                
                }
            }
            else 
            {
                ModelState.AddModelError("", "Invalid User");
            
            }
            return View(uEdit);
        }

        public async Task <IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [Authorize]
        public IActionResult Dashboard()
        {
            return RedirectToAction("Index", "Home");
        }
    }
}
