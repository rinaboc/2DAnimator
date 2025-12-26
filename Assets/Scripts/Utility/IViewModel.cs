namespace Assets.Scripts.Utility
{
    public interface IViewModel<T>
    {
        void Bind(Store<T> store);
        void Bind(IView<T> view);
        void Unbind(IView<T> view);
        void Unbind();
        void Send(IIntent intent);
    }
}