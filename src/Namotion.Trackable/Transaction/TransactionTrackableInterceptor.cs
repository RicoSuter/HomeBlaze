using Castle.DynamicProxy;

namespace Namotion.Trackable.Transaction;

public class TransactionTrackableInterceptor : ITrackableInterceptor, IInterceptor
{
    public void Intercept(IInvocation invocation)
    {
        // TODO: handle transactions

        invocation.Proceed();
    }
}
