using Microsoft.AspNetCore.Components;

using Namotion.Proxy.Abstractions;
using Namotion.Proxy.ChangeTracking;

namespace Namotion.Proxy.Blazor
{
    public class ProxyComponentBase<TProxy> : ComponentBase, IDisposable
        where TProxy : IProxy
    {
        private IDisposable? _subscription;
        private ReadPropertyRecorderScope? _recorder;
        public ProxyPropertyReference[]? _properties;

        [Inject]
        public IObservable<ProxyPropertyChanged>? ProxyPropertyChanges { get; set; }

        [Inject]
        public TProxy? Proxy { get; set; }

        protected override void OnInitialized()
        {
            _subscription = ProxyPropertyChanges!
                .Subscribe(change =>
                {
                    if (_properties?.Any(p => p == change.Property) != false)
                    {
                        InvokeAsync(StateHasChanged);
                    }
                });
        }

        protected override bool ShouldRender()
        {
            var result = base.ShouldRender();
            if (result)
            {
                _recorder = Proxy?.Context?.BeginReadPropertyRecording();
            }

            return result;
        }

        protected override void OnAfterRender(bool firstRender)
        {
            _properties = _recorder?.GetPropertiesAndDispose();
        }

        public virtual void Dispose()
        {
            _subscription?.Dispose();
            _recorder?.Dispose();
        }
    }
}
