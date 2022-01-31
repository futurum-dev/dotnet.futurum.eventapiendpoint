using FluentValidation;

using Futurum.Core.Result;
using Futurum.FluentValidation;

namespace Futurum.EventApiEndpoint;

public interface IEventApiEndpointEventValidation<TEvent>
{
    Task<Result> ExecuteAsync(TEvent @event);
}

public class EventApiEndpointEventValidation<TEvent> : IEventApiEndpointEventValidation<TEvent>
{
    private readonly IValidator<TEvent>[] _validator;

    public EventApiEndpointEventValidation(IEnumerable<IValidator<TEvent>> validator)
    {
        _validator = validator.ToArray();
    }

    public Task<Result> ExecuteAsync(TEvent @event) =>
        _validator.Length switch
        {
            0 => Result.OkAsync(),
            1 => ValidateAsync(_validator[0], @event),
            _ => _validator.FlatMapAsync(validator => ValidateAsync(validator, @event))
        };

    private static Task<Result> ValidateAsync(IValidator<TEvent> validator, TEvent request)
    {
        var validationResult = validator.Validate(request);

        return validationResult.IsValid
            ? Result.OkAsync()
            : Result.FailAsync(validationResult.ToResultError());
    }
}