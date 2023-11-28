using System.Collections.Concurrent;
using Shared.Model;

namespace Server.Services;

public class MoveQueue
{
    private readonly ConcurrentQueue<Move> _queue = new();

    public void Enqueue(Move move) => _queue.Enqueue(move);

    public Move[] DequeueAll() => DequeueInternal().ToArray();

    private IEnumerable<Move> DequeueInternal()
    {
        while (_queue.TryDequeue(out var move))
        {
            yield return move;
        }
    }
}
