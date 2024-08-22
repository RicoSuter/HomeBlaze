using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions;
using System.ComponentModel;
using System;

namespace HomeBlaze.Widgets
{
    [DisplayName("Browser")]
    [ThingWidget(typeof(BrowserWidgetComponent))]
    public class BrowserWidget : IThing
    {
        [Configuration(IsIdentifier = true)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string? Title => BrowserUrl;

        [Configuration]
        public string? BrowserUrl { get; set; } = "https://github.com/RicoSuter/HomeBlaze";

        [Configuration]
        public int Width { get; set; } = 400;

        [Configuration]
        public int Height { get; set; } = 300;
    }
}