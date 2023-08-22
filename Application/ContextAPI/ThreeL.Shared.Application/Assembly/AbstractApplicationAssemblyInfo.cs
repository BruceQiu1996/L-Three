using ThreeL.Shared.Application.Contract;

namespace ThreeL.Shared.Application.Assembly
{
    public abstract class AbstractApplicationAssemblyInfo : IApplicationAssemblyInfo
    {
        public virtual System.Reflection.Assembly ImplementAssembly { get => GetType().Assembly; }
        public virtual System.Reflection.Assembly ContractAssembly { get => System.Reflection.Assembly.Load(GetType().Assembly.FullName!.Replace("Impl", "Contract")); }
        public virtual System.Reflection.Assembly DomainAssembly { get => System.Reflection.Assembly.Load(GetType().Assembly.FullName!.Replace("Application.Impl", "Domain")); }
    }
}
