using FluentValidation;

namespace TaskBoard.Application.Common;

public static class ValidationExtensions
{
    /// <summary>Required, non-whitespace text whose trimmed length is at most <paramref name="maxLength"/>.</summary>
    public static IRuleBuilderOptions<T, string> RequiredText<T>(this IRuleBuilderInitial<T, string> rule, string label, int maxLength) =>
        rule.Cascade(CascadeMode.Stop)
            .Must(value => !string.IsNullOrWhiteSpace(value)).WithMessage($"{label} is required")
            .Must(value => value!.Trim().Length <= maxLength).WithMessage($"{label} must be at most {maxLength} characters");
}
