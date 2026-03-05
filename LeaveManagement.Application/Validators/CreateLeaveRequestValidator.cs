using FluentValidation;
using LeaveManagement.Application.DTOs;

namespace LeaveManagement.Application.Validators
{
    public class CreateLeaveRequestValidator : AbstractValidator<CreateLeaveRequestDto>
    {
        public CreateLeaveRequestValidator()
        {
            RuleFor(dto => dto.StartDate)
                .GreaterThan(_ => DateTime.UtcNow)
                .WithMessage("Start date must be in the future.");
            RuleFor(dto => dto.EndDate)
                .GreaterThanOrEqualTo(dto => dto.StartDate)
                .WithMessage("End date must be on or after the start date.");
            RuleFor(dto => dto.LeaveType)
                .IsInEnum()
                .WithMessage("Invalid leave type.");
        }
    }
}
