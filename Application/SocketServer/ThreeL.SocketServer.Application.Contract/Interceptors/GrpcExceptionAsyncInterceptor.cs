using Castle.DynamicProxy;
using Grpc.Core;

namespace ThreeL.SocketServer.Application.Contract.Interceptors
{
    public class GrpcExceptionAsyncInterceptor : IAsyncInterceptor
    {
        //异步方法
        public void InterceptAsynchronous(IInvocation invocation)
        {
            try
            {
                invocation.Proceed();
                var task = (Task)invocation.ReturnValue;
                invocation.ReturnValue = task;
            }
            catch (RpcException ex)
            {
                if (ex.StatusCode == StatusCode.Unauthenticated)
                {
                    throw new Exception("登录凭证过期,请重新登陆");
                }

                throw new Exception("服务器通信异常");
            }
            catch (Exception ex)
            {
                throw new Exception("服务器通信异常");
            }
        }


        //异步方法带返回值
        public void InterceptAsynchronous<TResult>(IInvocation invocation)
        {
            try
            {

                invocation.Proceed();
                var task = (Task<TResult>)invocation.ReturnValue;
                invocation.ReturnValue = task;
            }
            catch (RpcException ex)
            {
                if (ex.StatusCode == StatusCode.Unauthenticated)
                {
                    throw new Exception("登录凭证过期,请重新登陆");
                }

                throw new Exception("服务器通信异常");
            }
            catch (Exception ex)
            {
                throw new Exception("服务器通信异常");
            }
        }

        //同步方法
        public void InterceptSynchronous(IInvocation invocation)
        {
            try
            {

                invocation.Proceed();
            }
            catch (RpcException ex)
            {
                if (ex.StatusCode == StatusCode.Unauthenticated)
                {
                    throw new Exception("登录凭证过期,请重新登陆");
                }

                throw new Exception("服务器通信异常");
            }
            catch (Exception ex)
            {
                throw new Exception("服务器通信异常");
            }
        }
    }
}
