namespace Assets.Scripts.States
{
    public class OperationState : IState<OperationState>
    {
        public int UndoCount { get; set; }
        public int RedoCount { get; set; }

        public bool CanUndo { get; set; }
        public bool CanRedo { get; set; }

        public bool IsEditMode { get; set; }
        public EditTool CurrentTool { get; set; }

        public OperationState Copy() => new()
        {
            UndoCount = UndoCount,
            RedoCount = RedoCount,
            CanUndo = CanUndo,
            CanRedo = CanRedo,
            IsEditMode = IsEditMode,
            CurrentTool = CurrentTool
        };
    }
}
