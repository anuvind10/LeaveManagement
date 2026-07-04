using LeaveManagement.Application.Common;
using LeaveManagement.Application.DTOs;
using LeaveManagement.Domain.Enums;

namespace LeaveManagement.Application.Interfaces
{
    /// <summary>
    /// Service contract for handling leave request lifecycle operations.
    /// </summary>
    public interface ILeaveRequestService
    {
        /// <summary>
        /// Submits a new leave request for a specific employee.
        /// </summary>
        /// <param name="dto">The leave request creation payload.</param>
        /// <param name="employeeId">The ID of the employee submitting the request.</param>
        /// <returns>The created <see cref="LeaveRequestDto"/>.</returns>
        /// <exception cref="FluentValidation.ValidationException">Thrown if validation of creation details fails.</exception>
        Task<LeaveRequestDto> SubmitLeaveRequestAsync(CreateLeaveRequestDto dto, int employeeId);

        /// <summary>
        /// Retrieves a leave request by its unique identifier, validating authorization permissions.
        /// </summary>
        /// <param name="id">The unique identifier of the leave request.</param>
        /// <returns>The matching <see cref="LeaveRequestDto"/>, or null if not found or unauthorized.</returns>
        Task<LeaveRequestDto?> GetByIdAsync(Guid id);

        /// <summary>
        /// Approves a pending leave request.
        /// </summary>
        /// <param name="id">The unique identifier of the leave request.</param>
        /// <param name="auditorId">The ID of the manager performing the approval.</param>
        /// <param name="comments">Optional remarks explaining the approval.</param>
        /// <returns>The updated <see cref="LeaveRequestDto"/>, or null if the request was not found.</returns>
        /// <exception cref="LeaveManagement.Domain.Exceptions.InvalidLeaveStatusException">Thrown if the request status is not Pending.</exception>
        Task<LeaveRequestDto?> ApproveLeaveRequestAsync(Guid id, int auditorId, string? comments);

        /// <summary>
        /// Rejects a pending leave request.
        /// </summary>
        /// <param name="id">The unique identifier of the leave request.</param>
        /// <param name="auditorId">The ID of the manager performing the rejection.</param>
        /// <param name="comments">Required comments explaining the rejection reason.</param>
        /// <returns>The updated <see cref="LeaveRequestDto"/>, or null if the request was not found.</returns>
        /// <exception cref="LeaveManagement.Domain.Exceptions.InvalidLeaveStatusException">Thrown if the request status is not Pending.</exception>
        Task<LeaveRequestDto?> RejectLeaveRequestAsync(Guid id, int auditorId, string comments);

        /// <summary>
        /// Cancels a pending or approved leave request.
        /// </summary>
        /// <param name="id">The unique identifier of the leave request.</param>
        /// <param name="auditorId">The ID of the employee performing the cancellation.</param>
        /// <param name="comments">Optional comments regarding the cancellation.</param>
        /// <returns>The updated <see cref="LeaveRequestDto"/>, or null if the request was not found.</returns>
        /// <exception cref="LeaveManagement.Domain.Exceptions.InvalidLeaveStatusException">Thrown if the request status is neither Pending nor Approved.</exception>
        Task<LeaveRequestDto?> CancelLeaveRequestAsync(Guid id, int auditorId, string? comments);

        /// <summary>
        /// Retrieves a paginated, sorted, and filtered list of all leave requests (usually restricted to Managers/HR).
        /// </summary>
        /// <param name="pagination">Pagination parameters (page, page size).</param>
        /// <param name="sortParams">Sorting options.</param>
        /// <param name="filterParams">Filter criteria.</param>
        /// <returns>A tuple containing the total matching request count and the page's item collection.</returns>
        /// <exception cref="FluentValidation.ValidationException">Thrown if pagination parameters are invalid.</exception>
        Task<(int, IEnumerable<LeaveRequestSummaryDto>)> GetAllAsync(LeaveRequestPaginationParams pagination, 
            LeaveRequestSortParams sortParams, 
            LeaveRequestFilterParams filterParams);

        /// <summary>
        /// Retrieves a paginated, sorted, and filtered list of leave requests for a specific employee.
        /// </summary>
        /// <param name="employeeId">The unique ID of the employee.</param>
        /// <param name="pagination">Pagination parameters.</param>
        /// <param name="sortParams">Sorting options.</param>
        /// <param name="filterParams">Filter criteria.</param>
        /// <returns>A tuple containing the total request count for the employee and the page's item collection.</returns>
        /// <exception cref="FluentValidation.ValidationException">Thrown if pagination parameters are invalid.</exception>
        Task<(int, IEnumerable<LeaveRequestSummaryDto>)> GetByEmployeeIdAsync(int employeeId, 
            LeaveRequestPaginationParams pagination,
            LeaveRequestSortParams sortParams,
            LeaveRequestFilterParams filterParams);
    }
}
