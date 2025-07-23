using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpendSmart_Backend.Models
{
    public class UserAdmin
    {
        public int UserId { get; set; }
        public int ManagerId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        [ForeignKey("ManagerId")]
        public User Manager { get; set; }
    }
}
