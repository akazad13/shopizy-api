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
                        background-color: #f1f5f9;
                        margin: 0;
                        padding: 0;
                        color: #0f172a;
                        -webkit-font-smoothing: antialiased;
                    }
                    .email-wrapper {
                        width: 100%;
                        background-color: #f1f5f9;
                        padding: 32px 12px;
                    }
                    .email-container {
                        max-width: 600px;
                        margin: 0 auto;
                        background: #ffffff;
                        border-radius: 16px;
                        overflow: hidden;
                        box-shadow: 0 10px 25px -5px rgba(15, 23, 42, 0.08), 0 8px 10px -6px rgba(15, 23, 42, 0.04);
                        border: 1px solid #e2e8f0;
                    }
                    .email-header {
                        background: linear-gradient(135deg, #0f172a 0%, #1e1b4b 50%, #312e81 100%);
                        padding: 36px 32px;
                        text-align: center;
                        color: #ffffff;
                        position: relative;
                    }
                    .brand-badge {
                        display: inline-block;
                        background: rgba(255, 255, 255, 0.12);
                        border: 1px solid rgba(255, 255, 255, 0.2);
                        backdrop-filter: blur(8px);
                        color: #fbbf24;
                        font-size: 11px;
                        font-weight: 700;
                        text-transform: uppercase;
                        letter-spacing: 1.5px;
                        padding: 4px 14px;
                        border-radius: 9999px;
                        margin-bottom: 12px;
                    }
                    .email-header h1 {
                        margin: 0;
                        font-size: 28px;
                        font-weight: 800;
                        letter-spacing: -0.5px;
                        color: #ffffff;
                    }
                    .email-body {
                        padding: 36px 32px;
                        line-height: 1.65;
                        font-size: 15px;
                        color: #334155;
                    }
                    .btn-primary {
                        display: inline-block;
                        background: linear-gradient(135deg, #4f46e5 0%, #6366f1 100%);
                        color: #ffffff !important;
                        font-weight: 600;
                        font-size: 15px;
                        padding: 14px 32px;
                        border-radius: 10px;
                        text-decoration: none;
                        margin: 20px 0;
                        box-shadow: 0 4px 12px rgba(79, 70, 229, 0.35);
                        text-align: center;
                    }
                    .btn-gold {
                        display: inline-block;
                        background: linear-gradient(135deg, #d97706 0%, #f59e0b 50%, #fbbf24 100%);
                        color: #0f172a !important;
                        font-weight: 700;
                        font-size: 15px;
                        padding: 14px 32px;
                        border-radius: 10px;
                        text-decoration: none;
                        margin: 20px 0;
                        box-shadow: 0 4px 14px rgba(245, 158, 11, 0.35);
                        text-align: center;
                    }
                    .card-box {
                        background-color: #f8fafc;
                        border-radius: 12px;
                        padding: 20px 24px;
                        margin: 24px 0;
                        border: 1px solid #e2e8f0;
                        border-left: 4px solid #4f46e5;
                    }
                    .gold-box {
                        background: #fffbeb;
                        border: 1px solid #fef3c7;
                        border-left: 4px solid #f59e0b;
                        border-radius: 12px;
                        padding: 20px 24px;
                        margin: 24px 0;
                    }
                    .email-footer {
                        background-color: #f8fafc;
                        padding: 28px 32px;
                        text-align: center;
                        font-size: 12px;
                        color: #64748b;
                        border-top: 1px solid #e2e8f0;
                        line-height: 1.6;
                    }
                </style>
            </head>
            <body>
                <div class="email-wrapper">
                    <div class="email-container">
                        <div class="email-header">
                            <div><span class="brand-badge">Official Store</span></div>
                            <h1>Shopizy</h1>
                        </div>
                        <div class="email-body">
                            {{contentHtml}}
                        </div>
                        <div class="email-footer">
                            <p style="margin: 0 0 8px 0; font-weight: 600; color: #475569;">Need assistance? We're here to help.</p>
                            <p style="margin: 0 0 12px 0;">Contact support at <a href="mailto:support@shopizy.com" style="color: #4f46e5; text-decoration: none;">support@shopizy.com</a></p>
                            &copy; {{DateTime.UtcNow.Year}} Shopizy Inc. All rights reserved.<br>
                            This is an automated transactional notification.
                        </div>
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
                <h2 style="margin-top:0; color:#0f172a; font-size:22px; font-weight:700;">Password Reset Request</h2>
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
                <h2 style="margin-top:0; color:#0f172a; font-size:22px; font-weight:700;">Welcome to Shopizy! 🎉</h2>
                <p>Hi <strong>{firstName}</strong>,</p>
                <p>Your account has been created successfully. You're all set to discover amazing deals, track your orders, and manage your wishlist.</p>
                <div style="text-align: center;">
                    <a href="https://shopizy.netlify.app" class="btn-primary" target="_blank">Start Shopping Now</a>
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
            var shortId = orderId.ToString()[..8].ToUpperInvariant();
            var content = $"""
                <h2 style="margin-top:0; color:#0f172a; font-size:22px; font-weight:700;">Order Confirmation 📦</h2>
                <p>Hi <strong>{firstName}</strong>,</p>
                <p>Thank you for shopping with us! We have received your order <strong>#{shortId}</strong> and our team is preparing it.</p>
                <div class="card-box">
                    <table style="width: 100%; border-collapse: collapse; font-size: 14px;">
                        <tr>
                            <td style="padding: 6px 0; color: #64748b;">Order Number:</td>
                            <td style="padding: 6px 0; text-align: right; font-weight: 700; color: #0f172a;">#{shortId}</td>
                        </tr>
                        <tr>
                            <td style="padding: 6px 0; color: #64748b;">Full Reference:</td>
                            <td style="padding: 6px 0; text-align: right; font-family: monospace; font-size: 12px; color: #475569;">{orderId}</td>
                        </tr>
                        <tr style="border-top: 1px solid #e2e8f0;">
                            <td style="padding: 10px 0 0 0; font-weight: 700; color: #0f172a;">Total Amount:</td>
                            <td style="padding: 10px 0 0 0; text-align: right; font-weight: 800; font-size: 16px; color: #4f46e5;">{totalAmount:N2} {currency}</td>
                        </tr>
                    </table>
                </div>
                <p style="margin-bottom:0;">Thank you for shopping with Shopizy!<br><strong>The Shopizy Team</strong></p>
                """;

            return WrapHtml($"Order Confirmation #{orderId}", content);
        }
    }

    /// <summary>
    /// Premium "Your Shopizy order has been processed!" gold-themed receipt template.
    /// </summary>
    public static class OrderPaid
    {
        public static string GetSubject(Guid orderId) =>
            $"Your Shopizy order has been processed! #{orderId}";

        public static string BuildBody(
            string firstName,
            Guid orderId,
            decimal? totalAmount = null,
            string? currency = null,
            string? deliveryMethod = null,
            int? itemsCount = null,
            string? shippingCity = null,
            string? shippingCountry = null,
            string? spaUrl = null
        )
        {
            var shortId = orderId.ToString()[..8].ToUpperInvariant();
            var targetSpaUrl = !string.IsNullOrWhiteSpace(spaUrl)
                ? spaUrl.TrimEnd('/')
                : "https://shopizy.netlify.app";
            var orderTrackUrl = $"{targetSpaUrl}/orders";

            var totalDisplay = totalAmount.HasValue
                ? $"{totalAmount.Value:N2} {(string.IsNullOrWhiteSpace(currency) ? "USD" : currency)}"
                : "Paid in Full";

            var shippingLocation =
                !string.IsNullOrWhiteSpace(shippingCity)
                && !string.IsNullOrWhiteSpace(shippingCountry)
                    ? $"{shippingCity}, {shippingCountry}"
                    : "Default Delivery Address";

            var deliveryTierDisplay = !string.IsNullOrWhiteSpace(deliveryMethod)
                ? $"{deliveryMethod} Delivery"
                : "Standard Delivery (3-5 Days)";

            var content = $"""
                <!-- Hero Section -->
                <div style="text-align: center; margin-bottom: 28px;">
                    <div style="display: inline-block; width: 64px; height: 64px; line-height: 64px; border-radius: 50%; background: linear-gradient(135deg, #fef3c7 0%, #fde68a 100%); border: 2px solid #f59e0b; margin-bottom: 16px; font-size: 30px;">
                        ✨
                    </div>
                    <div style="margin-bottom: 6px;">
                        <span style="display: inline-block; background: #ecfdf5; color: #059669; border: 1px solid #a7f3d0; font-size: 11px; font-weight: 700; text-transform: uppercase; letter-spacing: 1px; padding: 4px 12px; border-radius: 20px;">
                            ✓ Payment Confirmed & Verified
                        </span>
                    </div>
                    <h2 style="margin: 8px 0 4px 0; color: #0f172a; font-size: 24px; font-weight: 800; letter-spacing: -0.5px;">
                        Your Order is Being Processed!
                    </h2>
                    <p style="margin: 0; color: #64748b; font-size: 15px;">
                        Hi <strong>{firstName}</strong>, we've received your payment and our fulfillment team is packing your order.
                    </p>
                </div>

                <!-- Order Details Gold Card -->
                <div style="background: linear-gradient(180deg, #fffdfa 0%, #fffbeb 100%); border: 1px solid #fde68a; border-radius: 14px; padding: 24px; margin: 24px 0; box-shadow: 0 4px 12px rgba(245, 158, 11, 0.08);">
                    <div style="border-bottom: 1px solid #fef3c7; padding-bottom: 14px; margin-bottom: 16px;">
                        <span style="font-size: 12px; font-weight: 700; color: #b45309; text-transform: uppercase; letter-spacing: 1px;">
                            Order Receipt #{shortId}
                        </span>
                    </div>
                    <table style="width: 100%; border-collapse: collapse; font-size: 14px;">
                        <tr>
                            <td style="padding: 7px 0; color: #78716c;">Order Reference:</td>
                            <td style="padding: 7px 0; text-align: right; font-weight: 700; color: #1c1917; font-family: monospace; font-size: 13px;">#{shortId}</td>
                        </tr>
                        <tr>
                            <td style="padding: 7px 0; color: #78716c;">Full ID:</td>
                            <td style="padding: 7px 0; text-align: right; font-family: monospace; font-size: 12px; color: #78716c;">{orderId}</td>
                        </tr>
                        <tr>
                            <td style="padding: 7px 0; color: #78716c;">Shipping Method:</td>
                            <td style="padding: 7px 0; text-align: right; font-weight: 600; color: #1c1917;">{deliveryTierDisplay}</td>
                        </tr>
                        <tr>
                            <td style="padding: 7px 0; color: #78716c;">Ship To:</td>
                            <td style="padding: 7px 0; text-align: right; font-weight: 500; color: #1c1917;">{shippingLocation}</td>
                        </tr>
                        {(itemsCount.HasValue ? $"""
                        <tr>
                            <td style="padding: 7px 0; color: #78716c;">Total Items:</td>
                            <td style="padding: 7px 0; text-align: right; font-weight: 600; color: #1c1917;">{itemsCount.Value} {(itemsCount.Value == 1 ? "item" : "items")}</td>
                        </tr>
                        """ : "")}
                        <tr style="border-top: 1px dashed #fcd34d;">
                            <td style="padding: 12px 0 0 0; font-weight: 700; font-size: 15px; color: #0f172a;">Total Paid:</td>
                            <td style="padding: 12px 0 0 0; text-align: right; font-weight: 800; font-size: 18px; color: #b45309;">{totalDisplay}</td>
                        </tr>
                    </table>
                </div>

                <!-- Fulfillment Progress Tracker -->
                <div style="background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 14px; padding: 20px; margin: 24px 0;">
                    <div style="font-size: 12px; font-weight: 700; color: #475569; text-transform: uppercase; letter-spacing: 1px; margin-bottom: 16px; text-align: center;">
                        Fulfillment Progress
                    </div>
                    <table style="width: 100%; border-collapse: collapse; text-align: center; font-size: 12px;">
                        <tr>
                            <td style="width: 25%; padding: 4px;">
                                <div style="width: 28px; height: 28px; line-height: 28px; border-radius: 50%; background: #10b981; color: #ffffff; font-weight: bold; margin: 0 auto 6px auto;">✓</div>
                                <div style="font-weight: 700; color: #0f172a;">Placed</div>
                            </td>
                            <td style="width: 25%; padding: 4px;">
                                <div style="width: 28px; height: 28px; line-height: 28px; border-radius: 50%; background: #10b981; color: #ffffff; font-weight: bold; margin: 0 auto 6px auto;">✓</div>
                                <div style="font-weight: 700; color: #0f172a;">Paid</div>
                            </td>
                            <td style="width: 25%; padding: 4px;">
                                <div style="width: 28px; height: 28px; line-height: 28px; border-radius: 50%; background: #f59e0b; color: #ffffff; font-weight: bold; margin: 0 auto 6px auto;">⚡</div>
                                <div style="font-weight: 700; color: #b45309;">Processing</div>
                            </td>
                            <td style="width: 25%; padding: 4px;">
                                <div style="width: 28px; height: 28px; line-height: 28px; border-radius: 50%; background: #e2e8f0; color: #94a3b8; font-weight: bold; margin: 0 auto 6px auto;">📦</div>
                                <div style="font-weight: 500; color: #94a3b8;">Delivered</div>
                            </td>
                        </tr>
                    </table>
                </div>

                <!-- Call to Action -->
                <div style="text-align: center; margin: 30px 0;">
                    <a href="{orderTrackUrl}" class="btn-gold" target="_blank">Track Your Order Live →</a>
                </div>

                <!-- Guarantees Badge -->
                <div style="border-top: 1px solid #f1f5f9; padding-top: 20px; margin-top: 24px; font-size: 13px; color: #64748b; text-align: center; line-height: 1.6;">
                    🛡️ <strong>The Shopizy Guarantee:</strong> 100% Authentic Products • 30-Day Hassle-Free Returns • 24/7 Dedicated Support
                </div>
                <p style="margin-top: 24px; margin-bottom: 0; color: #334155; font-size: 14px;">
                    Thank you for choosing Shopizy!<br>
                    <strong>The Shopizy Team</strong>
                </p>
                """;

            return WrapHtml($"Your Shopizy order has been processed! #{orderId}", content);
        }
    }

    public static class OrderCancelled
    {
        public static string GetSubject(Guid orderId) => $"Order #{orderId} Cancelled";

        public static string BuildBody(string firstName, Guid orderId, string? reason)
        {
            var shortId = orderId.ToString()[..8].ToUpperInvariant();
            var content = $"""
                <h2 style="margin-top:0; color:#0f172a; font-size:22px; font-weight:700;">Order Cancelled</h2>
                <p>Hi <strong>{firstName}</strong>,</p>
                <p>Your order <strong>#{shortId}</strong> has been cancelled.</p>
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
