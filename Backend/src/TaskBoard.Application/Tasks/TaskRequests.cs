using System.ComponentModel.DataAnnotations;
using FluentValidation;
using TaskBoard.Application.Common;
using TaskBoard.Domain.Entities;
using TaskBoard.Domain.Enums;

namespace TaskBoard.Application.Tasks;

// [Required] only documents the OpenAPI contract; FluentValidation enforces the rules and messages.
// Value types are nullable so an omitted field is reported as "required" instead of silently defaulting.

public sealed class CreateTaskRequest
{
    [Required]
    public string Title { get; init; } = string.Empty;

    public string? Description { get; init; }

    /// <summary>Local wall-clock date and time in the user's configured timezone, without offset (e.g. <c>2026-10-21T15:00</c>).</summary>
    [Required]
    public DateTime? DueDate { get; init; }
}

public sealed class UpdateTaskRequest
{
    [Required]
    public string Title { get; init; } = string.Empty;

    public string? Description { get; init; }

    /// <summary>Local wall-clock date and time in the user's configured timezone, without offset.</summary>
    [Required]
    public DateTime? DueDate { get; init; }

    [Required]
    public TaskItemStatus? Status { get; init; }
}

public sealed class ChangeTaskStatusRequest
{
    [Required]
    public TaskItemStatus? Status { get; init; }
}

public sealed class CreateTaskRequestValidator : AbstractValidator<CreateTaskRequest>
{
    public CreateTaskRequestValidator()
    {
        RuleFor(x => x.Title).RequiredText("Title", TaskItem.TitleMaxLength);
        RuleFor(x => x.Description).OptionalText("Description", TaskItem.DescriptionMaxLength);
        RuleFor(x => x.DueDate).LocalDueDate();
    }
}

public sealed class UpdateTaskRequestValidator : AbstractValidator<UpdateTaskRequest>
{
    public UpdateTaskRequestValidator()
    {
        RuleFor(x => x.Title).RequiredText("Title", TaskItem.TitleMaxLength);
        RuleFor(x => x.Description).OptionalText("Description", TaskItem.DescriptionMaxLength);
        RuleFor(x => x.DueDate).LocalDueDate();
        RuleFor(x => x.Status).ValidStatus();
    }
}

public sealed class ChangeTaskStatusRequestValidator : AbstractValidator<ChangeTaskStatusRequest>
{
    public ChangeTaskStatusRequestValidator() => RuleFor(x => x.Status).ValidStatus();
}

internal static class TaskValidationRules
{
    public static IRuleBuilderOptions<T, string?> OptionalText<T>(this IRuleBuilderInitial<T, string?> rule, string label, int maxLength) =>
        rule.Must(value => value is null || value.Trim().Length <= maxLength)
            .WithMessage($"{label} must be at most {maxLength} characters");

    public static IRuleBuilderOptions<T, DateTime?> LocalDueDate<T>(this IRuleBuilderInitial<T, DateTime?> rule) =>
        rule.Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("Due date is required")
            // A value with "Z" or an offset would be silently shifted by the JSON deserializer; the API contract
            // is wall-clock time in the user's configured timezone.
            .Must(value => value!.Value.Kind == DateTimeKind.Unspecified)
            .WithMessage("Due date must be a local date and time without a UTC offset (for example 2026-10-21T15:00)");

    public static IRuleBuilderOptions<T, TaskItemStatus?> ValidStatus<T>(this IRuleBuilderInitial<T, TaskItemStatus?> rule) =>
        rule.Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("Status is required")
            .IsInEnum().WithMessage("Status must be todo, inProgress or done");
}
