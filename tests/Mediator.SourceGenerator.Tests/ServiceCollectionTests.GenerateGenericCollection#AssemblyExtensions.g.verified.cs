//HintName: AssemblyExtensions.g.cs

namespace Zapto.Mediator
{
    internal static class AssemblyExtensions
    {
        public static global::Zapto.Mediator.IMediatorBuilder AddAssemblyHandlers(this global::Zapto.Mediator.IMediatorBuilder builder)
        {
            builder.AddRequestHandler(typeof(global::RequestHandler<>));
            return builder;
        }
    }
}
