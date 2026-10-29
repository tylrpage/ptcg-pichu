using System;
using System.Linq;

namespace tylrpage.Pichu.PTCGEngine.Core
{
    public struct PlayerCommand : IEquatable<PlayerCommand>
    {
        public CommandType Type;
        public bool IsPlayer1;
        public int CardId;
        public int TargetId;
        public int ChoiceRequestId;
        public int[] ChosenIds;
        
        public bool Equals(PlayerCommand other)
        {
            return Type == other.Type &&
                   IsPlayer1 == other.IsPlayer1 &&
                   CardId == other.CardId &&
                   TargetId == other.TargetId &&
                   ChoiceRequestId == other.ChoiceRequestId &&
                   ChosenIds.SequenceEqual(other.ChosenIds);
        }

        public override bool Equals(object obj)
        {
            return obj is PlayerCommand other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, IsPlayer1, CardId, TargetId);
        }

        public static bool operator ==(PlayerCommand left, PlayerCommand right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PlayerCommand left, PlayerCommand right)
        {
            return !(left == right);
        }

        public static PlayerCommand PlaceBasic(bool p1, int basicId)
        {
            return new PlayerCommand()
            {
                Type = CommandType.PlaceBasic,
                IsPlayer1 = p1,
                CardId = basicId,
            };
        }

        public static PlayerCommand AttachEnergy(bool p1, int energyId, int targetId)
        {
            return new PlayerCommand()
            {
                Type = CommandType.ManualAttachEnergy,
                IsPlayer1 = p1,
                CardId = energyId,
                TargetId = targetId,
            };
        }
        
        public static PlayerCommand EndTurn(bool p1)
        {
            return new PlayerCommand()
            {
                Type = CommandType.EndTurn,
                IsPlayer1 = p1,
            };
        }

        public static PlayerCommand Choose(int choiceRequestId, int[] chosenIds)
        {
            return new PlayerCommand()
            {
                Type = CommandType.Choose,
                ChoiceRequestId = choiceRequestId,
                ChosenIds = chosenIds,
            };
        }
    }
}