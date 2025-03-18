using Zapto.Mediator;

namespace Benchmarks.Handlers.Zapto;

public class AddExclamationMarkPipelineBehavior<TRequest> : IPipelineBehavior<TRequest, string>
    where TRequest : global::MediatR.IRequest<string>
{
    public async ValueTask<string> Handle(IServiceProvider provider, TRequest request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken)
    {
        var response = await next();
        return response + "!";
    }
}