using System.ComponentModel.DataAnnotations;
using FluentValidation;
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

// TDD stubs — rules are added in the GREEN step.
public sealed class CreateTaskRequestValidator : AbstractValidator<CreateTaskRequest>;

public sealed class UpdateTaskRequestValidator : AbstractValidator<UpdateTaskRequest>;

public sealed class ChangeTaskStatusRequestValidator : AbstractValidator<ChangeTaskStatusRequest>;
