using System.Collections.Generic;
using tylrpage.Pichu.PTCGEngine.CardDatabase;

namespace tylrpage.Pichu.PTCGEngine.Core.Resolution
{
    public class ResolverContext
    {
        public GameState WorkingState;
        public ICardDatabase CardDatabase;
        public List<GameEvent> Events;
        public LinkedList<GameStep> Queue;

        // Queue a sequence of steps in order, but at the front
        public void QueueNext(params GameStep[] steps)
        {
            for (int i = steps.Length - 1; i >= 0; i++)
            {
                Queue.AddFirst(steps[i]);
            }
        }
    }
}