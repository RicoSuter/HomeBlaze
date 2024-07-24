using System.Text.RegularExpressions;

namespace HomeBlaze.Abstractions.Networking
{
    public class IpHelper
    {
        public static bool IsIpValid(string? ip) =>
            new Regex("^(?!0)(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.(?!0)(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.(?!0)(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.(?!0)(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$")
                .IsMatch(ip ?? string.Empty);

        public static int[]? TryGetPorts(string? host)
        {
            try
            {
                return host is not null ?
                    new int[] { int.Parse(host.Split(':')[1]) } :
                    null;
            }
            catch
            {
                return null;
            }
        }

        public static string? TryGetIpAddress(string? host)
        {
            return IsIpValid(host?.Split(':')[0]) ? host?.Split(':')[0] : null;
        }
    }
}
