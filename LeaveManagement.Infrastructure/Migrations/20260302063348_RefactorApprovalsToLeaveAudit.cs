using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeaveManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorApprovalsToLeaveAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "Approvals",
                newName: "LeaveAudits");

            migrationBuilder.RenameColumn(
                name: "ApproverId",
                table: "Approvals",
                newName: "AuditorId");

            migrationBuilder.RenameColumn(
                name: "ApprovalId",
                table: "Approvals",
                newName: "AuditId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AuditorId",
                table: "Approvals",
                newName: "ApproverId");

            migrationBuilder.RenameColumn(
                name: "AuditId",
                table: "Approvals",
                newName: "ApprovalId");

            migrationBuilder.RenameTable(
                name: "LeaveAudits",
                newName: "Approvals");
        }
    }
}
