namespace Assets.Scripts.States
{
    public class OperationState : IState<OperationState>
    {
        public int UndoCount { get; set; }
        public int RedoCount { get; set; }

        public bool CanUndo { get; set; }
        public bool CanRedo { get; set; }

        public OperationState Clone() => new()
        {
            UndoCount = UndoCount,
            RedoCount = RedoCount,
            CanUndo = CanUndo,
            CanRedo = CanRedo
        };
    }
}
