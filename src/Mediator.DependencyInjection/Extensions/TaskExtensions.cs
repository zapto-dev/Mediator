using System.Threading.Tasks;

namespace Zapto.Mediator;

internal static class TaskExtensions
{
    public static ValueTask<T> AsValueTask<T>(this Task<T> task)
    {
        return new ValueTask<T>(task);
    }

    public static ValueTask AsValueTask(this Task task)
    {
        return new ValueTask(task);
    }
}
