using FluentValidation;
using LeaveManagement.Application.Common;

namespace LeaveManagement.Application.Validators
{
    public class PaginationParamValidator : AbstractValidator<LeaveRequestPaginationParams>
    {
        public PaginationParamValidator()
        {
            RuleFor(pagination => pagination.Page)
                .GreaterThan(0)
                .WithMessage("Page should be a positive value.");
            RuleFor(pagination => pagination.PageSize)
                .GreaterThan(0)
                .LessThanOrEqualTo(100)
                .WithMessage("Page size should be between 1 and 100.");
        }
    }
}
