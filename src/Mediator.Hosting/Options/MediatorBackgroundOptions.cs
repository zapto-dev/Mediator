namespace Zapto.Mediator.Options;

public class MediatorBackgroundOptions
{
    /// <summary>
    /// Maximum number of concurrent tasks that will be executed.
    /// </summary>
    public int MaxDegreeOfParallelism { get; set; } = 1;

    /// <summary>
    /// Gets or sets whether notifications can be scheduled when the application is stopping.
    /// </summary>
    public bool AllowBackgroundWorkWhileStopping { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the worker items should be canceled when the application is stopping.
    /// </summary>
    public bool CancelWorkerItemsWhenStopping { get; set; } = false;
}
