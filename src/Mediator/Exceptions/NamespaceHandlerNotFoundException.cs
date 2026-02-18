using System;

namespace Zapto.Mediator;

public class NamespaceHandlerNotFoundException : HandlerNotFoundException
{
	public NamespaceHandlerNotFoundException()
	{
	}

	public NamespaceHandlerNotFoundException(string message) : base(message)
	{
	}

	public NamespaceHandlerNotFoundException(string message, Exception innerException) : base(message, innerException)
	{
	}
}
