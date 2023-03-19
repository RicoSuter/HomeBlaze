using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace HomeBlaze.Gardena
{
    public abstract class GardenaDevice : IThing
    {
        public abstract string? Id { get; }

        public virtual string? Title { get; set; }

        [State]
        internal string? GardenaId { get; set; }

        internal virtual IEnumerable<GardenaDevice> Children { get; } = Array.Empty<GardenaDevice>();

        internal abstract GardenaDevice Update(JObject data);

        internal abstract GardenaDevice UpdateCommon(JObject data);
    }
}
