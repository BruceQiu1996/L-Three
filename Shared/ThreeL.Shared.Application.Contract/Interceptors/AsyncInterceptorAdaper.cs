﻿using Castle.DynamicProxy;

namespace ThreeL.Shared.Application.Contract.Interceptors
{
    public class AsyncInterceptorAdaper<TAsyncInterceptor> : AsyncDeterminationInterceptor where TAsyncInterceptor : IAsyncInterceptor
    {
        TAsyncInterceptor _asyncInterceptor;

        public AsyncInterceptorAdaper(TAsyncInterceptor Interceptor) : base(Interceptor)
        {
            _asyncInterceptor = Interceptor;
        }
    }
}
