namespace Assets.Scripts.Utility.MVI
{
    public interface IViewModel<TDomain, TView>
    {
        void Bind(Store<TDomain> store);
        void Bind(IView<TDomain, TView> view);
        void Unbind(IView<TDomain, TView> view);
        void Unbind();
        void Send(IIntent intent);
    }
}