using System.Text.Json.Serialization;

namespace HomeBlaze.Abstractions.Inputs
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ButtonState
    {
        None,
        Down,
        Repeat,
        Press,
        LongPress
    }
}