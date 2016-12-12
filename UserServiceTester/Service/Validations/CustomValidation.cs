using FluentValidation;

namespace Service.Validations
{
    public class CustomValidation : AbstractValidator<User>
    {
        public CustomValidation()
        {
            RuleFor(user => user.FirstName.Length).LessThan(30);
            RuleFor(user => user.LastName.Length).LessThan(30);
            RuleFor(user => user.FirstName).NotEmpty();
            RuleFor(user => user.LastName).NotEmpty();
        }
    }
}
