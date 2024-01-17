using System.ComponentModel.DataAnnotations;

namespace BlipApp.Models
{
    public class Group
    {
        [Key]
        public int Id { get; set; }
        public virtual ICollection<UserInGroup>? UserInGroup { get; set; }

        [Required(ErrorMessage = "Titlul este obligatoriu")]
        [StringLength(100, ErrorMessage = "Titlul nu poate avea mai mult de 20 de caractere")]
        public string Title { get; set; }

        public virtual ICollection<Message>? Messages { get; set; } 

    }
}
