using BlipApp.Data;
using BlipApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Security.Claims;

namespace BlipApp.Controllers
{
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly IWebHostEnvironment _env;

        public PostsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IWebHostEnvironment env
            )
        {
            db = context;

            _userManager = userManager;

            _roleManager = roleManager;

            _env = env;
        }

        [Authorize(Roles = "Registered,Admin")]
        [HttpPost]
        public async Task<IActionResult> UploadImage(Post post, IFormFile? PostImage, IFormFile? PostVideo)
        {
            if (PostImage != null && PostImage.Length > 0)
            {
                var storagePath = Path.Combine(
                    _env.WebRootPath,
                    "uploads",
                    PostImage.FileName);

                var databaseFileName = "/uploads/" + PostImage.FileName;
                using (var fileStream = new FileStream(storagePath, FileMode.Create))
                {
                    await PostImage.CopyToAsync(fileStream);
                }
                post.Image = databaseFileName;
            }

            if (PostVideo != null && PostVideo.Length > 0)
            {
                var storagePath = Path.Combine(
                    _env.WebRootPath,
                    "uploads",
                    PostVideo.FileName);

                var databaseFileName = "/uploads/" + PostVideo.FileName;
                using (var fileStream = new FileStream(storagePath, FileMode.Create))
                {
                    await PostVideo.CopyToAsync(fileStream);
                }
                post.Video = databaseFileName;

            }

            post.UserId = _userManager.GetUserId(User);
            post.Date = DateTime.Now;

            if (ModelState.IsValid)
            {
                TempData["message"] = "Postarea a fost adaugată";
                TempData["messageType"] = "alert-success";
                db.Posts.Add(post);
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Registered,Admin")]
        public async Task<IActionResult> EditImage(int id, Post requestPost, IFormFile? PostImage, IFormFile? PostVideo)
        {
            Post post = db.Posts.Find(id);

            if (PostImage != null && PostImage.Length > 0)
            {
                var storagePath = Path.Combine(
                    _env.WebRootPath,
                    "uploads",
                    PostImage.FileName);

                var databaseFileName = "/uploads/" + PostImage.FileName;
                using (var fileStream = new FileStream(storagePath, FileMode.Create))
                {
                    await PostImage.CopyToAsync(fileStream);
                }
                requestPost.Image = databaseFileName;
            }

            if (PostVideo != null && PostVideo.Length > 0)
            {
                var storagePath = Path.Combine(
                    _env.WebRootPath,
                    "uploads",
                    PostVideo.FileName);

                var databaseFileName = "/uploads/" + PostVideo.FileName;
                using (var fileStream = new FileStream(storagePath, FileMode.Create))
                {
                    await PostVideo.CopyToAsync(fileStream);
                }
                requestPost.Video = databaseFileName;

            }

            if (ModelState.IsValid)
            {
                post.Content = requestPost.Content;
                post.Image = requestPost.Image;
                post.Video = requestPost.Video;

                TempData["message"] = "Postarea a fost modificata";
                TempData["messageType"] = "alert-success";
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        [AllowAnonymous]
        [Authorize(Roles = "Unregistered,Registered,Admin")]
        public IActionResult Index()
        {
            var posts = db.Posts.Include("User").ToList();
            ViewBag.Posts = posts;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            var search = "";
            if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
            {
                search = Convert.ToString(HttpContext.Request.Query["search"]).Trim();

                List<ApplicationUser> users = db.Users.Where(usr => usr.UserName.Contains(search)).ToList();

                return RedirectToAction("Search", "ApplicationUsers", new { path = search });

            }

            var groups = from grp in db.Groups
                         select grp;

            ViewBag.Groups = groups;

            ViewBag.SearchString = search;

            return View();
        }

        [AllowAnonymous]
        [Authorize(Roles = "Unregistered,Registered,Admin")]
        public IActionResult Show(int id)
        {
            Post post = db.Posts.Include("User")
                                         .Include("Comments")
                                         .Include("Comments.User")
                                         .Where(pst => pst.Id == id)
                                         .First();


            SetAccessRights();

            return View(post);
        }

        [Authorize(Roles = "Registered,Admin")]
        [HttpPost]
        public IActionResult HandleLikes(int id)
        {
            var post = db.Posts.Find(id);

            if (post != null)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                bool userLiked = UserHasLikedPost(id, userId);

                if (!userLiked)
                {
                    post.Likes++;
                    SetUserLikedCookie(id, userId);
                }
                else
                {
                    post.Likes--;
                    RemoveUserLikedCookie(id, userId);
                }

                db.Update(post);
                db.SaveChanges();

                return Json(new { likes = post.Likes });
            }


            return Json(new { error = "post not found" });

        }

        private bool UserHasLikedPost(int id, string userId)
        {
            var likedCookie = Request.Cookies[$"UserLikedPost_{userId}_{id}"];
            return likedCookie == "true";
        }

        private void SetUserLikedCookie(int id, string userId)
        {
            Response.Cookies.Append($"UserLikedPost_{userId}_{id}", "true");
        }

        private void RemoveUserLikedCookie(int id, string userId)
        {
            Response.Cookies.Delete($"UserLikedPost_{userId}_{id}");
        }




        [HttpPost]
        [Authorize(Roles = "Registered,Admin")]
        public IActionResult Show([FromForm] Comment comment)
        {
            comment.Date = DateTime.Now;

            comment.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                db.Comments.Add(comment);
                db.SaveChanges();
                return Redirect("/Posts/Show/" + comment.PostId);
            }
            else
            {
                Post pst = db.Posts.Include("User")
                                         .Include("Comments")
                                         .Include("Comments.User")
                                         .Where(pst => pst.Id == comment.PostId)
                                         .First();


                SetAccessRights();

                return View(pst);
            }
        }

        [Authorize(Roles = "Registered,Admin")]
        public IActionResult Edit(int id)
        {

            Post post = db.Posts.Where(pst => pst.Id == id)
                                        .First();


            if (post.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                return View(post);
            }

            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unei postari care nu vă aparține";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

        }

        [HttpPost]
        [Authorize(Roles = "Registered,Admin")]
        public ActionResult Delete(int id)
        {
            Post post = db.Posts.Include("Comments")
                                         .Where(pst => pst.Id == id)
                                         .First();

            if (post.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                db.Posts.Remove(post);
                db.SaveChanges();
                TempData["message"] = "Postarea a fost stearsă";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }

            else
            {
                TempData["message"] = "Nu aveți dreptul să ștergeți o postare care nu vă aparține";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }


        // Conditiile de afisare a butoanelor de editare si stergere
        private void SetAccessRights()
        {
            ViewBag.AfisareButoane = false;

            if (User.IsInRole("Registered"))
            {
                ViewBag.AfisareButoane = true;
            }

            ViewBag.EsteAdmin = User.IsInRole("Admin");

            ViewBag.UserCurent = _userManager.GetUserId(User);
        }


    }
}