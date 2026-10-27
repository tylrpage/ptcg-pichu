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
                PlayerState player = ctx.WorkingState.GetPlayer(isPlayer1);
                ctx.QueueNext(new ShuffleDeckStep()
                {
                    IsPlayer1 = isPlayer1,
                });
                ctx.QueueNext(new DrawStep()
                {
                    IsPlayer1 = isPlayer1,
                    IsTurnDraw = false,
                    Number = CardQueries.StartingHandSize,
                });
                
                // Put the first basic in active
                CardInstance firstBasic =
                    player.Hand.FirstOrDefault(x => CardQueries.IsBasicPokemon(x, ctx.WorkingState, db));
                if (firstBasic == null)
                {
                    // todo: add mulligans
                    throw new InvalidOperationException($"Player {(isPlayer1 ? "1" : "2")} has no basics in hand");
                }
                ctx.QueueNext(new MoveCardStep()
                {
                    CardId = firstBasic.Id,
                    To = Zone.Active,
                });
                
                // Setup prize cards
                ctx.QueueNext(new SetPrizesStep()
                {
                    IsPlayer1 = true,
                });
            }
            
            // Get the game started
            // todo: flip and stuff for who goes first
            ctx.QueueNext(new EndSetupStep() { },
                new StartTurnStep() { IsPlayer1 = true });

            return RunToCompletion(ctx);
        }
        
        public static CommandResult Submit(GameState state, PlayerCommand command, ICardDatabase db)
        {
            RuleResult ruleResult = RuleChecker.Check(command, state, db);
            if (ruleResult != RuleResult.Ok)
            {
                return CommandResult.Rejected(state, ruleResult.ToString());
            }

            ResolverContext ctx = CreateContext(state, db);
            
            // Turn command into game steps and queue them
            QueueStepsForCommand(command, ctx);

            return RunToCompletion(ctx);
        }
        
        private static ResolverContext CreateContext(GameState state, ICardDatabase db)
        {
            return new ResolverContext()
            {
                WorkingState = state.Clone(),
                CardDatabase = db,
                Queue = new(),
                Events = new(),
            };
        }

        // Runs and resolves each of the steps in queue
        private static CommandResult RunToCompletion(ResolverContext ctx)
        {
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