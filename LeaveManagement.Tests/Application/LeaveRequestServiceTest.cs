using FluentValidation;
using LeaveManagement.Application.Common;
using LeaveManagement.Application.DTOs;
using LeaveManagement.Application.Exceptions;
using LeaveManagement.Application.Interfaces;
using LeaveManagement.Application.Mappings;
using LeaveManagement.Application.Services;
using LeaveManagement.Domain.Entities;
using LeaveManagement.Domain.Enums;
using Moq;

namespace LeaveManagement.Tests.Application
{
    public class LeaveRequestServiceTest
    {
        private readonly Mock<ILeaveRequestRepository> _mockRepo;
        private readonly Mock<IValidator<CreateLeaveRequestDto>> _mockCreationValidator;
        private readonly Mock<IValidator<LeaveRequestPaginationParams>> _mockPaginationValidator;
        private readonly Mock<ICurrentUserService> _mockCurrentUser;
        private readonly LeaveRequestService _service;
        private readonly LeaveRequestPaginationParams _paginationParams;
        private readonly LeaveRequestSortParams _sortParams;
        private readonly LeaveRequestFilterParams _filterParams;
        private readonly Mock<IUserRepository> _mockUserRepo;
        public LeaveRequestServiceTest()
        {
            _mockRepo = new Mock<ILeaveRequestRepository>();
            _mockCreationValidator = new Mock<IValidator<CreateLeaveRequestDto>>();
            _mockPaginationValidator = new Mock<IValidator<LeaveRequestPaginationParams>>();
            _mockCurrentUser = new Mock<ICurrentUserService>();
            _mockUserRepo = new Mock<IUserRepository>();
            
            _service = new LeaveRequestService(_mockRepo.Object,
                                               _mockCreationValidator.Object,
                                               _mockPaginationValidator.Object,
                                               new LeaveRequestMapper(),
                                               _mockCurrentUser.Object,
                                               _mockUserRepo.Object);

            _paginationParams = new LeaveRequestPaginationParams();
            _sortParams = new LeaveRequestSortParams();
            _filterParams = new LeaveRequestFilterParams();
        }

