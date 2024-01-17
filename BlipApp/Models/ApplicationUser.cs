using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlipApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        public virtual ICollection<Comment>? Comments { get; set; }
        public virtual ICollection<Post>? Posts { get; set; }
        public string? ProfileImage {  get; set; }
        public int FollowersCount { get; set; } = 0;
        public int FollowedByCount { get; set; } = 0;

        [Required]
        public bool ProfileVisibility { get; set; } = false;
        
        public virtual ICollection<UserInGroup>? UserInGroup { get; set; }

    }
}