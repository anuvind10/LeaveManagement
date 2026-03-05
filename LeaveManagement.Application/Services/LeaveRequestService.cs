using FluentValidation;
using LeaveManagement.Application.DTOs;
using LeaveManagement.Application.Interfaces;
using LeaveManagement.Domain.Entities;
using LeaveManagement.Domain.Enums;

namespace LeaveManagement.Application.Services
{
    public class LeaveRequestService
    {
        private readonly ILeaveRequestRepository _repository;
        private readonly IValidator<CreateLeaveRequestDto> _validator;
        public LeaveRequestService(ILeaveRequestRepository leaveRequestRepository, IValidator<CreateLeaveRequestDto> validator)
        {
            _repository = leaveRequestRepository;
            _validator = validator;
        }

        public async Task<LeaveRequestDto> SubmitLeaveRequestAsync(CreateLeaveRequestDto dto, int employeeId) {
            var validationResult = await _validator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );

                throw new Exceptions.ValidationException("One or more validation errors occurred." ,errors);
            }

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
                LeaveAudits = new List<LeaveAudit>()
            };

            await _repository.CreateAsync(leaveRequest);

            return MapToLeaveRequestDto(leaveRequest);
        }

        public async Task<LeaveRequestDto?> GetByIdAsync(Guid id)
        {
            var leaveRequest = await _repository.GetByIdAsync(id);

            if (leaveRequest == null)
            {
                return null;
            }

            return MapToLeaveRequestDto(leaveRequest); ;
        }

        public async Task<LeaveRequestDto?> ApproveLeaveRequestAsync(Guid id, int auditorId, string? comments) {
            var leaveRequest = await _repository.GetByIdAsync(id);

            if (leaveRequest == null)
            {
                return null;
            }
            
            leaveRequest.Approve(auditorId, comments);
            await _repository.UpdateAsync(leaveRequest);

            return MapToLeaveRequestDto(leaveRequest);
        }

        public async Task<LeaveRequestDto?> RejectLeaveRequestAsync(Guid id, int auditorId, string comments)
        {
            var leaveRequest = await _repository.GetByIdAsync(id);

            if (leaveRequest == null)
            {
                return null;
            }

            leaveRequest.Reject(auditorId, comments);
            await _repository.UpdateAsync(leaveRequest);

            return MapToLeaveRequestDto(leaveRequest);
        }

        public async Task<LeaveRequestDto?> CancelLeaveRequestAsync(Guid id, int auditorId, string? comments)
        {
            var leaveRequest = await _repository.GetByIdAsync(id);

            if (leaveRequest == null)
            {
                return null;
            }

            leaveRequest.Cancel(auditorId, comments);
            await _repository.UpdateAsync(leaveRequest);

            return MapToLeaveRequestDto(leaveRequest);
        }

        public async Task<IEnumerable<LeaveRequestSummaryDto>> GetAllAsync() {
            var leaveRequests = await _repository.GetAllAsync();

            return MapToLeaveRequestSummaryDto(leaveRequests);
        }

        public async Task<IEnumerable<LeaveRequestSummaryDto>> GetByStatusAsync(LeaveStatus status)
        {
            var leaveRequests = await _repository.GetByStatusAsync(status);

            return MapToLeaveRequestSummaryDto(leaveRequests);
        }

        public async Task<IEnumerable<LeaveRequestSummaryDto>> GetByEmployeeIdAsync(int employeeId)
        {
            var leaveRequests = await _repository.GetByEmployeeIdAsync(employeeId);

            return MapToLeaveRequestSummaryDto(leaveRequests);
        }

        private decimal CalculateNoOfDays(DateTime startDate, DateTime endDate) 
        {
            var noOfDays = endDate - startDate;
            return (decimal)(noOfDays.TotalDays + 1);
        }

        private LeaveRequestDto MapToLeaveRequestDto(LeaveRequest leaveRequest) {
            var dto = new LeaveRequestDto()
            {
                Id = leaveRequest.Id,
                SubmittedDate = leaveRequest.SubmittedDate,
                LeaveType = leaveRequest.LeaveType,
                StartDate = leaveRequest.StartDate,
                EndDate = leaveRequest.EndDate,
                NoOfDays = leaveRequest.NoOfDays,
                Reason = leaveRequest.Reason,
                LeaveStatus = leaveRequest.LeaveStatus,
                LeaveAudits = leaveRequest.LeaveAudits.Select(audit => new LeaveAuditDto
                {
                    AuditId = audit.AuditId,
                    AuditorId = audit.AuditorId,
                    ProcessDateTime = audit.ProcessDateTime,
                    Comments = audit.Comments,
                    Action = audit.Action,
                }).ToList()
            };

            return dto;
        }

        private IEnumerable<LeaveRequestSummaryDto> MapToLeaveRequestSummaryDto(IEnumerable<LeaveRequest> leaveRequests)
        {
            var leaveRequestSummaryDtos = leaveRequests.Select(lr => new LeaveRequestSummaryDto
            {
                Id = lr.Id,
                LeaveType = lr.LeaveType,
                NoOfDays = lr.NoOfDays,
                Reason = lr.Reason,
                LeaveStatus = lr.LeaveStatus
            });
            
            return leaveRequestSummaryDtos;
        }
    }
}
