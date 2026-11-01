namespace tylrpage.Pichu.PTCGEngine.Core.Resolution
{
    // Base class for steps that process a choice
    public abstract class ApplyChoiceStep : GameStep
    {
        public int ChoiceId;
        // public ChoiceRequest Request;
        public int[] ChosenIds;
        
        public override void Run(ResolverContext ctx)
        {
            throw new System.NotImplementedException();
        }
    }
}