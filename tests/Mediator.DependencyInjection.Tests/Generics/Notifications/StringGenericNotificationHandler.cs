namespace Mediator.DependencyInjection.Tests.Generics;

public class StringGenericNotificationHandler(Result result) : GenericNotificationHandler<string>(result);
