using Microsoft.IdentityModel.Tokens;

namespace ThreeL.ContextAPI.Application.Contract.Services
{
    public interface IJwtService : IAppService
    {
        IEnumerable<SecurityKey> ValidateIssuerSigningKey(string token, SecurityToken securityToken, string kid, TokenValidationParameters validationParameters);
    }
}
