namespace tylrpage.Pichu.PTCGEngine.Core.Resolution
{
    public class AttachEnergyStep : GameStep
    {
        public int EnergyId;
        public int TargetId;
        public bool IsManual;
        
        public override void Run(ResolveContext ctx)
        {
            CardInstance energy = ctx.WorkingState.FindCard(EnergyId);
            CardInstance target = ctx.WorkingState.FindCard(TargetId);
            
            ctx.WorkingState.AttachEnergy(energy, target);
            ctx.Events.Add(new EnergyAttached()
            {
                EnergyId = EnergyId,
                TargetId = TargetId,
            });
            
            if (IsManual)
            {
                ctx.WorkingState.Turn.EnergyAttached = true;
            }
        }
    }
}