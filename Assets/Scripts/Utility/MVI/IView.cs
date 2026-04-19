namespace Assets.Scripts.Utility.MVI
{
    public interface IView<TDomain, TView> where TDomain : IState<TDomain>
    {
        void Render(TView state);
        void SetViewModel(IViewModel<TDomain, TView> viewModel);
    }
}
