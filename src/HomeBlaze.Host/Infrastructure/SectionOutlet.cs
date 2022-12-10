using Microsoft.AspNetCore.Components;

namespace HomeBlaze.Infrastructure
{
    public class SectionOutlet : IComponent, IDisposable
    {
        private static RenderFragment EmptyRenderFragment = builder => { };
        private string? _subscribedName;
        private SectionRegistry? _registry;
        private Action<RenderFragment> _onChangeCallback = f => { };

        public void Attach(RenderHandle renderHandle)
        {
            _onChangeCallback = content => renderHandle.Render(content ?? EmptyRenderFragment);
            _registry = SectionRegistry.GetRegistry(renderHandle);
        }

        public Task SetParametersAsync(ParameterView parameters)
        {
            var suppliedName = parameters.GetValueOrDefault<string>("Name");
            if (suppliedName != _subscribedName)
            {
                if (_subscribedName != null)
                {
                    _registry?.Unsubscribe(_subscribedName, _onChangeCallback);
                }

                if (suppliedName != null)
                {
                    _registry?.Subscribe(suppliedName, _onChangeCallback);
                    _subscribedName = suppliedName;
                }
            }

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            if (_subscribedName != null)
            {
                _registry?.Unsubscribe(_subscribedName, _onChangeCallback);
            }
        }
    }
}
