using System;
using System.Linq;
using tylrpage.Pichu.PTCGEngine.CardDatabase;
using tylrpage.Pichu.PTCGEngine.Core.Rules;
using tylrpage.Pichu.PTCGEngine.Random;

namespace tylrpage.Pichu.PTCGEngine.Core.Resolution
{
    public static class Resolver
    {
        // todo: temp game setup, replace later with proper setup
        public static CommandResult StartGame(GameState state, ulong seed, ICardDatabase db)
        {
            if (state.Phase != GamePhase.Setup || state.TurnNumber != 0)
            {
                throw new InvalidOperationException("Game has already started");
            }
            
            ResolverContext ctx = CreateContext(state, db);
            
            // Setup rng
            ctx.WorkingState.Rng = new TinyRNG(seed);
            
            // Shuffle decks and draw
            for (int i = 0; i < 2; i++)
            {
                bool isPlayer1 = i == 0 ? true : false;
                ctx.WorkingState.StepQueue.AddLast(new ShuffleDeckStep()
                {
                    IsPlayer1 = isPlayer1,
                });
                ctx.WorkingState.StepQueue.AddLast(new DrawStep()
                {
                    IsPlayer1 = isPlayer1,
                    IsTurnDraw = false,
                    Number = CardQueries.StartingHandSize,
                });
                
                // Put the first basic in active
                ctx.WorkingState.StepQueue.AddLast(new AutoPlaceStartingActiveStep()
                {
                    IsPlayer1 = isPlayer1,
                });
                
                // Setup prize cards
                ctx.WorkingState.StepQueue.AddLast(new SetPrizesStep()
                {
                    IsPlayer1 = isPlayer1,
                });
            }
            
            // Get the game started
            // todo: flip and stuff for who goes first
            ctx.WorkingState.StepQueue.AddLast(new EndSetupStep() { });
            ctx.WorkingState.StepQueue.AddLast(new StartTurnStep() { IsPlayer1 = true });

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
                ctx.WorkingState.PendingChoice = null;
                ctx.WorkingState.LastChoiceResult = new ChoiceResult()
                {
                    Id = command.ChoiceRequestId,
                    ChosenIds = command.ChosenIds.ToArray(),
                };
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