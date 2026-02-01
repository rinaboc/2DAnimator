namespace Assets.Scripts.Utility.MVI
{
    public interface IView<TDomain, TView>
    {
        void Render(TView state);
        void SetViewModel(IViewModel<TDomain, TView> viewModel);
    }
}
