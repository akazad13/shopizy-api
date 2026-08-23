using FluentValidation;

namespace Shopizy.Application.Common.Validation;

public static class PasswordRules
{
    public const int MinLength = 12;

    public static IRuleBuilderOptions<T, string> StrongPassword<T>(
        this IRuleBuilder<T, string> rule
    ) =>
        rule.NotEmpty()
            .MinimumLength(MinLength)
            .WithMessage($"Password must be at least {MinLength} characters long.")
            .Matches("[A-Z]")
            .WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]")
            .WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]")
            .WithMessage("Password must contain at least one digit.")
            .Matches("[^a-zA-Z0-9]")
            .WithMessage("Password must contain at least one special character.")
            .Must(password =>
                string.IsNullOrWhiteSpace(password) || !BannedPasswords.Contains(password)
            )
            .WithMessage("Password is too common or easily guessable.");

    private static readonly HashSet<string> BannedPasswords = new(StringComparer.OrdinalIgnoreCase)
    {
        "Compromised1!",
        "BannedPassword1!",
        "SuperSecret1!",
        "AdminPassword1!",
        "DefaultPassword1!",
    };
}
