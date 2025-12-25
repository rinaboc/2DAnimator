namespace Assets.Scripts.Utility
{
    public interface IReducer<TState>
    {
        TState Reduce(TState previous, IIntent intent);
    }
}
