using LeaveManagement.Domain.Enums;

namespace LeaveManagement.Domain.Entities
{
    public class LeaveAudit
    {
        public Guid AuditId { get; set; }
        public Guid LeaveRequestId { get; set; }
        public int AuditorId { get; set; }
        public DateTime ProcessDateTime { get; set; }
        public string? Comments { get; set; }
        public LeaveAction Action { get; set; }
    }
}
