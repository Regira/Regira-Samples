using System.Text.RegularExpressions;
using GeniusTest.Api.Services.Spin;
using Regira.Entities.Models;
using Regira.Entities.Preppers.Abstractions;

namespace GeniusTest.Api.Entities.Questions;

/// <summary>Keeps every question playable and every hand-written reaction positive and spoiler-free.</summary>
public class QuestionPrepper : EntityPrepperBase<Question>
{
    public override Task Prepare(Question modified, Question? original, CancellationToken token = default)
    {
        var errors = new Dictionary<string, string>();

        switch (modified.Type)
        {
            case QuestionType.Choice:
                // null = options not sent (a partial update) -> the stored options stay valid
                if (modified.Options != null)
                {
                    if (modified.Options.Count < 2)
                        errors[nameof(Question.Options)] = "A choice question needs at least 2 options.";
                    else if (!modified.Options.Any(o => o.IsCorrect && !o.IsUnreachable))
                        errors[nameof(Question.Options)] = "Mark at least one option as the official answer.";
                }
                else if (original == null)
                    errors[nameof(Question.Options)] = "A choice question needs at least 2 options.";
                break;
            case QuestionType.Number:
                if (modified.CorrectNumber == null)
                    errors[nameof(Question.CorrectNumber)] = "A number question needs an official answer.";
                break;
            case QuestionType.Rating:
                modified.MinValue ??= 1;
                modified.MaxValue ??= 10;
                break;
        }
        if (modified.MinValue != null && modified.MaxValue != null && modified.MinValue >= modified.MaxValue)
            errors[nameof(Question.MaxValue)] = "Max must be bigger than min. Bigger is better.";

        if (modified.Options != null)
        {
            var correctTexts = modified.Options.Where(o => o.IsCorrect && !string.IsNullOrWhiteSpace(o.Text))
                .Select(o => o.Text!.Trim()).ToList();
            var index = 0;
            foreach (var option in modified.Options)
            {
                // camelCase, like the JSON the SPA binds (toFeedbackError only lower-cases the first letter)
                var key = $"options[{index++}].customReaction";
                if (option.IsUnreachable && option.IsCorrect)
                    errors[$"options[{index - 1}].isCorrect"] = "An unreachable option can't be the official answer. Nobody could ever pick it.";
                else if (PositivityGuard.Validate(option.CustomReaction) is { } message)
                    errors[key] = message;
                else if (!option.IsCorrect && !string.IsNullOrWhiteSpace(option.CustomReaction) && correctTexts.Any(t =>
                             Regex.IsMatch(option.CustomReaction, $@"(?<!\w){Regex.Escape(t)}(?!\w)", RegexOptions.IgnoreCase)))
                    errors[key] = "No spoilers: a reaction may never mention the official answer.";
            }
        }

        if (errors.Count > 0)
        {
            var exception = new EntityInputException<Question>("Question is not genius-proof yet") { Item = modified };
            foreach (var (key, value) in errors) exception.InputErrors[key] = value;
            throw exception;
        }
        return Task.CompletedTask;
    }
}
