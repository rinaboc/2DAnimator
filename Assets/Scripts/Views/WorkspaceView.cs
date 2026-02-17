using Assets.Scripts.States;
using Assets.Scripts.Utility.MVI;
using UnityEngine.UIElements;

public class WorkspaceView : BaseUIController, IView<OperationState, OperationState>
{
    private IViewModel<OperationState, OperationState> _viewModel;

    override protected void Awake()
    {
        base.Awake();
        ui.Q<Button>("UndoButton").clicked += () => _viewModel?.Send(new UndoIntent());
        ui.Q<Button>("RedoButton").clicked += () => _viewModel?.Send(new RedoIntent());
    }

    public void Render(OperationState state)
    {

    }

    public void SetViewModel(IViewModel<OperationState, OperationState> viewModel)
    {
        _viewModel = viewModel;
        _viewModel?.Bind(this);
    }
}
