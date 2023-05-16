using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Presentation;
using HomeBlaze.Abstractions.Security;
using HomeBlaze.Abstractions.Services;
using HomeBlaze.Services.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using XboxWebApi.Authentication;

namespace HomeBlaze.Xbox
{
    [DisplayName("Microsoft Xbox Live Account")]
    [ThingSetup(typeof(XboxLiveAccountSetup), CanEdit = true)]
    public class XboxLiveAccount : PollingThing, IIconProvider, IAuthenticatedThing
    {
        internal AuthenticationService? Authentication { get; set; }

        public override string Title => "Xbox Live Account" + (!string.IsNullOrEmpty(Gamertag) ? " (" + Gamertag + ")" : " (Offline)");

        public string IconName => "fas fa-user-circle";

        [State]
        public bool IsAuthenticated => Authentication?.UserInformation != null;

        [State]
        public string? Gamertag => Authentication?.UserInformation?.Gamertag;

        [Configuration(IsSecret = true)]
        public string? AuthenticationJson { get; set; }

        protected override TimeSpan PollingInterval => TimeSpan.FromMinutes(30);

        protected override TimeSpan FailureInterval => TimeSpan.FromMinutes(5);

        public XboxLiveAccount(IThingManager thingManager, ILogger<XboxLiveAccount> logger) 
            : base(thingManager, logger)
        {
        }

        public override async Task PollAsync(CancellationToken cancellationToken)
        {
            var authentication = AuthenticationService.LoadFromJson(AuthenticationJson);
            if (await authentication.AuthenticateAsync())
            {
                Authentication = authentication;
            }
            else
            {
                Authentication = null;
            }
        }
    }
}