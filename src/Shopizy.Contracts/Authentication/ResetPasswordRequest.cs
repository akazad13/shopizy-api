namespace Shopizy.Contracts.Authentication;

public record ResetPasswordRequest(string ResetToken, string NewPassword);
