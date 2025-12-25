using System;

namespace Assets.Scripts.Utility
{
    public interface IView<TState>
    {
        void Render(TState state);
        void SetIntentEmitter(Action<IIntent> intentEmitter);
    }
}
