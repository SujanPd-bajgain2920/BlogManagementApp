using BlogManagementApp.Models;
using BlogManagementApp.Security;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogManagementApp.Controllers
{
    public class BlogController : Controller
    {
        private readonly BlogManagementSystemContext _context;
        private readonly IWebHostEnvironment _env;


        public BlogController( BlogManagementSystemContext context, IWebHostEnvironment env)
        { 
            _env = env;
            _context = context;
        }

        // GET: BlogController
        public ActionResult Index()
        {
            var blogs = _context.BlogContents.Include(b=> b.UploadUser).Select(e => new BlogContentEdit
            {
                Bid = e.Bid,
                SectionDescription = e.SectionDescription,
                SectionHeading = e.SectionHeading,
                SectionImage = e.SectionImage,

                UploadUserId = e.UploadUserId,
                UploadUserName = e.UploadUser.FullName,
                UserProfile = e.UploadUser.UsePhoto,
                Postdate = e.Postdate
            }).ToList();
            return View(blogs);
          
        }

        // GET: BlogController/Details/5
        public ActionResult Details(int id)
        {
            var u = _context.BlogContents.Where(x => x.Bid == id).First();
            return View(u);
        }

        // GET: BlogController/Create
        public ActionResult AddBlog()
        {
            return View();

        }

        // POST: BlogController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddBlog(BlogContentEdit edit)
        {
            short maxid;
            try
            {
                // any and max are Linq
                // if data is present plus 1.
                if (_context.BlogContents.Any())
                    maxid = Convert.ToInt16(_context.BlogContents.Max(x => x.Bid + 1));
                else
                    maxid = 1;
                edit.Bid = maxid;

                if (edit.BlogFile != null)
                {
                    string fileName = Guid.NewGuid() + Path.GetExtension(edit.BlogFile.FileName);
                    // webRootPath is for directory 
                    string filePath = Path.Combine(_env.WebRootPath, "BlogImage", fileName);
                    using (FileStream stream = new FileStream(filePath, FileMode.Create))
                    {
                        edit.BlogFile.CopyTo(stream);
                    }
                    edit.SectionImage = fileName;
                }

                BlogContent p = new()
                {
                    Bid = edit.Bid,
                    SectionHeading = edit.SectionHeading,
                    SectionDescription = edit.SectionDescription,
                    SectionImage = edit.SectionImage,
                    Postdate = edit.Postdate,
                    UploadUserId = Convert.ToInt16(User.Identity.Name)
                };

                _context.Add(p);
                _context.SaveChanges();
                return Content("success");
            }
            catch
            {
                return View();
            }
        }

        // GET: BlogController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: BlogController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: BlogController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: BlogController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
