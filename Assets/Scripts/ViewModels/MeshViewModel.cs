using Assets.Scripts.States;
using Assets.Scripts.Utility.MVI;

public sealed class MeshViewModel : ViewModelBase<MeshState>
{
    public Store<MeshState> GetStore() => _store;
}

