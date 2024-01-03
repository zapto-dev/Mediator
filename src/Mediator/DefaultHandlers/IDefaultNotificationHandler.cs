using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Zapto.Mediator;

public interface IDefaultNotificationHandler
{
	/// <summary>
	/// Handles a notification
	/// </summary>
	/// <param name="provider">Current service provider</param>
	/// <param name="notification">The notification</param>
	/// <param name="cancellationToken">Cancellation token</param>
	ValueTask Handle<TNotification>(IServiceProvider provider, TNotification notification, CancellationToken cancellationToken)
		where TNotification : INotification;
}
