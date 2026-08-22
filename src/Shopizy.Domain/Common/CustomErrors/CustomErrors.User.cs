using Shopizy.SharedKernel.Domain.Models;

namespace Shopizy.Domain.Common.CustomErrors;

public static partial class CustomErrors
{
    public static class User
    {
        public static DomainError DuplicateEmail =>
            DomainError.Conflict(
                code: "User.DuplicateEmail",
                description: "Email is already in use."
            );
        public static DomainError UserNotFoundWhileLogin =>
            DomainError.Unauthorized(
                code: "User.UserNotFound",
                description: "User is not found with this email & password."
            );
        public static DomainError UserNotFound =>
            DomainError.NotFound(code: "User.UserNotFound", description: "User is not found.");
        public static DomainError UserNotCreated =>
            DomainError.Failure(code: "User.UserNotCreated", description: "Failed to create User.");
        public static DomainError UserNotUpdated =>
            DomainError.Failure(code: "User.UserNotUpdated", description: "Failed to update.");
        public static DomainError PasswordNotCorrect =>
            DomainError.Validation(
                code: "User.PasswordNotCorrect",
                description: "Password is not correct."
            );
        public static DomainError PasswordSameAsOld =>
            DomainError.Validation(
                code: "User.PasswordSameAsOld",
                description: "Password is same as old password."
            );
        public static DomainError PasswordNotUpdated =>
            DomainError.Failure(
                code: "User.PasswordNotUpdated",
                description: "Failed to update password."
            );

        public static DomainError InvalidName =>
            DomainError.Validation(
                code: "User.InvalidName",
                description: "Invalid first/last name."
            );

        public static DomainError InvalidPhoneFormat =>
            DomainError.Validation(
                code: "User.InvalidPhoneFormat",
                description: "Invalid phone format."
            );
        public static DomainError InvalidEmailFormat =>
            DomainError.Validation(
                code: "User.InvalidEmailFormat",
                description: "Invalid email format."
            );
    }
}
