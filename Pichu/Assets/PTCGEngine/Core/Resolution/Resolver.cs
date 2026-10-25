using System;
using tylrpage.Pichu.PTCGEngine.CardDatabase;

namespace tylrpage.Pichu.PTCGEngine.Core.Resolution
{
    public static class Resolver
    {
        public static CommandResult Submit(GameState state, PlayerCommand command, ICardDatabase db)
        {
            // todo: Hook up rule checker here
            // if (rejected)
            // {
            //     return CommandResult.Rejected(state, "");
            // }
            
            ResolveContext ctx = new()
            {
                WorkingState = state.Clone(),
                CardDatabase = db,
                Queue = new(),
                Events = new(),
            };
            
            // todo:
            // Turn command into game steps and queue them

            int iterations = 0;
            while (true)
            {
                while (ctx.WorkingState.Phase != GamePhase.GameOver && ctx.Queue.Count > 0)
                {
                    GameStep step = ctx.Queue.First.Value;
                    ctx.Queue.RemoveFirst();
                
                    // Iteration stop gap
                    if (iterations >= 1000)
                    {
                        throw new InvalidOperationException("Reached game step limit");
                    }
                    
                    step.Run(ctx);

                    iterations++;
                }

                if (ctx.WorkingState.Phase == GamePhase.GameOver)
                {
                    break;
                }

                if (!RunAfterQueueChecks(ctx))
                {
                    break;
                }
            }

            if (ctx.WorkingState.Phase == GamePhase.GameOver)
            {
                return CommandResult.GameOver(ctx.WorkingState, ctx.Events);
            }
            return CommandResult.Ok(ctx.WorkingState, ctx.Events);
        }
        
        // Returns true if any new steps were queued
        private static bool RunAfterQueueChecks(ResolveContext ctx)
        {
            bool stepQueued = false;
            void QueueNext(params GameStep[] gameStep)
            {
                ctx.QueueNext(gameStep);
                stepQueued = true;
            }
            
            // Win checks
            if (ctx.WorkingState.Player1State.FailedToDrawCard)
            {
                ctx.WorkingState.Player1Won = false;
                ctx.WorkingState.Phase = GamePhase.GameOver;
                ctx.Events.Add(new PlayerLost()
                {
                    IsPlayer1 = true,
                });

                return stepQueued;
            }
            else if (ctx.WorkingState.Player2State.FailedToDrawCard)
            {
                ctx.WorkingState.Player1Won = true;
                ctx.WorkingState.Phase = GamePhase.GameOver;
                ctx.Events.Add(new PlayerLost()
                {
                    IsPlayer1 = false,
                });
                
                return stepQueued;
            }

            if (ctx.WorkingState.EndTurnRequested)
            {
                ctx.WorkingState.EndTurnRequested = false;
                QueueNext(new StartTurnStep()
                {
                    IsPlayer1 = !ctx.WorkingState.IsPlayer1Turn,
                });
            }

            return stepQueued;
        }
    }
}