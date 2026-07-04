using LeaveManagement.Domain.Enums;

namespace LeaveManagement.Application.DTOs
{
    /// <summary>
    /// Data transfer object representing the detailed information of a leave request.
    /// </summary>
    public class LeaveRequestDto
    {
        /// <summary>
        /// The unique identifier of the leave request.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The date and time when the leave request was submitted (UTC).
        /// </summary>
        public DateTime SubmittedDate { get; set; }

        /// <summary>
        /// The type of leave requested.
        /// </summary>
        public LeaveType LeaveType { get; set; }

        /// <summary>
        /// The start date of the requested leave period.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// The end date of the requested leave period.
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// The total number of calculated leave days.
        /// </summary>
        public decimal NoOfDays { get; set; }

        /// <summary>
        /// The reason provided for the leave request.
        /// </summary>
        public string? Reason { get; set; }

        /// <summary>
        /// The current processing status of the leave request (e.g., Pending, Approved, Rejected, Canceled).
        /// </summary>
        public LeaveStatus LeaveStatus { get; set; }

        /// <summary>
        /// The list of workflow audit logs associated with this leave request.
        /// </summary>
        public List<LeaveAuditDto>? LeaveAudits { get; set; }
    }

    /// <summary>
    /// Data transfer object representing a specific step or change in the leave request workflow.
    /// </summary>
    public class LeaveAuditDto
    {
        /// <summary>
        /// The unique identifier of the audit record.
        /// </summary>
        public Guid AuditId { get; set; }

        /// <summary>
        /// The ID of the employee (auditor) who processed this workflow step.
        /// </summary>
        public int AuditorId { get; set; }

        /// <summary>
        /// The date and time when this step was processed (UTC).
        /// </summary>
        public DateTime ProcessDateTime { get; set; }

        /// <summary>
        /// Comments or justifications supplied during this workflow transition.
        /// </summary>
        public string? Comments { get; set; }

        /// <summary>
        /// The workflow action that was taken (e.g., Approved, Rejected, Canceled).
        /// </summary>
        public LeaveAction Action { get; set; }
    }
}
