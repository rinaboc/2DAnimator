using Assets.Scripts.States;
using Assets.Scripts.Utility.MVI;
using UnityEngine.UIElements;

public class WorkspaceView : BaseUIController, IView<OperationState, OperationState>
{
    private IViewModel<OperationState, OperationState> _viewModel;
    private Button _undoButton;
    private Button _redoButton;
    private Button _saveButton;
    private Button _exitButton;
    private VisualElement _editModeHeader;

    override protected void Awake()
    {
        base.Awake();
        _undoButton = ui.Q<Button>("UndoButton");
        _redoButton = ui.Q<Button>("RedoButton");
        _editModeHeader = ui.Q<VisualElement>("EditModeHeader");
        _saveButton = ui.Q<Button>("SaveButton");
        _exitButton = ui.Q<Button>("ExitButton");
    }

    void Start()
    {
        _undoButton.clicked += () => _viewModel?.Send(new UndoIntent());
        _redoButton.clicked += () => _viewModel?.Send(new RedoIntent());
        _saveButton.clicked += () => _viewModel?.Send(new EndEditModeIntent(true));
        _exitButton.clicked += () => _viewModel?.Send(new EndEditModeIntent(false));
    }

    public void Render(OperationState state)
    {
        _undoButton.SetEnabled(state.CanUndo);
        _redoButton.SetEnabled(state.CanRedo);
        if (state.IsEditMode)
            _editModeHeader.RemoveFromClassList("disable");
        else
            _editModeHeader.AddToClassList("disable");
    }

    public void SetViewModel(IViewModel<OperationState, OperationState> viewModel)
    {
        _viewModel = viewModel;
        _viewModel?.Bind(this);
    }
}
