using LeaveManagement.Domain.Enums;

namespace LeaveManagement.Application.DTOs
{
    /// <summary>
    /// Data transfer object containing a summary representation of a leave request for list views.
    /// </summary>
    public class LeaveRequestSummaryDto
    {
        /// <summary>
        /// The unique identifier of the leave request.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The type of leave requested.
        /// </summary>
        public LeaveType LeaveType { get; set; }

        /// <summary>
        /// The total number of calculated leave days.
        /// </summary>
        public decimal NoOfDays { get; set; }

        /// <summary>
        /// The reason provided for the leave request.
        /// </summary>
        public string? Reason { get; set; }

        /// <summary>
        /// The current processing status of the leave request.
        /// </summary>
        public LeaveStatus LeaveStatus { get; set; }
    }
}

