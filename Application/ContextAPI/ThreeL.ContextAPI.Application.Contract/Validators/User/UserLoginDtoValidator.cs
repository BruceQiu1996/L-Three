using FluentValidation;
using ThreeL.ContextAPI.Application.Contract.Dtos.User;
using ThreeL.Shared.Application.Contract.Validators;

namespace ThreeL.ContextAPI.Application.Contract.Validators.User
{
    public class UserLoginDtoValidator : AbstractValidator<UserLoginDto>, IMyValidator
    {
        public UserLoginDtoValidator()
        {
            RuleFor(x => x.UserName).NotNull().NotEmpty().MinimumLength(2)
                .MaximumLength(16).WithName("用户名");
            RuleFor(x => x.Password).NotNull().NotEmpty().MinimumLength(6)
                .MaximumLength(16).WithName("密码");
        }
    }
}
