﻿using FluentValidation;
using HandmadeProductManagement.ModelViews.CategoryModelViews;

namespace HandmadeProductManagement.Validation.Category
{
    public class CategoryForUpdateDtoValidator : AbstractValidator<CategoryForUpdateDto>
    {
        public CategoryForUpdateDtoValidator()
        {
            RuleFor(category => category.Name)
                .NotEmpty().WithMessage("Category name is required.")
                .MaximumLength(100).WithMessage("Category name must not exceed 100 characters.");


            RuleFor(category => category.Description)
                .MaximumLength(500).WithMessage("Category description must not exceed 500 characters.")
                .When(x => !string.IsNullOrEmpty(x.Description));


        }
    }
}
