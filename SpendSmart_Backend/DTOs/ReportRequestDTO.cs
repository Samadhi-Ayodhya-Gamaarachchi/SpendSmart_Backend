using System;
using System.ComponentModel.DataAnnotations;

namespace SpendSmart_Backend.DTOs
{
    public class ReportRequestDto
    {

        public int UserId { get; set; }
        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }
    }
}
