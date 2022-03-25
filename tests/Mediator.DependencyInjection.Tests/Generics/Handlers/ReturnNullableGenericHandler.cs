using System.Threading;
using System.Threading.Tasks;
using Zapto.Mediator;

namespace Mediator.DependencyInjection.Tests.Generics;

public class ReturnNullableGenericHandler<TType> : IRequestHandler<ReturnNullableGenericRequest<TType>, TType?>
    where TType : struct
{
    public ValueTask<TType?> Handle(ReturnNullableGenericRequest<TType> request, CancellationToken cancellationToken)
    {
        return new ValueTask<TType?>(request.Value);
    }
}