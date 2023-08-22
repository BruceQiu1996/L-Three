using FluentValidation;
using ThreeL.ContextAPI.Application.Contract.Dtos.User;
using ThreeL.Shared.Application.Contract.Validators;

namespace ThreeL.ContextAPI.Application.Contract.Validators.User
{
    public class UserUpdateDtoValidator : AbstractValidator<UserUpdateAvatarDto>, IMyValidator
    {
        public UserUpdateDtoValidator()
        {
            RuleFor(x => x.Avatar).Must(x => x > 0).WithName("图片编号");
        }
    }
}
