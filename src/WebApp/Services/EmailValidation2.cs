using System.Text.RegularExpressions;

namespace eShop.WebApp.Services
{
    // Implement a new EmailValidation class designed to validate e-mail addresses within the project. This should include:
    // - Robust format checking using regular expressions.
    // - Method(s) to return validation status (valid/invalid).
    // - Clear error messaging for invalid e-mails.
    // - Support for both standard and edge-case e-mail formats. 
    public class EmailValidation2
    {
        private const int MaxEmailLength = 254; // RFC 5321 standard
        private const int MaxLocalPartLength = 64; // RFC 5321 standard
        private const int MaxDomainPartLength = 255; // RFC 5321 standard

        private static readonly Regex EmailRegex = new(
            @"^[a-zA-Z0-9]([a-zA-Z0-9._+-]{0,62}[a-zA-Z0-9])?@[a-zA-Z0-9]([a-zA-Z0-9.-]{0,253}[a-zA-Z0-9])?\.[a-zA-Z]{2,}$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase,
            TimeSpan.FromMilliseconds(250));

        public (bool IsValid, string ErrorMessage) ValidateEmail(string? email)
        {
            try
            {
                // Check for null or whitespace
                if (string.IsNullOrWhiteSpace(email))
                {
                    return (false, "Email address cannot be empty.");
                }

                // Trim whitespace
                email = email.Trim();

                // Check maximum length
                if (email.Length > MaxEmailLength)
                {
                    return (false, $"Email address exceeds maximum length of {MaxEmailLength} characters.");
                }

                // Check for @ symbol and split
                var atIndex = email.IndexOf('@');
                if (atIndex == -1)
                {
                    return (false, "Email address must contain an '@' symbol.");
                }

                if (email.IndexOf('@', atIndex + 1) != -1)
                {
                    return (false, "Email address cannot contain multiple '@' symbols.");
                }

                // Validate local and domain parts length
                var localPart = email[..atIndex];
                var domainPart = email[(atIndex + 1)..];

                if (localPart.Length == 0)
                {
                    return (false, "Email address local part cannot be empty.");
                }

                if (localPart.Length > MaxLocalPartLength)
                {
                    return (false, $"Email local part exceeds maximum length of {MaxLocalPartLength} characters.");
                }

                if (domainPart.Length == 0)
                {
                    return (false, "Email address domain cannot be empty.");
                }

                if (domainPart.Length > MaxDomainPartLength)
                {
                    return (false, $"Email domain exceeds maximum length of {MaxDomainPartLength} characters.");
                }

                // Check for consecutive dots
                if (email.Contains(".."))
                {
                    return (false, "Email address cannot contain consecutive dots.");
                }

                // Check for dots at start or end of local/domain parts
                if (localPart.StartsWith('.') || localPart.EndsWith('.'))
                {
                    return (false, "Email local part cannot start or end with a dot.");
                }

                if (domainPart.StartsWith('.') || domainPart.EndsWith('.') || domainPart.StartsWith('-') || domainPart.EndsWith('-'))
                {
                    return (false, "Email domain has invalid format.");
                }

                // Validate with regex
                if (!EmailRegex.IsMatch(email))
                {
                    return (false, "Email address format is invalid.");
                }

                return (true, string.Empty);
            }
            catch (RegexMatchTimeoutException)
            {
                return (false, "Email validation timed out. Please check the format.");
            }
            catch (ArgumentException ex)
            {
                return (false, $"Invalid email format: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Log exception in production scenario
                return (false, $"An unexpected error occurred during validation: {ex.Message}");
            }
        }
    }
}
