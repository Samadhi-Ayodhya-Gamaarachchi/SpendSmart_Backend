using Microsoft.AspNetCore.Mvc;

namespace SpendSmart_Backend.DTOs
{
    public class UpdateUserEmailDto
    {
        public int UserId { get; set; }
        public string Email { get; set; }
    }
}