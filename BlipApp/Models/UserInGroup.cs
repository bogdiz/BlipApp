using System.ComponentModel.DataAnnotations.Schema;

namespace BlipApp.Models
{
    public class UserInGroup
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string? UserId { get; set; }
        public int? GroupId {  get; set; }
        public virtual ApplicationUser? User { get; set; }
        public virtual Group? Group { get; set; }

        

    }
}
