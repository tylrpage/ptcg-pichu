
using System.Collections.Generic;
using tylrpage.Pichu.PTCGEngine.Core.Rules;

namespace tylrpage.Pichu.PTCGEngine.Core.Resolution
{
    public class CommandResult
    {
        public enum CommandStatus
        {
            Ok,
            Rejected,
            GameOver,
            WaitingForInput,
        }
        public CommandStatus Status;
        public GameState State;
        public List<GameEvent> Events = new();
        public RuleResult RejectReason;

        public static CommandResult Ok(GameState state, List<GameEvent> events)
        {
            return new CommandResult()
            {
                Status = CommandStatus.Ok,
                State = state,
                Events = events,
            };
        }
        
        public static CommandResult Rejected(GameState state, RuleResult rejectionReason)
        {
            return new CommandResult()
            {
                Status = CommandStatus.Rejected,
                State = state,
                RejectReason = rejectionReason,
            };
        }
        
        public static CommandResult GameOver(GameState state, List<GameEvent> events)
        {
            return new CommandResult()
            {
                Status = CommandStatus.GameOver,
                State = state,
                Events = events,
            };
        }

        public static CommandResult WaitingForInput(GameState state, List<GameEvent> events)
        {
            return new CommandResult()
            {
                Status = CommandStatus.WaitingForInput,
                State = state,
                Events = events,
            };
        }
    }
}