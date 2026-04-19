using System;

namespace Assets.Scripts.Utility.MVI
{
    public interface ICommandHandler
    {
        // void Execute(IIntent intent, object state, IModelContext context);
        Action Execute(IIntent intent, IModelContext context);
    }
}
