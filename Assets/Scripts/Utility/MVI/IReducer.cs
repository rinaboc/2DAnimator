namespace Assets.Scripts.Utility.MVI
{
    public interface IReducer<TState>
    {
        bool CanReduce(IIntent intent);
        TState Reduce(TState previous, IIntent intent);
    }
}
