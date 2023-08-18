using Castle.DynamicProxy;
using Grpc.Core;
using ThreeL.Infra.Core.CSharp;

namespace ThreeL.SocketServer.Application.Contract.Interceptors
{
    /// <summary>
    /// grpc通信异常
    /// </summary>
    public class GrpcExceptionAsyncInterceptor : AsyncInterceptorBase
    {
        protected async override Task InterceptAsync(IInvocation invocation, IInvocationProceedInfo proceedInfo,
            Func<IInvocation, IInvocationProceedInfo, Task> proceed)
        {
            var attribute = invocation.GetAttribute<GrpcExceptionAttribute>();
            if (attribute == null)
            {
                await proceed(invocation, proceedInfo);
            }
            else
            {
                try
                {
                    await proceed(invocation, proceedInfo);
                }
                catch (RpcException ex)
                {
                    if (ex.StatusCode == StatusCode.Unauthenticated)
                    {
                        throw new Exception("登录凭证过期,请重新登录");
                    }

                    if (ex.StatusCode == StatusCode.PermissionDenied) 
                    {
                        throw new Exception("权限验证异常");
                    }

                    throw new Exception("服务器通信异常");
                }
                catch
                {
                    throw new Exception("服务器通信异常");
                }
            }
        }

        protected async override Task<TResult> InterceptAsync<TResult>(IInvocation invocation, IInvocationProceedInfo proceedInfo,
            Func<IInvocation, IInvocationProceedInfo, Task<TResult>> proceed)
        {
            var attribute = invocation.GetAttribute<GrpcExceptionAttribute>();
            TResult result = default(TResult);
            if (attribute == null)
            {
                result = await proceed(invocation, proceedInfo);
            }
            else
            {
                try
                {
                    result = await proceed(invocation, proceedInfo);
                }
                catch (RpcException ex)
                {
                    if (ex.StatusCode == StatusCode.Unauthenticated)
                    {
                        throw new Exception("登录凭证过期,请重新登录");
                    }

                    if (ex.StatusCode == StatusCode.PermissionDenied)
                    {
                        throw new Exception("权限验证异常");
                    }

                    throw new Exception("服务器通信异常");
                }
                catch
                {
                    throw new Exception("服务器通信异常");
                }
            }

            return result;
        }
    }
}
