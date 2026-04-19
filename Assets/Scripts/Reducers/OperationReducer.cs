using Assets.Scripts.States;
using Assets.Scripts.Utility.MVI;
using UnityEngine;

public class OperationReducer : IReducer<OperationState>
{
    public OperationState Reduce(OperationState previous, IIntent intent)
    {
        return intent switch
        {
            // UndoIntent _ => ReduceUndo(previous),
            // RedoIntent _ => ReduceRedo(previous),
            // _ => ReduceDefault(previous, intent)
            _ => previous
        };
    }

    public OperationState Update(OperationState previous, IModelContext context)
    {
        var next = previous.Clone();
        next.CanUndo = context.SessionInfo.UndoCount > 0;
        next.CanRedo = context.SessionInfo.RedoCount > 0;
        return next;
    }

    private OperationState ReduceDefault(OperationState previous, IIntent intent)
    {
        var next = previous.Clone();
        if (IntentHelper.IsUndoableIntent(intent))
        {
            next.CanUndo = true;
            next.CanRedo = false;
            next.UndoCount++;
            next.RedoCount = 0;
        }
        else return previous;

        return next;
    }

    private OperationState ReduceRedo(OperationState previous)
    {
        if (previous.RedoCount == 0) return previous;

        var next = previous.Clone();
        if (previous.CanRedo && previous.RedoCount > 0)
        {
            next.CanUndo = true;
            next.UndoCount++;
            next.RedoCount--;
        }

        next.CanRedo = next.RedoCount != 0;

        return next;
    }

    private OperationState ReduceUndo(OperationState previous)
    {
        if (previous.UndoCount == 0) return previous;

        var next = previous.Clone();
        if (previous.CanUndo && previous.UndoCount > 0)
        {
            next.CanRedo = true;
            next.UndoCount--;
            next.RedoCount++;
        }

        next.CanUndo = next.UndoCount != 0;

        return next;
    }
}
