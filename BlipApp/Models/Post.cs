using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System;
using System.Collections.Generic;

namespace BlipApp.Models
{
    public class Post
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Continutul postului este obligatoriu")]
        public string Content {  get; set; }
        public string? Image { get; set; }
        public string? Video { get; set; }
        public DateTime Date { get; set; }
        public int Likes { get; set; } = 0;
        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }
        public virtual ICollection<Comment>? Comments { get; set; }

    }
}
