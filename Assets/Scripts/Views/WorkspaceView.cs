using Assets.Scripts.States;
using Assets.Scripts.Utility.MVI;
using UnityEngine.UIElements;

public class WorkspaceView : BaseUIController, IView<OperationState, OperationState>
{
    private IViewModel<OperationState, OperationState> _viewModel;
    private Button _undoButton;
    private Button _redoButton;

    override protected void Awake()
    {
        base.Awake();
        _undoButton = ui.Q<Button>("UndoButton");
        _redoButton = ui.Q<Button>("RedoButton");
    }

    void Start()
    {
        _undoButton.clicked += () => _viewModel?.Send(new UndoIntent());
        _redoButton.clicked += () => _viewModel?.Send(new RedoIntent());
    }

    public void Render(OperationState state)
    {
        _undoButton.SetEnabled(state.CanUndo);
        _redoButton.SetEnabled(state.CanRedo);
    }

    public void SetViewModel(IViewModel<OperationState, OperationState> viewModel)
    {
        _viewModel = viewModel;
        _viewModel?.Bind(this);
    }
}
