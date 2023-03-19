using System;

namespace TeslaAuth
{
    /// <summary>
    /// Multi-factor authentication can fail in at least two ways:
    /// 1) MFA is required for an account and we didn't supply an MFA code
    /// 2) The MFA code entered is invalid or expired.
    /// </summary>
    public class MultiFactorAuthenticationException : Exception
    {
        public MultiFactorAuthenticationException() : base("Multi-factor authentication is required for this account")
        {
        }

        public MultiFactorAuthenticationException(string? message) : base(message)
        {
        }

        public MultiFactorAuthenticationException(string? message, string? accountName) : base(message)
        {
            AccountName = accountName;
        }

        public string? AccountName { get; set; }
    }
}
