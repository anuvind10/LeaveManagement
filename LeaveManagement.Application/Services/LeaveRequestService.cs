using FluentValidation;
using LeaveManagement.Application.Common;
using LeaveManagement.Application.DTOs;
using LeaveManagement.Application.Exceptions;
using LeaveManagement.Application.Interfaces;
using LeaveManagement.Domain.Entities;
using LeaveManagement.Domain.Enums;

namespace LeaveManagement.Application.Services
{
    public class LeaveRequestService : ILeaveRequestService
    {
        private readonly ILeaveRequestRepository _repository;
        private readonly IValidator<CreateLeaveRequestDto> _creationValidator;
        private readonly IValidator<LeaveRequestPaginationParams> _paginationValidator;
        private readonly ILeaveRequestMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        public LeaveRequestService(ILeaveRequestRepository leaveRequestRepository, 
                                    IValidator<CreateLeaveRequestDto> creationValidator, 
                                    IValidator<LeaveRequestPaginationParams> paginationValidator,
                                    ILeaveRequestMapper mapper,
                                    ICurrentUserService currentUserService)
        {
            _repository = leaveRequestRepository;
            _creationValidator = creationValidator;
            _paginationValidator = paginationValidator;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<LeaveRequestDto> SubmitLeaveRequestAsync(CreateLeaveRequestDto dto, int employeeId) {
            await ValidateAsync(_creationValidator, dto);

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

        public async Task<(int, IEnumerable<LeaveRequestSummaryDto>)> GetAllAsync(LeaveRequestPaginationParams pagination,
            LeaveRequestSortParams sortParams,
            LeaveRequestFilterParams filterParams) 
        {
            await ValidateAsync(_paginationValidator, pagination);

            var result = await _repository.GetAllAsync(pagination.PageSize, 
                pagination.Page,
                sortParams,
                filterParams);

            var summaryDtos = _mapper.ToSummaryDtoList(result.Item2);

            return new (result.Item1, summaryDtos);
        }

        public async Task<(int, IEnumerable<LeaveRequestSummaryDto>)> GetByEmployeeIdAsync(int employeeId, 
            LeaveRequestPaginationParams pagination,
            LeaveRequestSortParams sortParams,
            LeaveRequestFilterParams filterParams)
        {
            await ValidateAsync(_paginationValidator, pagination);

            if (!_currentUserService.IsInRole("Manager") &&
                 !_currentUserService.IsInRole("HR") &&
                 employeeId != _currentUserService.UserId)
            {
                throw new ForbiddenAccessException("You do not have permission to access this resource.");
            }

            var result = await _repository.GetByEmployeeIdAsync(employeeId, 
                pagination.PageSize, 
                pagination.Page,
                sortParams,
                filterParams);

            var summaryDtos = _mapper.ToSummaryDtoList(result.Item2);

            return (result.Item1, summaryDtos);
        }

        private async Task ValidateAsync<T>(IValidator<T> validator, T instance)
        {
            var validationResult = await validator.ValidateAsync(instance);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );

                throw new Exceptions.ValidationException("One or more validation errors occurred.", errors);
            }
        }
    }
}
