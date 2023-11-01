using System;
using System.Reflection;
using Castle.DynamicProxy;

namespace Namotion.Trackable;

public partial class TrackableInterceptor
{
    internal class InvocationInterceptor : IInvocation
    {
        private readonly IInvocation _innerInvocation;
        private readonly Action<IInvocation> _proceedAction;

        public InvocationInterceptor(IInvocation innerInvocation, Action<IInvocation> proceedAction)
        {
            _innerInvocation = innerInvocation;
            _proceedAction = proceedAction;
        }

        public object[] Arguments => _innerInvocation.Arguments;

        public Type[] GenericArguments => _innerInvocation.GenericArguments;

        public object InvocationTarget => _innerInvocation.InvocationTarget;

        public MethodInfo Method => _innerInvocation.Method;

        public MethodInfo MethodInvocationTarget => _innerInvocation.MethodInvocationTarget;

        public object Proxy => _innerInvocation.Proxy;

        public object ReturnValue { get => _innerInvocation.ReturnValue; set => _innerInvocation.ReturnValue = value; }

        public Type TargetType => _innerInvocation.TargetType;

        public IInvocationProceedInfo CaptureProceedInfo()
        {
            return _innerInvocation.CaptureProceedInfo();
        }

        public object GetArgumentValue(int index)
        {
            return _innerInvocation.GetArgumentValue(index);
        }

        public MethodInfo GetConcreteMethod()
        {
            return _innerInvocation.GetConcreteMethod();
        }

        public MethodInfo GetConcreteMethodInvocationTarget()
        {
            return _innerInvocation.GetConcreteMethodInvocationTarget();
        }

        public void Proceed()
        {
            _proceedAction(_innerInvocation);
        }

        public void SetArgumentValue(int index, object value)
        {
            _innerInvocation.SetArgumentValue(index, value);
        }
    }
}
