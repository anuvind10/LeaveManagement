using AutoMapper;
using FluentValidation;
using LeaveManagement.Application.DTOs;
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
        private readonly IMapper _mapper;
        private readonly Mock<ILeaveRequestRepository> _mockRepo;
        private readonly Mock<IValidator<CreateLeaveRequestDto>> _mockValidator;
        private readonly Mock<ICurrentUserService> _mockCurrentUser;
        private readonly LeaveRequestService _service;
        public LeaveRequestServiceTest()
        {
            _mockRepo = new Mock<ILeaveRequestRepository>();
            _mockValidator = new Mock<IValidator<CreateLeaveRequestDto>>();
            _mockCurrentUser = new Mock<ICurrentUserService>();
            _mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddMaps(typeof(LeaveRequestProfile).Assembly);
            }, Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance).CreateMapper();

            _service = new LeaveRequestService(_mockRepo.Object,
                                                  _mockValidator.Object,
                                                  _mapper,
                                                  _mockCurrentUser.Object);
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
        public async Task SubmitLeaveRequestAsync_WhenValidationFails_ShouldThrowValidationException()
        {
            var dto = CreateLeaveRequestDto();

            var validationResult = new FluentValidation.Results.ValidationResult(
                new List<FluentValidation.Results.ValidationFailure>
                {
                    new FluentValidation.Results.ValidationFailure("StartDate", "Start date must be in the future.")
                });

            _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<CreateLeaveRequestDto>(), default))
                .ReturnsAsync(validationResult);

            await Assert.ThrowsAsync<LeaveManagement.Application.Exceptions.ValidationException>
                (() =>  _service.SubmitLeaveRequestAsync(dto, 1));
        }

        [Fact]
        public async Task SubmitLeaveRequestAsync_WhenValidationPasses_ShouldCallCreateAsync()
        {
            var dto = CreateLeaveRequestDto();

            _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<CreateLeaveRequestDto>()))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());
            
            var result = await _service.SubmitLeaveRequestAsync(dto, 1);
            
            _mockRepo.Verify(r => r.CreateAsync(It.IsAny<LeaveRequest>()), Times.Once);
        }

        [Fact]
        public async Task SubmitLeaveRequestAsync_WhenValidationPasses_ShouldReturnDto()
        {
            var dto = CreateLeaveRequestDto();

            _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<CreateLeaveRequestDto>()))
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
            _mockRepo.Setup(r => r.GetByEmployeeIdAsync(leaveRequest.EmployeeId))
                    .ReturnsAsync(leaveRequests);
            _mockCurrentUser.Setup(cu => cu.UserId).Returns(1);
            _mockCurrentUser.Setup(cu => cu.IsInRole(It.IsAny<string>())).Returns(false);

            var result = await _service.GetByEmployeeIdAsync(1);

            Assert.Single(result);

            var request = result.Single();

            Assert.Equal(leaveRequest.Id, request.Id);
            Assert.Equal(LeaveType.Sick, request.LeaveType);
            Assert.Equal(LeaveStatus.Pending, request.LeaveStatus);
        }

        [Fact]
        public async Task GetByEmployeeIdAsync_WhenEmployeeRequestsAnotherEmployeesRecords_ShouldReturnNull()
        {
            _mockCurrentUser.Setup(cu => cu.UserId).Returns(1);
            _mockCurrentUser.Setup(cu => cu.IsInRole(It.IsAny<string>())).Returns(false);

            var result = await _service.GetByEmployeeIdAsync(2);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByEmployeeIdAsync_WhenManagerRequestsAnyEmployeesRecords_ShouldReturnDtos()
        {
            var leaveRequest = CreateLeaveRequest(1);
            var leaveRequests = new List<LeaveRequest> { leaveRequest };
            _mockRepo.Setup(r => r.GetByEmployeeIdAsync(leaveRequest.EmployeeId))
                    .ReturnsAsync(leaveRequests);

            _mockCurrentUser.Setup(cu => cu.UserId).Returns(2);
            _mockCurrentUser.Setup(cu => cu.IsInRole(It.IsAny<string>())).Returns(true);

            var result = await _service.GetByEmployeeIdAsync(1);

            Assert.Single(result);

            var request = result.Single();

            Assert.Equal(leaveRequest.Id, request.Id);
            Assert.Equal(1, leaveRequest.EmployeeId);
            Assert.Equal(LeaveType.Sick, request.LeaveType);
            Assert.Equal(LeaveStatus.Pending, request.LeaveStatus);
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
