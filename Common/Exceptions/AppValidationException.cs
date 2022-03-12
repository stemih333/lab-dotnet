using FluentValidation.Results;

namespace Common.Exceptions;

public class AppValidationException : Exception
{
    public readonly IEnumerable<ValidationError>? Errors;

    public AppValidationException(string message, IEnumerable<ValidationError> errors) : base(message)
    {
        Errors = errors;
    }

    public AppValidationException(string message, IEnumerable<ValidationFailure> failures) : base(message)
    {
        Errors = failures
            .Select(_ => new ValidationError
            {
                ErrorMessage = _.ErrorMessage,
                Property = _.PropertyName,
                AttemptedValue = _.AttemptedValue?.ToString()
            });
    }

    public AppValidationException(IEnumerable<ValidationFailure> failures) : base()
    {
        Errors = failures
            .Select(_ => new ValidationError
            {
                ErrorMessage = _.ErrorMessage,
                Property = _.PropertyName,
                AttemptedValue = _.AttemptedValue?.ToString()
            });
    }


}
