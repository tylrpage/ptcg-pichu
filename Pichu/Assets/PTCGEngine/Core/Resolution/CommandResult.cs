
using System.Collections.Generic;

namespace tylrpage.Pichu.PTCGEngine.Core.Resolution
{
    public class CommandResult
    {
        public enum CommandStatus
        {
            Ok,
            Rejected,
            GameOver,
        }
        public CommandStatus Status;
        public GameState State;
        public List<GameEvent> Events = new();
        public string RejectReason;

        public static CommandResult Ok(GameState state, List<GameEvent> events)
        {
            return new CommandResult()
            {
                Status = CommandStatus.Ok,
                State = state,
                Events = events,
            };
        }
        
        public static CommandResult Rejected(GameState state, string rejectionReason)
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
    }
}