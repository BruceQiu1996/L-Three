using ThreeL.ContextAPI.Application.Contract.Services;

namespace ThreeL.Shared.Application.Contract.Services
{
    public interface IPreheatService
    {
        Task PreheatAsync();
    }
}
