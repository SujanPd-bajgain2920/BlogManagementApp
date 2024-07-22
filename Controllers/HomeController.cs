using BlogManagementApp.Models;
using BlogManagementApp.Security;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Reflection.Metadata;

namespace BlogManagementApp.Controllers
{

    public class HomeController : Controller
    {
        
        private readonly IDataProtector _protector;
        private readonly BlogManagementSystemContext _context;

        // used for image
        private readonly IWebHostEnvironment _env;

        public HomeController( DataSecurityProvider key, IDataProtectionProvider provider, BlogManagementSystemContext context, IWebHostEnvironment env)
        {
           
            _protector = provider.CreateProtector(key.Key);
            _context = context;
            _env = env;
         }

        public IActionResult Index()
        {
            List<UserList> users = _context.UserLists.ToList();
            var u = users.Select(e => new UserListEdit
            {
                UserId = e.UserId,
                FullName = e.FullName,
                CurrentAddress = e.CurrentAddress,
                EmailAddress = e.EmailAddress,
                UsePhoto = e.UsePhoto,
                
                UserPassword = e.UserPassword,
                UserRole = e.UserRole,
                EncId = _protector.Protect(e.UserId.ToString())

            }).ToList();
            ViewData["Users"] = users;

            return View(u);
        }
        public IActionResult Details(string id)
        {
            
            int userid= Convert.ToInt32(_protector.Unprotect(id));

            var u = _context.UserLists.Where(x => x.UserId == userid).First();
             return View(u);
            //return Json(u);

        }

       /* public IActionResult Users()
        {
            List<UserList> users = _context.UserLists.ToList();
            var u = users.Select(e =>  new UserListEdit
            {
                UserId=e.UserId,
                FullName=e.FullName,
                CurrentAddress=e.CurrentAddress,
                EmailAddress=e.EmailAddress,
                UsePhoto=e.UsePhoto,
                UserPassword=e.UserPassword,
                UserRole=e.UserRole,

            }).ToList();
            ViewData["Users"] = users;
                
            return View(u);
        }*/

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        public IActionResult Create(UserListEdit u)
        {

            try
            {
                var users = _context.UserLists.Where(x=>x.EmailAddress==u.EmailAddress).FirstOrDefault();
                if (users == null)
                {

                    Random r = new Random();
                    HttpContext.Session.SetString("token", r.Next(9999).ToString());
                    var token = HttpContext.Session.GetString("token");


                    short maxid;
                    if (_context.UserLists.Any())
                        maxid = Convert.ToInt16(_context.UserLists.Max(x => x.UserId) + 1);
                    else
                        maxid = 1;
                    u.UserId = maxid;


                    if (u.UserFile != null)
                    {
                        string fileName = "UserImage" + Guid.NewGuid() + Path.GetExtension(u.UserFile.FileName);
                        string filePath = Path.Combine(_env.WebRootPath, "UserImage", fileName);
                        using (FileStream stream = new FileStream(filePath, FileMode.Create))
                        {
                            u.UserFile.CopyTo(stream);
                        }
                        u.UsePhoto = fileName;
                    }


                    UserList userList = new()
                    {
                        EmailAddress = u.EmailAddress,
                        FullName = u.FullName,
                        CurrentAddress = u.CurrentAddress,
                        UsePhoto = u.UsePhoto,
                        UserId = u.UserId,
                        UserPassword = _protector.Protect(u.UserPassword),
                        UserRole = u.UserRole
                    };


                    _context.Add(userList);
                    _context.SaveChanges();
                    return RedirectToAction("Login", "Account");

                }
                else
                {
                    ModelState.AddModelError("", "User already exist with this email.!");
                    return View(u);
                }
            }
            catch
            {
                ModelState.AddModelError("", "User Registration Failed. Please try again");
                return View(u);
            }
        }


        

        //partial view
        public IActionResult ProfileImage()
        {
            var p= _context.UserLists.Where(u => u.UserId.Equals(Convert.ToInt16(User.Identity!.Name))).FirstOrDefault();
            ViewData["img"] = p.UsePhoto;
            return PartialView("_Profile"); 
        }

        [HttpGet]
        public IActionResult Edit(string id)
        {
            int userid = Convert.ToInt16(_protector.Unprotect(id));
            var user = _context.UserLists.Where(x => x.UserId == userid).FirstOrDefault();
            if (user == null)
            {
                return NotFound();
            }

            UserListEdit userEdit = new()
            {
                EmailAddress = user.EmailAddress,
                FullName = user.FullName,
                CurrentAddress = user.CurrentAddress,
                UserId = user.UserId,
                UsePhoto = user.UsePhoto
            };

            return View(userEdit);
        }

        [HttpPost]
        public async Task <IActionResult> Edit(UserListEdit u)
        {
           
                var user = _context.UserLists.Find(u.UserId);
                if (user == null)
                {
                    return NotFound();
                }

                if (u.UserFile != null)
                {
                    string fileName = "UserImage" + Guid.NewGuid() + Path.GetExtension(u.UserFile.FileName);
                    string filePath = Path.Combine(_env.WebRootPath, "UserImage", fileName);
                    using (FileStream stream = new FileStream(filePath, FileMode.Create))
                    {
                        u.UserFile.CopyTo(stream);
                    }
                    u.UsePhoto = fileName;
                }

               // return Json(u);

                user.EmailAddress = u.EmailAddress;
                user.FullName = u.FullName;
                user.CurrentAddress = u.CurrentAddress;
                user.UsePhoto = u.UsePhoto;
                user.UserId = u.UserId; //Convert.ToInt16(User.Identity.Name);
                    
               
                _context.Update(user);
               await _context.SaveChangesAsync();
               // return Json(user);
               return RedirectToAction("Index");
        }

        [Authorize]
        [HttpGet]
        public IActionResult ProfileUpdate(int id)
        {
            // UserList domain model
            var p = _context.UserLists.Where(x => x.UserId == Convert.ToInt16(User.Identity!.Name)).First();
            UserListEdit u = new()
            {
                UserId = p.UserId,
                FullName = p.FullName,
                EmailAddress = p.EmailAddress,
                CurrentAddress = p.CurrentAddress,
                UsePhoto = p.UsePhoto,
                UserPassword= p.UserPassword,
                UserRole = p.UserRole
            };
            return View(u);
        }

        [HttpPost]
        [Authorize]
        public IActionResult ProfileUpdate(UserListEdit edit)
        {
            // for image upload
            if (edit.UserFile != null)
            {
                string fileName = "UserImage" + Guid.NewGuid() + Path.GetExtension(edit.UserFile.FileName);
                string filePath = Path.Combine(_env.WebRootPath, "UserImage", fileName);
                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                {
                    edit.UserFile.CopyTo(stream);
                }
                edit.UsePhoto = fileName;
            }
            // mapping
            UserList u = new()
            {
                UserId = edit.UserId,
                FullName = edit.FullName,
                EmailAddress = edit.EmailAddress,
                CurrentAddress = edit.CurrentAddress,
                UsePhoto = edit.UsePhoto,
                UserPassword = edit.UserPassword,
                UserRole = edit.UserRole
            };
           // return Json(u);
            _context.Update(u);
            _context.SaveChanges();
            return RedirectToAction("ProfileUpdate");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
