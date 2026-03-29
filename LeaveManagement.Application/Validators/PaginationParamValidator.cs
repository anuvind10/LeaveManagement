using FluentValidation;
using LeaveManagement.Application.Common;

namespace LeaveManagement.Application.Validators
{
    public class PaginationParamValidator : AbstractValidator<PaginationParams>
    {
        public PaginationParamValidator()
        {
            RuleFor(paginationParams => paginationParams.Page)
                .GreaterThan(0)
                .WithMessage("Page should be a positive value.");
            RuleFor(paginationParams => paginationParams.PageSize)
                .GreaterThan(0)
                .WithMessage("Page size should be a positive value.");
        }
    }
}
