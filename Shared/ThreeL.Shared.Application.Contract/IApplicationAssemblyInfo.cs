using System.Reflection;

namespace ThreeL.Shared.Application.Contract
{
    public interface IApplicationAssemblyInfo
    {
        Assembly ImplementAssembly { get; }
        Assembly ContractAssembly { get; }
    }
}
