using Castle.DynamicProxy;
using System.Reflection;

namespace ThreeL.Infra.Core.CSharp
{
    public static class AttributeExtension
    {
        public static T GetAttribute<T>(this IInvocation invocation) where T : Attribute
        {
            var methodInfo = invocation.Method ?? invocation.MethodInvocationTarget;
            var attribute = methodInfo.GetCustomAttribute<T>();

            return attribute;
        }
    }
}
