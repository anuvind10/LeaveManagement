using FluentValidation;
using LeaveManagement.Application.DTOs;
using LeaveManagement.Application.Validators;
using LeaveManagement.Domain.Enums;

namespace LeaveManagement.Tests.Application
{
    public class CreateLeaveRequestValidatorTests
    {
        private readonly IValidator<CreateLeaveRequestDto> _validator;
        public CreateLeaveRequestValidatorTests()
        {
            _validator = new CreateLeaveRequestValidator();
        }

        [Fact]
        public void Validate_WhenAllFieldsAreValid_ShouldPassValidation()
        {
            var dto = new CreateLeaveRequestDto
            {
                LeaveType = LeaveType.Sick,
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(2)
            };

            var result = _validator.Validate(dto);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_WhenStartDateIsInThePast_ShouldFailValidation()
        {
            var dto = new CreateLeaveRequestDto
            {
                LeaveType = LeaveType.Unpaid,
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(1)
            };

            var errors = GetValidatorErrors(dto);

            Assert.Single(errors["StartDate"]);
            Assert.Equal("Start date must be in the future.", errors["StartDate"].Single());
        }

        [Fact]
        public void Validate_WhenEndDateIsBeforeStartDate_ShouldFailValidation()
        {
            var dto = new CreateLeaveRequestDto
            {
                LeaveType = LeaveType.Paternity,
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(-1)
            };

            var errors = GetValidatorErrors(dto);

            Assert.Single(errors["EndDate"]);
            Assert.Equal("End date must be on or after the start date.", errors["EndDate"].Single());
        }

        [Fact]
        public void Validate_WhenLeaveTypeIsInvalid_ShouldFailValidation()
        {
            var dto = new CreateLeaveRequestDto
            {
                LeaveType = (LeaveType)999,
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(2)
            };

            var errors = GetValidatorErrors(dto);

            Assert.Single(errors["LeaveType"]);
            Assert.Equal("Invalid leave type.", errors["LeaveType"].Single());
        }

        private Dictionary<string, string[]> GetValidatorErrors(CreateLeaveRequestDto dto)
        {
            var result = _validator.Validate(dto);
            return result.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );
        }
    }
}
