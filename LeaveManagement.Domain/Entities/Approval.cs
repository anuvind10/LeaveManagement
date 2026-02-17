namespace LeaveManagement.Domain.Entities
{
    public class Approval
    {
        public Guid ApprovalId { get; set; }
        public int ApproverId { get; set; }
        public DateTime ProcessDateTime { get; set; }
        public string? Comments { get; set; }
    }
}
