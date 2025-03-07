using Microsoft.AspNetCore.Mvc;
using Namotion.Proxy;
using Namotion.Proxy.AspNetCore.Controllers;
using NSwag.Annotations;

namespace HomeBlaze2.Host;

[OpenApiTag("Things")]
[Route("/api/things")]
public class ProxiesController<TProxy> : ProxyControllerBase<TProxy> where TProxy : IProxy
{
    public ProxiesController(TProxy proxy) : base(proxy)
    {
    }
}
