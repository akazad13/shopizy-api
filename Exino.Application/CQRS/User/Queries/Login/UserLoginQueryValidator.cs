﻿using FluentValidation;

namespace Exino.Application.CQRS.User.Queries.Login
{
    public class UserLoginQueryValidator : AbstractValidator<UserLoginQueryRequest>
    {
        public UserLoginQueryValidator()
        {
            RuleFor(v => v.Email)
                .MaximumLength(50)
                .NotEmpty()
                .EmailAddress();
            RuleFor(v => v.Password)
                .NotEmpty();
        }
    }
}