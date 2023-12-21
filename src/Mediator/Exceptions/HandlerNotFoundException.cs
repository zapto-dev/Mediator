using System;
using System.Runtime.Serialization;

namespace Zapto.Mediator;

public class HandlerNotFoundException : InvalidOperationException
{
	public HandlerNotFoundException()
	{
	}

	protected HandlerNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
	{
	}

	public HandlerNotFoundException(string message) : base(message)
	{
	}

	public HandlerNotFoundException(string message, Exception innerException) : base(message, innerException)
	{
	}
}