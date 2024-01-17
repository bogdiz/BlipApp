using BlipApp.Data;
using BlipApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using System.Security.Policy;

namespace BlipApp.Controllers
{
    public class GroupsController : Controller
    {
        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        public GroupsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            db = context;

            _userManager = userManager;

            _roleManager = roleManager;
        }



        [Authorize(Roles = "Registered,Admin")]
        public IActionResult Show(int id)
        {
            var groups = db.Groups.Include("Messages").Include(g => g.UserInGroup).Where(b => b.Id == id).FirstOrDefault();
            SetAccessRights();

            return View(groups);
        }

        [HttpPost]
        [Authorize(Roles = "Registered,Admin")]
        public IActionResult Show([FromForm] Message message)
        {
            message.Date = DateTime.Now;

            message.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                db.Messages.Add(message);
                db.SaveChanges();
                return Redirect("/Groups/Show/" + message.GroupId);
            }
            else
            {
                Group grp = db.Groups.Include("UserInGroup")
                                         .Include("Messages")
                                         .Include("User")
                                         .Include("Messages.User")
                                         .Where(pst => pst.Id == message.GroupId)
                                         .First();

                return View(grp);
            }
        }

        [Authorize(Roles = "Registered,Admin")]
        public IActionResult New()
        {
            Group group = new Group();
            return View(group);
        }

        [Authorize(Roles = "Registered,Admin")]
        [HttpPost]
        public IActionResult New(Group group)
        {
            if (ModelState.IsValid)
            {
                db.Groups.Add(group);
                db.SaveChanges();

                var userId = _userManager.GetUserId(User);
                var userInGroup = new UserInGroup
                {
                    UserId = userId,
                    GroupId = group.Id
                };

                db.UserInGroups.Add(userInGroup);
                db.SaveChanges();

                TempData["message"] = "The group has been created";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index", "Posts");
            }
            else
            {
                return View(group);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Registered,Admin")]
        public IActionResult AddUser([FromForm] UserInGroup userInGroup)
        {
            if (ModelState.IsValid)
            {
                if (db.UserInGroups
                    .Where(uig => uig.GroupId == userInGroup.GroupId)
                    .Where(uig => uig.UserId == userInGroup.UserId)
                    .Count() == 0)
                {
                    db.UserInGroups.Add(userInGroup);
                    db.SaveChanges();

                    TempData["message"] = "Joined";
                    TempData["messageType"] = "alert-success";

                }
            }
            else
            {
                TempData["message"] = "Couldn't join group";
                TempData["messageType"] = "alert-danger";
            }
            return RedirectToAction("Index", "Posts");

        }

        [Authorize(Roles = "Registered,Admin")]
        public IActionResult Edit(int id)
        {
            var group = db.Groups.Include(g => g.UserInGroup).Where(b => b.Id == id).FirstOrDefault();
            var groupCreator = group.UserInGroup.FirstOrDefault()?.UserId;

            if (groupCreator == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                return View(group);
            }
            else
            {
                TempData["message"] = "You can't modify a group that's not yours!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Posts");
            }
        }

        [Authorize(Roles = "Registered,Admin")]
        [HttpPost]
        public IActionResult Edit(int id, Group requestGroup)
        {
            Group group = db.Groups.Find(id);
            if (ModelState.IsValid)
            {
                group.Title = requestGroup.Title;

                TempData["message"] = "The group has been modified";
                TempData["messageType"] = "alert-success";
                db.SaveChanges();
            }
            return RedirectToAction("Index", "Posts");
        }


        [HttpPost]
        public IActionResult Delete(int id)
        {
            var group = db.Groups.Include(g => g.UserInGroup).Include(g => g.Messages).Where(b => b.Id == id).FirstOrDefault();
            var groupCreator = group.UserInGroup.FirstOrDefault()?.UserId;

            if (groupCreator == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                db.UserInGroups.RemoveRange(group.UserInGroup);
                db.Groups.Remove(group);
                db.SaveChanges();

                TempData["message"] = "The group has been deleted";
                TempData["messageType"] = "alert-success";
            }
            else
            {
                TempData["message"] = "You can't delete a group that's not yours!";
                TempData["messageType"] = "alert-danger";
            }
            return RedirectToAction("Index", "Posts");
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