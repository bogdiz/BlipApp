using System.ComponentModel.DataAnnotations.Schema;


namespace BlipApp.Models
{
    public class UserFollower
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? FollowerId { get; set; }
        

    }
}
