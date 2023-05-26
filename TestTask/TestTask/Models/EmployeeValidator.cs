using FluentValidation;

namespace TestTask.Models
{
    public class EmployeeValidator : AbstractValidator<Employee>
    {
        public EmployeeValidator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(e => e.Name)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .MinimumLength(2).WithMessage("{PropertyName} length can`t be less than 2.")
                .MaximumLength(30).WithMessage("{PropertyName} length should not exceed 30 characters.");

            RuleFor(e => e.DateOfBirth)
                .NotEmpty().WithMessage("{PropertyName} is required.");

            RuleFor(e => e.Phone)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .Matches(@"^\d{10}$").WithMessage("Phone must be a 10-digit number.");

            RuleFor(e => e.Salary)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} must be a positive value.");
        }
    }
}
