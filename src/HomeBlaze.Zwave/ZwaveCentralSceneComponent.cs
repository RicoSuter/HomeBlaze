using HomeBlaze.Abstractions.Inputs;
using HomeBlaze.Abstractions.Presentation;

namespace HomeBlaze.Zwave
{
    public class ZwaveCentralSceneComponent : ZwaveClassComponent, IIconProvider, IButtonDevice
    {
        public ButtonState? ButtonState { get; internal set; } = Abstractions.Inputs.ButtonState.None;

        public string IconName => "fas fa-circle";

        protected override string Class => "CentralScene";

        public ZwaveCentralSceneComponent(ZwaveDevice parent)
            : base(parent)
        {
        }
    }
}
