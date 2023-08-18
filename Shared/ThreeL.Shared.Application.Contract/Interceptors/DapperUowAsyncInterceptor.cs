using Castle.DynamicProxy;
using System.Data;
using System.Transactions;
using ThreeL.Infra.Core.CSharp;
using ThreeL.Infra.Dapper;
using ThreeL.Shared.Application.Contract.Attributes;

namespace ThreeL.Shared.Application.Contract.Interceptors
{
    /// <summary>
    /// dapper添加事务的切面
    /// </summary>
    public class DapperUowAsyncInterceptor : AsyncInterceptorBase
    {
        private readonly DbContext _dbContext;
        private readonly TransactionOptions _transactionOptions;

        public DapperUowAsyncInterceptor(DbContext _dbContex)
        {
            _transactionOptions = new TransactionOptions();
            _transactionOptions.IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted;
            _transactionOptions.Timeout = new TimeSpan(0, 0, 60);
            _dbContext = _dbContex;
        }

        protected async override Task InterceptAsync(IInvocation invocation, IInvocationProceedInfo proceedInfo, Func<IInvocation, IInvocationProceedInfo, Task> proceed)
        {
            var attribute = invocation.GetAttribute<DapperUnitOfWorkAttribute>();
            if (attribute == null)
            {
                await proceed(invocation, proceedInfo);
            }
            else
            {

                using (var transaction = new TransactionScope(TransactionScopeOption.Required, _transactionOptions, TransactionScopeAsyncFlowOption.Enabled))
                {
                    if (_dbContext.DbConnection.State == ConnectionState.Closed)
                        _dbContext.DbConnection.Open();

                    await proceed(invocation, proceedInfo);

                    transaction.Complete();
                }
            }
        }

        protected async override Task<TResult> InterceptAsync<TResult>(IInvocation invocation, IInvocationProceedInfo proceedInfo, Func<IInvocation, IInvocationProceedInfo, Task<TResult>> proceed)
        {
            var attribute = invocation.GetAttribute<DapperUnitOfWorkAttribute>();
            TResult result = default(TResult);
            if (attribute == null)
            {
                result = await proceed(invocation, proceedInfo);
            }
            else
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.Required, _transactionOptions, TransactionScopeAsyncFlowOption.Enabled))
                {
                    if (_dbContext.DbConnection.State == ConnectionState.Closed)
                        _dbContext.DbConnection.Open();

                    result = await proceed(invocation, proceedInfo);

                    transaction.Complete();
                }
            }

            return result;
        }
    }
}
