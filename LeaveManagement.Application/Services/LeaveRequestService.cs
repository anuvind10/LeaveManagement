using FluentValidation;
using LeaveManagement.Application.DTOs;
using LeaveManagement.Application.Interfaces;
using LeaveManagement.Domain.Entities;
using LeaveManagement.Domain.Enums;

namespace LeaveManagement.Application.Services
{
    public class LeaveRequestService : ILeaveRequestService
    {
        private readonly ILeaveRequestRepository _repository;
        private readonly IValidator<CreateLeaveRequestDto> _validator;
        private readonly ILeaveRequestMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        public LeaveRequestService(ILeaveRequestRepository leaveRequestRepository, 
                                    IValidator<CreateLeaveRequestDto> validator, 
                                    ILeaveRequestMapper mapper,
                                    ICurrentUserService currentUserService)
        {
            _repository = leaveRequestRepository;
            _validator = validator;
            _mapper = mapper;
            _currentUserService = currentUserService;
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
                Reason = dto.Reason,
                LeaveStatus = LeaveStatus.Pending,
                LeaveAudits = new List<LeaveAudit>()
            };

            await _repository.CreateAsync(leaveRequest);

            return _mapper.ToDto(leaveRequest);
        }

        public async Task<LeaveRequestDto?> GetByIdAsync(Guid id)
        {
            var leaveRequest = await _repository.GetByIdAsync(id);

            if (leaveRequest == null ||
                (!_currentUserService.IsInRole("Manager") && 
                 !_currentUserService.IsInRole("HR") &&
                 leaveRequest.EmployeeId != _currentUserService.UserId)
                )
            {
                return null;
            }

            return _mapper.ToDto(leaveRequest);
        }

        public async Task<LeaveRequestDto?> ApproveLeaveRequestAsync(Guid id, int auditorId, string? comments) {
            var leaveRequest = await _repository.GetByIdAsync(id);

            if (leaveRequest == null)
            {
                return null;
            }
            
            leaveRequest.Approve(auditorId, comments);
            await _repository.UpdateAsync(leaveRequest);

            return _mapper.ToDto(leaveRequest);
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

            return _mapper.ToDto(leaveRequest);
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

            return _mapper.ToDto(leaveRequest);
        }

        public async Task<IEnumerable<LeaveRequestSummaryDto>> GetAllAsync() {
            var leaveRequests = await _repository.GetAllAsync();

            return _mapper.ToSummaryDtoList(leaveRequests);
        }

        public async Task<IEnumerable<LeaveRequestSummaryDto>> GetByStatusAsync(LeaveStatus status)
        {
            var leaveRequests = await _repository.GetByStatusAsync(status);

            return _mapper.ToSummaryDtoList(leaveRequests);
        }

        public async Task<IEnumerable<LeaveRequestSummaryDto>?> GetByEmployeeIdAsync(int employeeId)
        {
            if (!_currentUserService.IsInRole("Manager") &&
                 !_currentUserService.IsInRole("HR") &&
                 employeeId != _currentUserService.UserId)
            {
                return null;
            }

            var leaveRequests = await _repository.GetByEmployeeIdAsync(employeeId);
            return _mapper.ToSummaryDtoList(leaveRequests);
        }
    }
}
