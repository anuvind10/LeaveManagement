using LeaveManagement.Domain.Enums;

namespace LeaveManagement.Application.DTOs
{
    /// <summary>
    /// Data transfer object containing parameters needed to submit a new leave request.
    /// </summary>
    public class CreateLeaveRequestDto
    {
        /// <summary>
        /// The type of leave requested (e.g., Annual, Sick, Unpaid).
        /// </summary>
        public LeaveType LeaveType { get; set; }

        /// <summary>
        /// The starting date of the leave period (inclusive).
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// The ending date of the leave period (inclusive).
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// The optional reason or justification for the leave request.
        /// </summary>
        public string? Reason { get; set; }
    }
}
