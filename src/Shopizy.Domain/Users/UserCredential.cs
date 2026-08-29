namespace Shopizy.Domain.Users;

/// <summary>
/// Owned type encapsulating all credential-related state for a <see cref="User"/>.
/// Stored in the same Users table row via EF owned-entity mapping.
/// </summary>
public sealed class UserCredential
{
    /// <summary>Gets the user's hashed password.</summary>
    public string? Password { get; private set; }

    /// <summary>Gets the password reset token.</summary>
    public string? PasswordResetToken { get; private set; }

    /// <summary>Gets the password reset token expiry.</summary>
    public DateTime? PasswordResetTokenExpiry { get; private set; }

    internal UserCredential(string? password)
    {
        Password = password;
    }

    // Required by EF Core for owned-entity materialisation
    private UserCredential() { }

    /// <summary>Updates the hashed password.</summary>
    /// <param name="password"></param>
    public void UpdatePassword(string password) => Password = password;

    /// <summary>Sets the password reset token and its expiry.</summary>
    /// <param name="token"></param>
    /// <param name="expiry"></param>
    public void SetPasswordResetToken(string token, DateTime expiry)
    {
        PasswordResetToken = token;
        PasswordResetTokenExpiry = expiry;
    }

    /// <summary>Returns true when <paramref name="token"/> matches and has not expired.</summary>
    /// <param name="token"></param>
    public bool IsPasswordResetTokenValid(string token) =>
        PasswordResetToken == token && PasswordResetTokenExpiry > DateTime.UtcNow;

    /// <summary>Clears the password reset token after use.</summary>
    public void ClearPasswordResetToken()
    {
        PasswordResetToken = null;
        PasswordResetTokenExpiry = null;
    }
}
