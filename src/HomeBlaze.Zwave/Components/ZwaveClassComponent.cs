using HomeBlaze.Abstractions;
using System;

namespace HomeBlaze.Zwave.Components
{
    public abstract class ZwaveClassComponent : IThing, ILastUpdatedProvider
    {
        internal ZwaveDevice ParentDevice { get; private set; }

        public IThing? Parent => ParentDevice;

        public string? Id => ParentDevice?.Id != null ? $"{ParentDevice.Id}:{Class}" : null;

        public virtual string? Title => $"{Class} (Node {ParentDevice?.NodeId.ToString() ?? "n/a"})";

        protected abstract string Class { get; }

        public DateTimeOffset? LastUpdated { get; internal set; }

        public ZwaveClassComponent(ZwaveDevice parent)
        {
            ParentDevice = parent;
        }
    }
}
