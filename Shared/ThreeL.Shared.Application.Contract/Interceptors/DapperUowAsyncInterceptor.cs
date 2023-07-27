using Castle.DynamicProxy;
using System.Data;
using System.Reflection;
using System.Transactions;
using ThreeL.Infra.Dapper;
using ThreeL.Shared.Application.Contract.Attributes;

namespace ThreeL.Shared.Application.Contract.Interceptors
{
    public class DapperUowAsyncInterceptor : IAsyncInterceptor
    {
        private readonly DbContext _dbContext;

        public DapperUowAsyncInterceptor(DbContext _dbContex)
        {
            _dbContext = _dbContex;
        }

        //异步方法
        public void InterceptAsynchronous(IInvocation invocation)
        {
            var attribute = GetUowAttribute(invocation);
            if (attribute == null)
            {
                invocation.Proceed();
                var task = (Task)invocation.ReturnValue;
                invocation.ReturnValue = task;
            }
            else
            {
                invocation.ReturnValue = InternalInterceptAsynchronous(invocation);
            }
        }

        public async Task InternalInterceptAsynchronous(IInvocation invocation)
        {
            using (var transaction = new TransactionScope())
            {
                if (_dbContext.DbConnection.State == ConnectionState.Closed)
                    _dbContext.DbConnection.Open();
                invocation.Proceed();
                var task = (Task)invocation.ReturnValue;
                await task;
                transaction.Complete();
            }
        }

        //异步方法带返回值
        public void InterceptAsynchronous<TResult>(IInvocation invocation)
        {
            var attribute = GetUowAttribute(invocation);
            if (attribute == null)
            {
                invocation.Proceed();
                var task = (Task<TResult>)invocation.ReturnValue;
                invocation.ReturnValue = task;
            }
            else
            {
                invocation.ReturnValue = InternalInterceptAsynchronous<TResult>(invocation);
            }
        }


        public async Task<TResult> InternalInterceptAsynchronous<TResult>(IInvocation invocation)
        {
            TResult result;
            using (var transaction = new TransactionScope())
            {
                if (_dbContext.DbConnection.State == ConnectionState.Closed)
                    _dbContext.DbConnection.Open();

                invocation.Proceed();
                var task = (Task<TResult>)invocation.ReturnValue;
                result = await task;

                transaction.Complete();
            }

            return result;
        }

        //同步方法
        public void InterceptSynchronous(IInvocation invocation)
        {
            var attribute = GetUowAttribute(invocation);
            if (attribute == null)
                invocation.Proceed();
            else
                InternalInterceptSynchronous(invocation);
        }

        public void InternalInterceptSynchronous(IInvocation invocation)
        {
            using (var transaction = new TransactionScope())
            {
                if (_dbContext.DbConnection.State == ConnectionState.Closed)
                    _dbContext.DbConnection.Open();

                invocation.Proceed();
                transaction.Complete();
            }
        }


        private DapperUnitOfWorkAttribute GetUowAttribute(IInvocation invocation)
        {
            var methodInfo = invocation.Method ?? invocation.MethodInvocationTarget;
            var attribute = methodInfo.GetCustomAttribute<DapperUnitOfWorkAttribute>();

            return attribute;
        }
    }
}
