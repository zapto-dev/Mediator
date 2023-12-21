using System;
using System.Runtime.Serialization;

namespace Zapto.Mediator;

public class NamespaceHandlerNotFoundException : HandlerNotFoundException
{
	public NamespaceHandlerNotFoundException()
	{
	}

	protected NamespaceHandlerNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
	{
	}

	public NamespaceHandlerNotFoundException(string message) : base(message)
	{
	}

	public NamespaceHandlerNotFoundException(string message, Exception innerException) : base(message, innerException)
	{
	}
}
