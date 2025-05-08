using HomeBlaze.Abstractions;
using Namotion.Interceptor;
using Namotion.Interceptor.Registry;
using Namotion.Interceptor.Tracking;
using Namotion.Interceptor.Tracking.Change;
using Namotion.Interceptor.Validation;

namespace HomeBlaze.Services;

public class ThingManager2 : IDisposable
{
    private readonly ThingManager _thingManager;
    private readonly IInterceptorSubjectContext _context;
    private readonly IDisposable _subscription;

    public ThingManager2(ThingManager thingManager)
    {
        _thingManager = thingManager;

        _context = InterceptorSubjectContext
            .Create()
            .WithRegistry()
            .WithFullPropertyTracking()
            .WithLifecycle()
            .WithDataAnnotationValidation();

        _subscription = _context
            .GetPropertyChangedObservable()
            .Subscribe((args) =>
            {
                if (args.Property.Subject is IThing thing)
                {
                    DetectChanges(thing, args);
                }
            });
    }

    private void DetectChanges(IThing thing, SubjectPropertyChange change)
    {
        _thingManager.DetectChanges(thing);
    }

    public void RegisterSubject(IInterceptorSubject subject)
    {
        subject.Context.AddFallbackContext(_context);
    }

    public void UnregisterSubject(IInterceptorSubject subject)
    {
        subject.Context.RemoveFallbackContext(_context);
    }

    public void Dispose()
    {
        _subscription.Dispose();
    }
}