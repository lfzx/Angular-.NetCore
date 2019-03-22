﻿using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace PeopleMatching.Infrastructure.Resources
{
    public class PostResourceValidator : AbstractValidator<PostResource>
    {
        public PostResourceValidator()
        {
            RuleFor(x => x.Author)
                .NotNull()
                .WithName("作者")
                .WithMessage("{PropertyName}是必填的")
                .MaximumLength(50)
                .WithMessage("{PropertyName}的最大长度是{MaxLength}");
        }
    }
}
