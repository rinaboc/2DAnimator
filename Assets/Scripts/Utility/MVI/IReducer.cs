namespace Assets.Scripts.Utility.MVI
{
    public interface IReducer<TState>
    {
        TState Reduce(TState previous, IIntent intent);
    }
}
