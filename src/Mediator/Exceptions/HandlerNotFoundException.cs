using System;

namespace Zapto.Mediator;

public class HandlerNotFoundException : InvalidOperationException
{
	public HandlerNotFoundException()
	{
	}

	public HandlerNotFoundException(string message) : base(message)
	{
	}

	public HandlerNotFoundException(string message, Exception innerException) : base(message, innerException)
	{
	}
}