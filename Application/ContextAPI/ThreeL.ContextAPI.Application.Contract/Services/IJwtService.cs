using Microsoft.IdentityModel.Tokens;

namespace ThreeL.ContextAPI.Application.Contract.Services
{
    public interface IJwtService
    {
        bool ValidateIssuerSigningKey(SecurityKey securityKey, SecurityToken securityToken, TokenValidationParameters validationParameters);
    }
}
