namespace Shopizy.Application.Common.EmailTemplates;

/// <summary>
/// Centralized repository of email subjects and HTML/text message templates.
/// </summary>
public static class EmailTemplates
{
    private static string WrapHtml(string title, string contentHtml)
    {
        return $$"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
                <meta charset="utf-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <title>{{title}}</title>
                <style>
                    body {
                        font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
                        background-color: #f8fafc;
                        margin: 0;
                        padding: 0;
                        color: #1e293b;
                    }
                    .email-container {
                        max-width: 600px;
                        margin: 40px auto;
                        background: #ffffff;
                        border-radius: 12px;
                        overflow: hidden;
                        box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -2px rgba(0, 0, 0, 0.1);
                        border: 1px solid #e2e8f0;
                    }
                    .email-header {
                        background: linear-gradient(135deg, #4f46e5 0%, #7c3aed 100%);
                        padding: 32px;
                        text-align: center;
                        color: #ffffff;
                    }
                    .email-header h1 {
                        margin: 0;
                        font-size: 26px;
                        font-weight: 700;
                        letter-spacing: -0.5px;
                    }
                    .email-body {
                        padding: 36px 32px;
                        line-height: 1.6;
                        font-size: 15px;
                    }
                    .btn-primary {
                        display: inline-block;
                        background: #4f46e5;
                        color: #ffffff !important;
                        font-weight: 600;
                        padding: 12px 28px;
                        border-radius: 8px;
                        text-decoration: none;
                        margin: 24px 0;
                        box-shadow: 0 2px 4px rgba(79, 70, 229, 0.3);
                    }
                    .card-box {
                        background-color: #f1f5f9;
                        border-radius: 8px;
                        padding: 16px 20px;
                        margin: 20px 0;
                        border-left: 4px solid #4f46e5;
                    }
                    .email-footer {
                        background-color: #f8fafc;
                        padding: 24px 32px;
                        text-align: center;
                        font-size: 13px;
                        color: #64748b;
                        border-top: 1px solid #e2e8f0;
                    }
                </style>
            </head>
            <body>
                <div class="email-container">
                    <div class="email-header">
                        <h1>Shopizy</h1>
                    </div>
                    <div class="email-body">
                        {{contentHtml}}
                    </div>
                    <div class="email-footer">
                        &copy; {{DateTime.UtcNow.Year}} Shopizy Inc. All rights reserved.<br>
                        This is an automated transactional notification.
                    </div>
                </div>
            </body>
            </html>
            """;
    }

    public static class ForgotPassword
    {
        public const string Subject = "Reset your Shopizy Password";

        public static string BuildBody(string firstName, string resetUrl)
        {
            var content = $"""
                <h2 style="margin-top:0; color:#0f172a; font-size:20px;">Password Reset Request</h2>
                <p>Hi <strong>{firstName}</strong>,</p>
                <p>You recently requested to reset your password for your Shopizy account. Click the button below to choose a new password:</p>
                <div style="text-align: center;">
                    <a href="{resetUrl}" class="btn-primary" target="_blank">Reset My Password</a>
                </div>
                <div class="card-box">
                    <p style="margin:0; font-size:13px; color:#475569; word-break: break-all;">
                        <strong>Direct Link:</strong><br>
                        <a href="{resetUrl}" style="color:#4f46e5;">{resetUrl}</a>
                    </p>
                </div>
                <p style="color:#64748b; font-size:13px;">
                    This reset link will expire in <strong>1 hour</strong>. If you did not request a password reset, you can safely ignore this email — your account remains secure.
                </p>
                <p style="margin-bottom:0;">Warm regards,<br><strong>The Shopizy Team</strong></p>
                """;

            return WrapHtml(Subject, content);
        }
    }

    public static class Welcome
    {
        public const string Subject = "Welcome to Shopizy!";

        public static string BuildBody(string firstName)
        {
            var content = $"""
                <h2 style="margin-top:0; color:#0f172a; font-size:20px;">Welcome to Shopizy! 🎉</h2>
                <p>Hi <strong>{firstName}</strong>,</p>
                <p>Your account has been created successfully. You're all set to discover amazing deals, track your orders, and manage your wishlist.</p>
                <div style="text-align: center;">
                    <a href="https://shopizy.netlify.app" class="btn-primary" target="_blank">Start Shopping</a>
                </div>
                <p style="margin-bottom:0;">Happy shopping,<br><strong>The Shopizy Team</strong></p>
                """;

            return WrapHtml(Subject, content);
        }
    }

    public static class OrderConfirmation
    {
        public static string GetSubject(Guid orderId) => $"Order Confirmation #{orderId}";

        public static string BuildBody(
            string firstName,
            Guid orderId,
            decimal totalAmount,
            string currency
        )
        {
            var content = $"""
                <h2 style="margin-top:0; color:#0f172a; font-size:20px;">Order Confirmation 📦</h2>
                <p>Hi <strong>{firstName}</strong>,</p>
                <p>Thank you for your order! We've received your order and are getting it ready.</p>
                <div class="card-box">
                    <p style="margin:0;"><strong>Order ID:</strong> #{orderId}</p>
                    <p style="margin:6px 0 0 0;"><strong>Total Amount:</strong> {totalAmount:N2} {currency}</p>
                </div>
                <p style="margin-bottom:0;">Thank you for shopping with Shopizy!<br><strong>The Shopizy Team</strong></p>
                """;

            return WrapHtml($"Order Confirmation #{orderId}", content);
        }
    }

    public static class OrderPaid
    {
        public static string GetSubject(Guid orderId) => $"Payment Received for Order #{orderId}";

        public static string BuildBody(string firstName, Guid orderId)
        {
            var content = $"""
                <h2 style="margin-top:0; color:#0f172a; font-size:20px;">Payment Received ✅</h2>
                <p>Hi <strong>{firstName}</strong>,</p>
                <p>We have successfully received payment for your order <strong>#{orderId}</strong>. We are currently processing your shipment.</p>
                <p style="margin-bottom:0;">Thank you for shopping with Shopizy!<br><strong>The Shopizy Team</strong></p>
                """;

            return WrapHtml($"Payment Received for Order #{orderId}", content);
        }
    }

    public static class OrderCancelled
    {
        public static string GetSubject(Guid orderId) => $"Order #{orderId} Cancelled";

        public static string BuildBody(string firstName, Guid orderId, string? reason)
        {
            var content = $"""
                <h2 style="margin-top:0; color:#0f172a; font-size:20px;">Order Cancelled</h2>
                <p>Hi <strong>{firstName}</strong>,</p>
                <p>Your order <strong>#{orderId}</strong> has been cancelled.</p>
                <div class="card-box">
                    <p style="margin:0;"><strong>Reason:</strong> {reason ?? "Customer request"}</p>
                </div>
                <p>If you have any questions or this was unexpected, please feel free to reach out to our customer support team.</p>
                <p style="margin-bottom:0;">Best regards,<br><strong>The Shopizy Team</strong></p>
                """;

            return WrapHtml($"Order #{orderId} Cancelled", content);
        }
    }
}
