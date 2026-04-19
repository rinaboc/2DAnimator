namespace Assets.Scripts.Utility.MVI
{
    public interface IReducer<TState>
    {
        TState Reduce(TState previous, IIntent intent);
        TState Update(TState previous, IModelContext context);
    }
}
