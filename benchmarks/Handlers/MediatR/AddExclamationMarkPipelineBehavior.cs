using MediatR;

namespace Benchmarks.Handlers.MediatR;

public class AddExclamationMarkPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, string>
    where TRequest : IRequest<string>
{
    public async Task<string> Handle(TRequest request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken)
    {
        var response = await next();
        return response + "!";
    }
}