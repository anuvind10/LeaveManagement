using AutoMapper;
using FluentValidation;
using LeaveManagement.Application.DTOs;
using LeaveManagement.Application.Interfaces;
using LeaveManagement.Application.Mappings;
using LeaveManagement.Application.Services;
using LeaveManagement.Domain.Entities;
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
        
    }
}
