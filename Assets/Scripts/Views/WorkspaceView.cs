using Assets.Scripts.States;
using Assets.Scripts.Utility.MVI;
using UnityEngine.UIElements;

public class WorkspaceView : BaseUIController, IView<OperationState, OperationState>
{
    private IViewModel<OperationState, OperationState> _viewModel;
    private Button _undoButton, _redoButton;
    private Button _saveButton, _exitButton;
    private Button _selectButton, _deleteButton, _createButton;
    private VisualElement _editModeHeader;

    override protected void Awake()
    {
        base.Awake();
        _undoButton = ui.Q<Button>("UndoButton");
        _redoButton = ui.Q<Button>("RedoButton");
        _editModeHeader = ui.Q<VisualElement>("EditModeHeader");
        _saveButton = ui.Q<Button>("SaveButton");
        _exitButton = ui.Q<Button>("ExitButton");
        _selectButton = ui.Q<Button>("SelectButton");
        _deleteButton = ui.Q<Button>("DeleteButton");
        _createButton = ui.Q<Button>("CreateButton");
    }

    void Start()
    {
        _undoButton.clicked += () => _viewModel?.Send(new UndoIntent());
        _redoButton.clicked += () => _viewModel?.Send(new RedoIntent());
        _saveButton.clicked += () => _viewModel?.Send(new EndEditModeIntent(true));
        _exitButton.clicked += () => _viewModel?.Send(new EndEditModeIntent(false));
        _selectButton.clicked += () => _viewModel?.Send(new ChangeEditToolIntent(EditTool.SELECT));
        _deleteButton.clicked += () => _viewModel?.Send(new ChangeEditToolIntent(EditTool.DELETE));
        _createButton.clicked += () => _viewModel?.Send(new ChangeEditToolIntent(EditTool.CREATE));
    }

    public void Render(OperationState state)
    {
        _undoButton.SetEnabled(state.CanUndo);
        _redoButton.SetEnabled(state.CanRedo);

        if (state.IsEditMode)
            _editModeHeader.RemoveFromClassList("disable");
        else
            _editModeHeader.AddToClassList("disable");

        _selectButton.RemoveFromClassList("selected");
        _deleteButton.RemoveFromClassList("selected");
        _createButton.RemoveFromClassList("selected");

        switch (state.CurrentTool)
        {
            case EditTool.SELECT:
                _selectButton.AddToClassList("selected");
                break;
            case EditTool.DELETE:
                _deleteButton.AddToClassList("selected");
                break;
            case EditTool.CREATE:
                _createButton.AddToClassList("selected");
                break;
        }
    }

    public void SetViewModel(IViewModel<OperationState, OperationState> viewModel)
    {
        _viewModel = viewModel;
        _viewModel?.Bind(this);
    }
}