        [Fact]
        public async Task GetByIdAsync_WhenRequestDoesNotExist_ShouldReturnNull()
        {
            _mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                    .ReturnsAsync((LeaveRequest?)null);
            _mockCurrentUser.Setup(cu => cu.UserId).Returns(1);
            _mockCurrentUser.Setup(cu => cu.IsInRole(It.IsAny<string>())).Returns(true);

            var result = await _service.GetByIdAsync(Guid.NewGuid());
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_WhenRequestExistsAndBelongsToCurrentUser_ShouldReturnDto()
        {
            var leaveRequest = CreateLeaveRequest(1);
            _mockRepo.Setup(r => r.GetByIdAsync(leaveRequest.Id))
                    .ReturnsAsync(leaveRequest);
            _mockCurrentUser.Setup(cu => cu.UserId).Returns(1);
            _mockCurrentUser.Setup(cu => cu.IsInRole(It.IsAny<string>())).Returns(true);

            var result = await _service.GetByIdAsync(leaveRequest.Id);
            Assert.NotNull(result);
            Assert.Equal(leaveRequest.Id, result.Id);
            Assert.Equal(LeaveStatus.Pending, result.LeaveStatus);
        }

        [Fact]
        public async Task GetByIdAsync_WhenRequestExistsBelongsToAnotherEmployee_ShouldReturnNull()
        {

            var leaveRequest = CreateLeaveRequest(2);
            _mockRepo.Setup(r => r.GetByIdAsync(leaveRequest.Id))
                   .ReturnsAsync(leaveRequest);
            _mockCurrentUser.Setup(cu => cu.UserId).Returns(1);
            _mockCurrentUser.Setup(cu => cu.IsInRole(It.IsAny<string>())).Returns(false);

            var result = await _service.GetByIdAsync(leaveRequest.Id);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_WhenCurrentUserIsManagerOfEmployee_ShouldReturnDto()
        {

            var leaveRequest = CreateLeaveRequest(2);
            _mockRepo.Setup(r => r.GetByIdAsync(leaveRequest.Id))
                   .ReturnsAsync(leaveRequest);
            _mockCurrentUser.Setup(cu => cu.UserId).Returns(5); //Managers ID
            _mockCurrentUser.Setup(cu => cu.IsInRole(It.IsAny<string>())).Returns(false); //Not HR
            _mockUserRepo.Setup(u => u.IsManagerAsync(leaveRequest.EmployeeId, 5)).ReturnsAsync(true);

            var result = await _service.GetByIdAsync(leaveRequest.Id);
            Assert.NotNull(result);
            Assert.Equal(leaveRequest.Id, result.Id);
        }

        [Fact]
        public async Task GetByIdAsync_WhenCurrentUserIsNotManagerOfEmployee_ShouldReturnNull()
        {

            var leaveRequest = CreateLeaveRequest(2);
            _mockRepo.Setup(r => r.GetByIdAsync(leaveRequest.Id))
                   .ReturnsAsync(leaveRequest);
            _mockCurrentUser.Setup(cu => cu.UserId).Returns(6); //Non Manager ID
            _mockCurrentUser.Setup(cu => cu.IsInRole(It.IsAny<string>())).Returns(false); //Not HR
            _mockUserRepo.Setup(u => u.IsManagerAsync(leaveRequest.EmployeeId, 6)).ReturnsAsync(false);

            var result = await _service.GetByIdAsync(leaveRequest.Id);
            Assert.Null(result);
        }

        [Fact]
        public async Task SubmitLeaveRequestAsync_WhenValidationFails_ShouldThrowValidationException()
        {
            var dto = CreateLeaveRequestDto();

            var validationResult = new FluentValidation.Results.ValidationResult(
                new List<FluentValidation.Results.ValidationFailure>
                {
                    new FluentValidation.Results.ValidationFailure("StartDate", "Start date must be in the future.")
                });

            _mockCreationValidator.Setup(v => v.ValidateAsync(It.IsAny<CreateLeaveRequestDto>(), default))
                .ReturnsAsync(validationResult);

            await Assert.ThrowsAsync<LeaveManagement.Application.Exceptions.ValidationException>
                (() =>  _service.SubmitLeaveRequestAsync(dto, 1));
        }

        [Fact]
        public async Task SubmitLeaveRequestAsync_WhenValidationPasses_ShouldCallCreateAsync()
        {
            var dto = CreateLeaveRequestDto();

            _mockCreationValidator.Setup(v => v.ValidateAsync(It.IsAny<CreateLeaveRequestDto>()))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());
            
            var result = await _service.SubmitLeaveRequestAsync(dto, 1);
            
            _mockRepo.Verify(r => r.CreateAsync(It.IsAny<LeaveRequest>()), Times.Once);
        }

        [Fact]
        public async Task SubmitLeaveRequestAsync_WhenValidationPasses_ShouldReturnDto()
        {
            var dto = CreateLeaveRequestDto();

            _mockCreationValidator.Setup(v => v.ValidateAsync(It.IsAny<CreateLeaveRequestDto>()))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            var result = await _service.SubmitLeaveRequestAsync(dto, 1);

            Assert.Equal(LeaveType.Sick, result.LeaveType);
            Assert.Equal(LeaveStatus.Pending, result.LeaveStatus);
        }

        [Fact]
        public async Task ApproveLeaveRequestAsync_WhenRequestDoesNotExist_ShouldReturnNull()
        {
            _mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                   .ReturnsAsync((LeaveRequest?)null);

            var result = await _service.ApproveLeaveRequestAsync(new Guid(), 2, "");
            Assert.Null(result);
        }

        [Fact]
        public async Task ApproveLeaveRequestAsync_WhenRequestExists_ShouldCallUpdateAsync()
        {
            var leaveRequest = CreateLeaveRequest(1);
            _mockRepo.Setup(r => r.GetByIdAsync(leaveRequest.Id))
                    .ReturnsAsync(leaveRequest);

            var result = await _service.ApproveLeaveRequestAsync(leaveRequest.Id, 2, "");

            _mockRepo.Verify(r => r.UpdateAsync(leaveRequest), Times.Once);
        }

        [Fact]
        public async Task RejectLeaveRequestAsync_WhenRequestDoesNotExist_ShouldReturnNull()
        {
            _mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                    .ReturnsAsync((LeaveRequest?)null);

            var result = await _service.RejectLeaveRequestAsync(Guid.NewGuid(), 2, "");
            Assert.Null(result);
        }

        [Fact]
        public async Task RejectLeaveRequestAsync_WhenRequestExists_ShouldCallUpdateAsync()
        {
            var leaveRequest = CreateLeaveRequest(1);
            _mockRepo.Setup(r => r.GetByIdAsync(leaveRequest.Id))
                    .ReturnsAsync(leaveRequest);

            var result = await _service.RejectLeaveRequestAsync(leaveRequest.Id, 2, "");

            _mockRepo.Verify(r => r.UpdateAsync(leaveRequest), Times.Once);
        }

        [Fact]
        public async Task CancelLeaveRequestAsync_WhenRequestDoesNotExist_ShouldReturnNull()
        {
            _mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                    .ReturnsAsync((LeaveRequest?)null);

            var result = await _service.CancelLeaveRequestAsync(Guid.NewGuid(), 1, "");

            Assert.Null(result);
        }

        [Fact]
        public async Task CancelLeaveRequestAsync_WhenRequestExists_ShouldCallUpdateAsync()
        {
            var leaveRequest = CreateLeaveRequest(1);
            _mockRepo.Setup(r => r.GetByIdAsync(leaveRequest.Id))
                    .ReturnsAsync(leaveRequest);

            var result = await _service.CancelLeaveRequestAsync(leaveRequest.Id, 1, "");

            _mockRepo.Verify(r => r.UpdateAsync(leaveRequest), Times.Once);
        }

        [Fact]
        public async Task GetByEmployeeIdAsync_WhenEmployeeRequestsOwnRecords_ShouldReturnDtos()
        {
            var leaveRequest = CreateLeaveRequest(1);
            var leaveRequests = new List<LeaveRequest> { leaveRequest };

            _mockRepo.Setup(r => r.GetByEmployeeIdAsync(leaveRequest.EmployeeId, It.IsAny<LeaveRequestPaginationParams>(), It.IsAny<LeaveRequestSortParams>(), It.IsAny<LeaveRequestFilterParams>()))
                    .ReturnsAsync((1, leaveRequests));
            _mockCurrentUser.Setup(cu => cu.UserId).Returns(1);
            _mockCurrentUser.Setup(cu => cu.IsInRole(It.IsAny<string>())).Returns(false);
            _mockPaginationValidator.Setup(v => v.ValidateAsync(It.IsAny<LeaveRequestPaginationParams>(), default))
               .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            var result = await _service.GetByEmployeeIdAsync(1, _paginationParams, _sortParams, _filterParams);
            Assert.Single(result.Item2);

            var request = result.Item2.Single();

            Assert.Equal(leaveRequest.Id, request.Id);
            Assert.Equal(LeaveType.Sick, request.LeaveType);
            Assert.Equal(LeaveStatus.Pending, request.LeaveStatus);
        }

        [Fact]
        public async Task GetByEmployeeIdAsync_WhenEmployeeRequestsAnotherEmployeesRecords_ShouldThrowForbiddenAccessException()
        {
            _mockCurrentUser.Setup(cu => cu.UserId).Returns(1);
            _mockCurrentUser.Setup(cu => cu.IsInRole(It.IsAny<string>())).Returns(false);
            _mockPaginationValidator.Setup(v => v.ValidateAsync(It.IsAny<LeaveRequestPaginationParams>(), default))
               .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            await Assert.ThrowsAsync<ForbiddenAccessException>
                (() => _service.GetByEmployeeIdAsync(2, _paginationParams, _sortParams, _filterParams));
        }

        [Fact]
        public async Task GetByEmployeeIdAsync_WhenManagerRequestsAnyEmployeesRecords_ShouldReturnDtos()
        {
            var leaveRequest = CreateLeaveRequest(1);
            var leaveRequests = new List<LeaveRequest> { leaveRequest };
            _mockRepo.Setup(r => r.GetByEmployeeIdAsync(leaveRequest.EmployeeId, It.IsAny<LeaveRequestPaginationParams>(), It.IsAny<LeaveRequestSortParams>(), It.IsAny<LeaveRequestFilterParams>()))
                    .ReturnsAsync((1, leaveRequests));
            _mockCurrentUser.Setup(cu => cu.UserId).Returns(2);
            _mockCurrentUser.Setup(cu => cu.IsInRole(It.IsAny<string>())).Returns(true);
            _mockPaginationValidator.Setup(v => v.ValidateAsync(It.IsAny<LeaveRequestPaginationParams>(), default))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            var result = await _service.GetByEmployeeIdAsync(1, _paginationParams, _sortParams, _filterParams);

            Assert.Single(result.Item2);

            var request = result.Item2.Single();

            Assert.Equal(leaveRequest.Id, request.Id);
            Assert.Equal(1, leaveRequest.EmployeeId);
            Assert.Equal(LeaveType.Sick, request.LeaveType);
            Assert.Equal(LeaveStatus.Pending, request.LeaveStatus);
        }

        [Fact]
        public async Task GetByEmployeeIdAsync_WhenValidationFails_ShouldThrowValidationException()
        {
            var invalidPaginationParams = new LeaveRequestPaginationParams { Page = 1, PageSize = -10 };
            var validationResult = new FluentValidation.Results.ValidationResult(
                new List<FluentValidation.Results.ValidationFailure>
                {
                    new FluentValidation.Results.ValidationFailure("PageSize", "Page size should be between 1 and 100.")
                });

            _mockPaginationValidator.Setup(v => v.ValidateAsync(It.IsAny<LeaveRequestPaginationParams>(), default))
                .ReturnsAsync(validationResult);

            await Assert.ThrowsAsync<LeaveManagement.Application.Exceptions.ValidationException>
                (() => _service.GetByEmployeeIdAsync(1, invalidPaginationParams, _sortParams, _filterParams));
        }

        [Fact]
        public async Task GetAllAsync_WhenValidationFails_ShouldThrowValidationException()
        {
            var invalidPaginationParams = new LeaveRequestPaginationParams { Page = -3, PageSize = 10 };
            var validationResult = new FluentValidation.Results.ValidationResult(
                new List<FluentValidation.Results.ValidationFailure>
                {
                    new FluentValidation.Results.ValidationFailure("Page", "Page should be a positive value.")
                });

            _mockPaginationValidator.Setup(v => v.ValidateAsync(It.IsAny<LeaveRequestPaginationParams>(), default))
                .ReturnsAsync(validationResult);

            await Assert.ThrowsAsync<LeaveManagement.Application.Exceptions.ValidationException>
                (() => _service.GetAllAsync(invalidPaginationParams, _sortParams, _filterParams));
        }

        [Fact]
        public async Task GetAllAsync_WhenUserisHR_ShouldReturnAllLeaveRequest()
        {
            var leaveRequest = new List<LeaveRequest> { CreateLeaveRequest(1), CreateLeaveRequest(2), CreateLeaveRequest(3) };

            _mockCurrentUser.Setup(cu => cu.UserId).Returns(1);
            _mockCurrentUser.Setup(cu => cu.IsInRole(It.IsAny<string>())).Returns(true); //HR

            _mockPaginationValidator.Setup(v => v.ValidateAsync(It.IsAny<LeaveRequestPaginationParams>()))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            _mockRepo.Setup(r => r.GetAllAsync(
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<LeaveRequestPaginationParams>(),
                It.IsAny<LeaveRequestSortParams>(),
                It.IsAny<LeaveRequestFilterParams>()))
                .ReturnsAsync((3, leaveRequest));

            var result = await _service.GetAllAsync(new LeaveRequestPaginationParams(), new LeaveRequestSortParams(), new LeaveRequestFilterParams());
            Assert.Equal(3, result.Item1);
        }

        [Fact]
        public async Task GetAllAsync_WhenUserisManager_ShouldReturnRepositoryResult()
        {
            var leaveRequest = new List<LeaveRequest> { CreateLeaveRequest(1), CreateLeaveRequest(11) };
            var reportees = new List<ApplicationUser> { new() { Id = 3 }, new() { Id = 5 } };

            _mockCurrentUser.Setup(cu => cu.UserId).Returns(2);
            _mockCurrentUser.Setup(cu => cu.IsInRole(It.IsAny<string>())).Returns(false); //Not HR

            _mockUserRepo.Setup(u => u.GetReporteesAsync(2)).ReturnsAsync(reportees.Select(r => r.Id));

            _mockPaginationValidator.Setup(v => v.ValidateAsync(It.IsAny<LeaveRequestPaginationParams>()))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult()); 

            _mockRepo.Setup(r => r.GetAllAsync(
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<LeaveRequestPaginationParams>(),
                It.IsAny<LeaveRequestSortParams>(),
                It.IsAny<LeaveRequestFilterParams>()))
                .ReturnsAsync((2, leaveRequest));

            var result = await _service.GetAllAsync(new LeaveRequestPaginationParams(), new LeaveRequestSortParams(), new LeaveRequestFilterParams());
            Assert.Equal(2, result.Item1);
        }

        private static LeaveRequest CreateLeaveRequest(int employeeId)
        {
            return new LeaveRequest()
            {
                Id = Guid.NewGuid(),
                SubmittedDate = DateTime.UtcNow,
                EmployeeId = employeeId,
                LeaveType = LeaveType.Sick,
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(2),
            };        
        }

        private static CreateLeaveRequestDto CreateLeaveRequestDto()
        {
            return new CreateLeaveRequestDto()
            {
                LeaveType = LeaveType.Sick,
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(2),
            };
        }
    }
}
