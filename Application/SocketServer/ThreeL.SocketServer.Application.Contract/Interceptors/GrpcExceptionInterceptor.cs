using Castle.DynamicProxy;

namespace ThreeL.SocketServer.Application.Contract.Interceptors
{
    public class GrpcExceptionInterceptor : IInterceptor
    {
        private readonly GrpcExceptionAsyncInterceptor _grpcExceptionAsyncInterceptor;

        public GrpcExceptionInterceptor(GrpcExceptionAsyncInterceptor grpcExceptionAsyncInterceptor)
        {
            _grpcExceptionAsyncInterceptor = grpcExceptionAsyncInterceptor;
        }

        public void Intercept(IInvocation invocation)
        {
            _grpcExceptionAsyncInterceptor.ToInterceptor().Intercept(invocation);
        }
    }
}
