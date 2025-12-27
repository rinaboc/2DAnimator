namespace Assets.Scripts.Utility.MVI
{
    public interface IView<TState>
    {
        void Render(TState state);
        void SetViewModel(IViewModel<TState> viewModel);
    }
}
