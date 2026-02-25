using LeaveManagement.Application.DTOs;
using LeaveManagement.Application.Interfaces;
using LeaveManagement.Domain.Entities;
using LeaveManagement.Domain.Enums;

namespace LeaveManagement.Application.Services
{
    public class LeaveRequestService
    {
        private readonly ILeaveRequestRepository _repository;
        public LeaveRequestService(ILeaveRequestRepository leaveRequestRepository)
        {
            _repository = leaveRequestRepository;
        }

        public async Task<LeaveRequestDto> SubmitLeaveRequestAsync(CreateLeaveRequestDto dto, int employeeId) {
            var leaveRequest = new LeaveRequest()
            {
                Id = Guid.NewGuid(),
                SubmittedDate = DateTime.UtcNow,
                EmployeeId = employeeId,
                LeaveType = dto.LeaveType,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                NoOfDays = CalculateNoOfDays(dto.StartDate, dto.EndDate),
                Reason = dto.Reason,
                LeaveStatus = LeaveStatus.Pending,
                Approvals = new List<Approval>()
            };

            ValidateInputs(leaveRequest);

            await _repository.CreateAsync(leaveRequest);

            var leaveRequestDto = new LeaveRequestDto()
            {
                Id = leaveRequest.Id,
                SubmittedDate = leaveRequest.SubmittedDate,
                LeaveType = leaveRequest.LeaveType,
                StartDate = leaveRequest.StartDate,
                EndDate = leaveRequest.EndDate,
                NoOfDays = leaveRequest.NoOfDays,
                Reason = leaveRequest.Reason,
                LeaveStatus = leaveRequest.LeaveStatus,
                Approvals = leaveRequest.Approvals.Select(approval => new ApprovalDto 
                { 
                    ApprovalId = approval.ApprovalId,
                    ApproverId = approval.ApproverId,
                    ProcessDateTime = approval.ProcessDateTime,
                    Comments = approval.Comments
                }).ToList()
            };

            return leaveRequestDto;
        }

        private decimal CalculateNoOfDays(DateTime startDate, DateTime endDate) 
        {
            var noOfDays = endDate - startDate;
            return (decimal)(noOfDays.TotalDays + 1);
        }

        //Naming the method ValidateInputs in case we need to validate things other than start and end date
        private void ValidateInputs(LeaveRequest leaveRequest) {
            if (leaveRequest.EndDate < leaveRequest.StartDate) {
                throw new ArgumentException("End Date cannot be before start date.");
            }
        }
    }
}
