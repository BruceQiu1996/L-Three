using Castle.DynamicProxy;

namespace ThreeL.Shared.Application.Contract.Interceptors
{
    public class DapperUowInterceptor : IInterceptor
    {
        private readonly DapperUowAsyncInterceptor _dapperUowAsyncInterceptor;

        public DapperUowInterceptor(DapperUowAsyncInterceptor dapperUowAsyncInterceptor)
        {
            _dapperUowAsyncInterceptor = dapperUowAsyncInterceptor;
        }

        public void Intercept(IInvocation invocation)
        {
            _dapperUowAsyncInterceptor.ToInterceptor().Intercept(invocation);
        }
    }
}
