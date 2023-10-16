using System.Collections.Generic;
using System.Reflection;

using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Namotion.Trackable.AspNetCore.Controllers;

internal class TrackablesControllerFeatureProvider<TController> : IApplicationFeatureProvider<ControllerFeature>
{
    public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
    {
        feature.Controllers.Add(typeof(TController).GetTypeInfo());
    }
}
