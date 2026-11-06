using System;
using System.Collections.Generic;
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
            int afterQueueCheckIterations = 0;
            while (true)
            {
                if (afterQueueCheckIterations >= 1000)
                {
                    throw new InvalidOperationException("Reached game step after queue check limit");
                }
                
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
                    afterQueueCheckIterations++;
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
            
            // KO check
            List<CardInstance> cardsInPlay = new();
            for (int i = 0; i < 2; i++)
            {
                PlayerState player = ctx.WorkingState.GetPlayer(i == 0);
                if (player.Active != null)
                {
                    cardsInPlay.Add(player.Active);   
                }
                cardsInPlay.AddRange(player.Bench);
            }
            foreach (CardInstance card in cardsInPlay)
            {
                if (CardQueries.GetRemainingHP(card, ctx.WorkingState, ctx.CardDatabase) <= 0)
                {
                    QueueNext(new KnockOutStep()
                    {
                        CardId = card.Id,
                    });
                }
            }
            if (stepQueued)
            {
                return true;
            }
            
            // Prizes
            if (ctx.WorkingState.Phase == GamePhase.Main)
            {
                for (int i = 0; i < 2; i++)
                {
                    // 0: current turn player, 1: other player, that way current player takes their prizes first
                    bool isPlayer1 = i == (ctx.WorkingState.IsPlayer1Turn ? 0 : 1);
                    PlayerState player = ctx.WorkingState.GetPlayer(isPlayer1);
                    if (player.OwedPrizeCards > 0)
                    {
                        QueueNext(new TakePrizesStep()
                        {
                            IsPlayer1 = isPlayer1,
                            Prizes = player.OwedPrizeCards,
                        });
                    }
                }
                if (stepQueued)
                {
                    return true;
                }   
            }
            
            // Prize win check
            for (int i = 0; i < 2; i++)
            {
                // Check current player first
                bool isPlayer1 = i == (ctx.WorkingState.IsPlayer1Turn ? 0 : 1);
                PlayerState player = ctx.WorkingState.GetPlayer(isPlayer1);
                if (player.Prize.Count == 0)
                {
                    ctx.WorkingState.Player1Won = isPlayer1;
                    ctx.WorkingState.Phase = GamePhase.GameOver;
                    ctx.Events.Add(new PlayerLost()
                    {
                        IsPlayer1 = !isPlayer1,
                    });

                    return stepQueued;
                }
            }
            
            // Deckout loss checks
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
            
            // Promotion when no active
            if (ctx.WorkingState.Phase == GamePhase.Main)
            {
                for (int i = 0; i < 2; i++)
                {
                    // Check current player first
                    bool isPlayer1 = i == (ctx.WorkingState.IsPlayer1Turn ? 0 : 1);
                    PlayerState player = ctx.WorkingState.GetPlayer(isPlayer1);

                    if (player.Active == null)
                    {
                        if (player.Bench.Count > 0)
                        {
                            QueueNext(new RequestChoicePromoteStep()
                            {
                                IsPlayer1 = isPlayer1,
                            });
                        }
                        else
                        {
                            // Donked
                            ctx.WorkingState.Player1Won = !isPlayer1;
                            ctx.WorkingState.Phase = GamePhase.GameOver;
                            ctx.Events.Add(new PlayerLost()
                            {
                                IsPlayer1 = isPlayer1,
                            });
                        }

                        return stepQueued;
                    }
                }
            }

            if (ctx.WorkingState.EndTurnRequested)
            {
                ctx.WorkingState.EndTurnRequested = false;
                QueueNext(new StartTurnStep()
                {
                    IsPlayer1 = !ctx.WorkingState.IsPlayer1Turn,
                });
                
                // Append the event here rather than in the EndTurnStep so it shows up
                // in the log last, not right after the attack for example
                ctx.Events.Add(new TurnEnded()
                {
                    IsPlayer1 = ctx.WorkingState.IsPlayer1Turn,
                });
            }

            return stepQueued;
        }
    }
}