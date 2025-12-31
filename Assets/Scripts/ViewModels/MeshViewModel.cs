using Assets.Scripts.States;
using Assets.Scripts.Utility.MVI;

public sealed class MeshViewModel : ViewModelBase<MeshStates>
{
    public Store<MeshStates> GetStore() => _store;
}

