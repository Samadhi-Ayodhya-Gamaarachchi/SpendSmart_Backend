using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpendSmart_Backend.domain.Entities
{
    public class Goal
    {
        [Key]
        public int GoalId { get; set; }
        public string GoalName { get; set; }
        public decimal TargetAmount { get; set; }
        public DateTime EndDate { get; set; }
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;
    }
}
