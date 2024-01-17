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
    public class ApplicationUsersController : Controller
    {
        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly IWebHostEnvironment _env;


        public ApplicationUsersController(
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

        public IActionResult Index()
        {
           
            ApplicationUser currentUser = new ApplicationUser();
            currentUser.Id = _userManager.GetUserId(User);

            if (currentUser.Id == null)
            {
                
                return NotFound();
            }

            db.ApplicationUsers.Add(currentUser);
            db.SaveChanges();

            return View();
        }

        [Authorize(Roles = "Registered,Admin")]
        public IActionResult Show(string id)
        {


            ApplicationUser user = db.ApplicationUsers
                                         .Include("Comments")
                                         .Include("Posts")  

                                         .Where(user => user.Id == id)
                                         .First();

            if(user.ProfileImage == null)
            {
                user.ProfileImage = "/profileUploads/default.jpg";
            }
            user.FollowedByCount = db.UserFollowers.Where(uf => uf.FollowerId == id).Count();
            user.FollowersCount = db.UserFollowers.Where(uf => uf.UserId == id).Count();

            SetAccessRights();

            return View(user);
        }

        public IActionResult Search(string path)
        {
            List<ApplicationUser> users = db.Users.Where(usr => usr.UserName.Contains(path))
                                            .Include("Posts").ToList();
            foreach(ApplicationUser user in users)
            {
                user.FollowedByCount = db.UserFollowers.Where(uf => uf.FollowerId == user.Id).Count();
                user.FollowersCount = db.UserFollowers.Where(uf => uf.UserId == user.Id).Count();
            }
            SetAccessRights();

            return View(users);
        }
        public IActionResult Edit(string id)
        {
            ApplicationUser user = db.ApplicationUsers.Where(usr => usr.Id == id).First();
            if (user.ProfileImage == null)
            {
                user.ProfileImage = "/profileUploads/default.jpg";
            }
            return View(user);
        }

        [Authorize(Roles = "Registered,Admin")]
        [HttpPost]
        public async Task<IActionResult> Edit(string id, ApplicationUser requestUser, IFormFile? PostImage)
        {
            ApplicationUser user = db.ApplicationUsers.Find(id);

            if (PostImage != null && PostImage.Length > 0)
            {
                var storagePath = Path.Combine(
                    _env.WebRootPath,
                    "profileUploads",
                    PostImage.FileName);

                var databaseFileName = "/profileUploads/" + PostImage.FileName;
                using (var fileStream = new FileStream(storagePath, FileMode.Create))
                {
                    await PostImage.CopyToAsync(fileStream);
                }
                requestUser.ProfileImage = databaseFileName;
            }

            if (ModelState.IsValid)
            {
                user.UserName = requestUser.UserName;
                user.FollowedByCount = requestUser.FollowedByCount;
                user.FollowersCount = requestUser.FollowersCount;
                user.ProfileVisibility = requestUser.ProfileVisibility;
                user.ProfileImage = requestUser.ProfileImage;

                TempData["message"] = "Profilul a fost modificat";
                TempData["messageType"] = "alert-success";
                db.SaveChanges();
            }

            return RedirectToAction("Show", new { id = user.Id});
        }

        [HttpPost]
        public IActionResult EditFollower([FromForm] UserFollower userFollower)
        {


            if (ModelState.IsValid)
            {
                
                db.UserFollowers.Add(userFollower);
                db.SaveChanges();

                TempData["message"] = "Followed";
                TempData["messageType"] = "alert-success";

            }
            else
            {
                TempData["message"] = "Couldn't follow";
                TempData["messageType"] = "alert-danger";

            }

            return RedirectToAction("Index", "Posts");
            
        }

        [HttpPost]
        public IActionResult RemoveFollower([FromForm] UserFollower userFollower)
        {
            UserFollower ufllw = db.UserFollowers?.Where(uf => uf.FollowerId == userFollower.FollowerId && uf.UserId == userFollower.UserId).FirstOrDefault();
            
            db.UserFollowers.Remove(ufllw);
            db.SaveChanges();

            TempData["message"] = "Unfollowed";
            TempData["messageType"] = "alert-danger";
            
            return RedirectToAction("Index", "Posts");
        }


        [NonAction]
        public IEnumerable<ApplicationUser> GetAllUsers()
        {
            
            var selectList = new List<ApplicationUser>();
            
            var users = from user in db.ApplicationUsers
                             select user;
            
            foreach (var user in users)
            {

                selectList.Add(user);
            }
            return selectList;
        }

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