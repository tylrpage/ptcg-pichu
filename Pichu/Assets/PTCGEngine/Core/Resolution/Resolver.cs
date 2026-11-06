using System;
using System.Linq;
using tylrpage.Pichu.PTCGEngine.CardDatabase;
using tylrpage.Pichu.PTCGEngine.Core.Rules;
using tylrpage.Pichu.PTCGEngine.Random;

namespace tylrpage.Pichu.PTCGEngine.Core.Resolution
{
    public static class Resolver
    {
        public static CommandResult StartGame(GameState state, ulong seed, ICardDatabase db)
        {
            if (state.Phase != GamePhase.Setup || state.TurnNumber != 0)
            {
                throw new InvalidOperationException("Game has already started");
            }
            
            ResolverContext ctx = CreateContext(state, db);
            
            // Setup rng
            ctx.WorkingState.Rng = new TinyRNG(seed);

            ctx.QueueNext(new GameSetupStep(),
                // todo: let players bench basics before game starts
                new SetPrizesStep() { IsPlayer1 = true },
                new SetPrizesStep() { IsPlayer1 = false },
                new DrawMulliganStep() { IsPlayer1 = true },
                new DrawMulliganStep() { IsPlayer1 = false },
                new EndSetupStep(),
                new StartFirstTurnStep());

            return RunToCompletion(ctx);
        }
        
        public static CommandResult Submit(GameState state, PlayerCommand command, ICardDatabase db)
        {
            RuleResult ruleResult = RuleChecker.Check(command, state, db);
            if (ruleResult != RuleResult.Ok)
            {
                return CommandResult.Rejected(state, ruleResult);
            }

            ResolverContext ctx = CreateContext(state, db);

            if (command.Type == CommandType.Choose)
            {
                if (ctx.WorkingState.PendingChoice == null)
                {
                    throw new InvalidOperationException($"Received choose command but there is no pending choice");
                }
                
                if (ctx.WorkingState.PendingChoice.Id != command.ChoiceRequestId)
                {
                    throw new ArgumentException($"Received choose command but request and choose id don't match");
                }
                
                // Check that next step is an apply choice step
                if (ctx.WorkingState.StepQueue.Count == 0 ||
                    ctx.WorkingState.StepQueue.First.Value is not ApplyChoiceStep applyChoiceStep)
                {
                    throw new InvalidOperationException(
                        $"Received choose command but next step isn't expecting choice, next step: {ctx.WorkingState.StepQueue.First.Value.GetType()}");
                }

                if (applyChoiceStep.ChoiceId != command.ChoiceRequestId)
                {
                    throw new InvalidOperationException(
                        $"Apply choice step was not expecting the choice command id. Expected: {applyChoiceStep.ChoiceId}, got: {command.ChoiceRequestId}");
                }

                applyChoiceStep.ChosenIds = command.ChosenIds.ToArray();

                ctx.WorkingState.PendingChoice = null;
            }
            else
            {
                // Turn command into game steps and queue them
                QueueStepsForCommand(command, ctx);   
            }

            return RunToCompletion(ctx);
        }
        
        private static ResolverContext CreateContext(GameState state, ICardDatabase db)
        {
            return new ResolverContext()
            {
                WorkingState = state.Clone(),
                CardDatabase = db,
                Events = new(),
            };
        }

        // Runs and resolves each of the steps in queue
        private static CommandResult RunToCompletion(ResolverContext ctx)
        {
            int iterations = 0;
            while (true)
            {
                while (ctx.WorkingState.Phase != GamePhase.GameOver &&
                       ctx.WorkingState.StepQueue.Count > 0 &&
                       ctx.WorkingState.PendingChoice == null)
                {
                    GameStep step = ctx.WorkingState.StepQueue.First.Value;
                    ctx.WorkingState.StepQueue.RemoveFirst();
                
                    // Iteration stop gap
                    if (iterations >= 1000)
                    {
                        throw new InvalidOperationException("Reached game step limit");
                    }
                    
                    step.Run(ctx);

                    iterations++;
                }

                // Shouldn't do any win checks until pending choice is made
                if (ctx.WorkingState.PendingChoice == null)
                {
                    if (!RunAfterQueueChecks(ctx))
                    {
                        break;
                    }   
                }
                else
                {
                    // Pending choice pauses the game loop
                    break;
                }
            }

            if (ctx.WorkingState.Phase == GamePhase.GameOver)
            {
                return CommandResult.GameOver(ctx.WorkingState, ctx.Events);
            }

            if (ctx.WorkingState.PendingChoice != null)
            {
                return CommandResult.WaitingForInput(ctx.WorkingState, ctx.Events);
            }
            
            return CommandResult.Ok(ctx.WorkingState, ctx.Events);
        }

        private static void QueueStepsForCommand(PlayerCommand command, ResolverContext ctx)
        {
            switch (command.Type)
            {
                case CommandType.PlaceBasic:
                    ctx.QueueNext(new MoveCardStep()
                    {
                        CardId = command.CardId,
                        To = Zone.Bench,
                    });
                    break;
                case CommandType.ManualAttachEnergy:
                    ctx.QueueNext(new AttachEnergyStep()
                    {
                        EnergyId = command.CardId,
                        TargetId = command.TargetId,
                        IsManual = true,
                    });
                    break;
                case CommandType.EndTurn:
                    ctx.QueueNext(new EndTurnStep());
                    break;
                case CommandType.Attack:
                    ctx.QueueNext(new AttackStep()
                    {
                        IsPlayer1 = command.IsPlayer1,
                        AttackerId = command.CardId,
                        AttackIndexId = command.ActionIndex,
                    });
                    break;
            }
        }
        
        // Returns true if any new steps were queued
        private static bool RunAfterQueueChecks(ResolverContext ctx)
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