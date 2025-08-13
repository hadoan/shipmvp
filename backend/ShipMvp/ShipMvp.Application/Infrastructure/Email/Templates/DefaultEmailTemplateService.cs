using ShipMvp.Domain.Email.Templates;

namespace ShipMvp.Application.Infrastructure.Email.Templates;

/// <summary>
/// Default email template service with built-in templates
/// </summary>
public class DefaultEmailTemplateService : IEmailTemplateService
{
    private const string AppName = "ShipMVP";
    private const string SupportEmail = "support@shipmvp.com";

    public EmailTemplate GetSignupConfirmationTemplate(string userName, string confirmationUrl, DateTime tokenExpiry)
    {
        var expiryHours = (int)(tokenExpiry - DateTime.UtcNow).TotalHours;

        var htmlContent = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Confirm Your Email - {AppName}</title>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 0; padding: 0; background-color: #f4f4f4; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; }}
        .header {{ background-color: #2563eb; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 30px; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #2563eb; color: white; text-decoration: none; border-radius: 6px; font-weight: bold; margin: 20px 0; }}
        .footer {{ background-color: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #6b7280; }}
        .warning {{ background-color: #fef3c7; border-left: 4px solid #f59e0b; padding: 15px; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>{AppName}</h1>
        </div>
        <div class=""content"">
            <h2>Welcome to {AppName}, {userName}!</h2>
            <p>Thank you for signing up! To complete your registration and start using {AppName}, please confirm your email address by clicking the button below:</p>

            <div style=""text-align: center;"">
                <a href=""{confirmationUrl}"" class=""button"">Confirm Email Address</a>
            </div>

            <div class=""warning"">
                <strong>Important:</strong> This confirmation link will expire in {expiryHours} hours. If you don't confirm your email within this time, you'll need to request a new confirmation email.
            </div>

            <p>If the button doesn't work, you can copy and paste this link into your browser:</p>
            <p style=""word-break: break-all; color: #2563eb;"">{confirmationUrl}</p>

            <p>If you didn't create an account with {AppName}, please ignore this email.</p>

            <p>Best regards,<br>The {AppName} Team</p>
        </div>
        <div class=""footer"">
            <p>Need help? Contact us at <a href=""mailto:{SupportEmail}"">{SupportEmail}</a></p>
            <p>&copy; 2025 {AppName}. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

        var textContent = $@"
Welcome to {AppName}, {userName}!

Thank you for signing up! To complete your registration and start using {AppName}, please confirm your email address by visiting this link:

{confirmationUrl}

Important: This confirmation link will expire in {expiryHours} hours. If you don't confirm your email within this time, you'll need to request a new confirmation email.

If you didn't create an account with {AppName}, please ignore this email.

Best regards,
The {AppName} Team

Need help? Contact us at {SupportEmail}
";

        return new EmailTemplate
        {
            Subject = $"Confirm your email address - {AppName}",
            HtmlContent = htmlContent,
            TextContent = textContent
        };
    }

    public EmailTemplate GetPasswordResetTemplate(string userName, string resetUrl, DateTime tokenExpiry)
    {
        var expiryHours = (int)(tokenExpiry - DateTime.UtcNow).TotalHours;

        var htmlContent = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Reset Your Password - {AppName}</title>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 0; padding: 0; background-color: #f4f4f4; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; }}
        .header {{ background-color: #dc2626; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 30px; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #dc2626; color: white; text-decoration: none; border-radius: 6px; font-weight: bold; margin: 20px 0; }}
        .footer {{ background-color: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #6b7280; }}
        .warning {{ background-color: #fef2f2; border-left: 4px solid #dc2626; padding: 15px; margin: 20px 0; }}
        .security-tip {{ background-color: #f0f9ff; border-left: 4px solid #0ea5e9; padding: 15px; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>{AppName}</h1>
        </div>
        <div class=""content"">
            <h2>Password Reset Request</h2>
            <p>Hi {userName},</p>
            <p>We received a request to reset your password for your {AppName} account. If you made this request, click the button below to reset your password:</p>

            <div style=""text-align: center;"">
                <a href=""{resetUrl}"" class=""button"">Reset Password</a>
            </div>

            <div class=""warning"">
                <strong>Security Notice:</strong> This password reset link will expire in {expiryHours} hours for your security.
            </div>

            <p>If the button doesn't work, you can copy and paste this link into your browser:</p>
            <p style=""word-break: break-all; color: #dc2626;"">{resetUrl}</p>

            <div class=""security-tip"">
                <strong>Security Tip:</strong> If you didn't request this password reset, please ignore this email. Your account remains secure and no changes have been made.
            </div>

            <p>For your security, never share this link with anyone.</p>

            <p>Best regards,<br>The {AppName} Security Team</p>
        </div>
        <div class=""footer"">
            <p>Need help? Contact us at <a href=""mailto:{SupportEmail}"">{SupportEmail}</a></p>
            <p>&copy; 2025 {AppName}. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

        var textContent = $@"
Password Reset Request - {AppName}

Hi {userName},

We received a request to reset your password for your {AppName} account. If you made this request, visit this link to reset your password:

{resetUrl}

Security Notice: This password reset link will expire in {expiryHours} hours for your security.

If you didn't request this password reset, please ignore this email. Your account remains secure and no changes have been made.

For your security, never share this link with anyone.

Best regards,
The {AppName} Security Team

Need help? Contact us at {SupportEmail}
";

        return new EmailTemplate
        {
            Subject = $"Reset your password - {AppName}",
            HtmlContent = htmlContent,
            TextContent = textContent
        };
    }

    public EmailTemplate GetWelcomeTemplate(string userName)
    {
        var htmlContent = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Welcome to {AppName}!</title>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 0; padding: 0; background-color: #f4f4f4; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; }}
        .header {{ background-color: #059669; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 30px; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #059669; color: white; text-decoration: none; border-radius: 6px; font-weight: bold; margin: 20px 0; }}
        .footer {{ background-color: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #6b7280; }}
        .feature-list {{ background-color: #f0fdf4; padding: 20px; border-radius: 8px; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>🎉 Welcome to {AppName}!</h1>
        </div>
        <div class=""content"">
            <h2>Hi {userName}, your account is ready!</h2>
            <p>Congratulations! Your email has been confirmed and your {AppName} account is now active. You're all set to start building amazing things!</p>

            <div class=""feature-list"">
                <h3>🚀 What's next?</h3>
                <ul>
                    <li><strong>Explore the Dashboard:</strong> Get familiar with your new workspace</li>
                    <li><strong>Set up your Profile:</strong> Add your details and preferences</li>
                    <li><strong>Start your first Project:</strong> Begin building your MVP</li>
                    <li><strong>Join our Community:</strong> Connect with other builders</li>
                </ul>
            </div>

            <div style=""text-align: center;"">
                <a href=""https://app.shipmvp.com/dashboard"" class=""button"">Go to Dashboard</a>
            </div>

            <p>If you have any questions or need help getting started, don't hesitate to reach out to our support team.</p>

            <p>Happy building!<br>The {AppName} Team</p>
        </div>
        <div class=""footer"">
            <p>Need help? Contact us at <a href=""mailto:{SupportEmail}"">{SupportEmail}</a></p>
            <p>&copy; 2025 {AppName}. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

        var textContent = $@"
Welcome to {AppName}!

Hi {userName}, your account is ready!

Congratulations! Your email has been confirmed and your {AppName} account is now active. You're all set to start building amazing things!

What's next?
- Explore the Dashboard: Get familiar with your new workspace
- Set up your Profile: Add your details and preferences
- Start your first Project: Begin building your MVP
- Join our Community: Connect with other builders

Visit your dashboard: https://app.shipmvp.com/dashboard

If you have any questions or need help getting started, don't hesitate to reach out to our support team.

Happy building!
The {AppName} Team

Need help? Contact us at {SupportEmail}
";

        return new EmailTemplate
        {
            Subject = $"🎉 Welcome to {AppName} - Your account is ready!",
            HtmlContent = htmlContent,
            TextContent = textContent
        };
    }
}
