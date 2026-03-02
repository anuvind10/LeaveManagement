using System.ComponentModel.DataAnnotations;

namespace LeaveManagement.Application.DTOs
{
    public class RejectLeaveRequestDto
    {
        [Required]
        public string Comments { get; set; } = string.Empty;
    }
}
