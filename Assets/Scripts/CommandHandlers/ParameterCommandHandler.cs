using System;
using System.Collections.Generic;
using Assets.Scripts.Utility.MVI;

public class ParameterCommandHandler : ICommandHandler
{
    public void Execute(IIntent intent, object state, IModelContext context)
    {
        switch (intent)
        {
            case SelectParameterIntent select: ExecuteSelectParameter(select, state, context); break;
            case CreateParameterIntent create: ExecuteCreateParameter(create, state, context); break;
            case CreateParamPointsIntent _: ExecuteCreateParamPoints(state, context); break;
            case DeleteSelectedParameterIntent _: ExecuteDeleteSelectedParameter(state, context); break;
            case UpdateParameterIntent update: ExecuteUpdateParameter(update, state, context); break;
            case OpenParameterCreatorIntent _: ExecuteOpenParameterCreator(state, context); break;
            case SelectLayerIntent select: ExecuteSelectLayer(select, state, context); break;
            case DeleteLayerIntent _: ExecuteDeleteLayer(state, context); break;
        }
    }

    private void ExecuteDeleteLayer(object state, IModelContext context)
    {
        ParameterManager.Instance.HighlightCurves(new());
    }

    private void ExecuteSelectLayer(SelectLayerIntent select, object state, IModelContext context)
    {
        List<Guid> paramIDs = context.ParamCurves.GetAssignedParamIDsOfMesh(select.LayerID);
        ParameterManager.Instance.HighlightCurves(paramIDs);
    }

    private void ExecuteOpenParameterCreator(object state, IModelContext context)
    {
        context.GeneralSettings.SelectedParamID = Guid.Empty;
    }

    private void ExecuteUpdateParameter(UpdateParameterIntent update, object state, IModelContext context)
    {
        if (context.Parameters.TryGet(update.ParamID, out Parameter parameter))
        {
            parameter.MinValue = update.Min;
            parameter.MaxValue = update.Max;
            parameter.DefaultValue = update.Default;
            parameter.Name = update.Name;
        }
    }

    private void ExecuteDeleteSelectedParameter(object state, IModelContext context)
    {
        if (context.GeneralSettings.SelectedParamID == Guid.Empty) return;

        Guid selectedParamID = context.GeneralSettings.SelectedParamID;
        context.Parameters.Remove(selectedParamID);
        ParameterManager.Instance.DeleteParameterSlider(selectedParamID);

        ParameterManager.Instance.DispatchToParameterStore(new DeletedParameterIntent(selectedParamID));
        context.GeneralSettings.SelectedParamID = Guid.Empty;
    }

    private void ExecuteCreateParamPoints(object state, IModelContext context)
    {
        if (!context.Parameters.TryGet(context.GeneralSettings.SelectedParamID, out Parameter parameter)) return;
        if (context.GeneralSettings.SelectedMeshID == Guid.Empty) return;

        ParamCurve paramCurve = new(context.GeneralSettings.SelectedMeshID, parameter.ID, autoRegister: false);
        ParamPoint minPoint = new(parameter.MinValue, autoRegister: false);
        ParamPoint maxPoint = new(parameter.MaxValue, autoRegister: false);

        context.ParamCurves.Register(paramCurve);
        context.ParamPoints.Register(minPoint);
        context.ParamPoints.Register(maxPoint);

        paramCurve.ParamPoints.Add(minPoint.ID);
        paramCurve.ParamPoints.Add(maxPoint.ID);

        parameter.ParamCurves.Add(paramCurve.ID);

        List<float> paramValues = new()
        {
            minPoint.ParamValue,
            maxPoint.ParamValue
        };

        if (Math.Abs(parameter.MinValue - parameter.DefaultValue) > 0.1f
        && Math.Abs(parameter.MaxValue - parameter.DefaultValue) > 0.1f)
        {
            ParamPoint midPoint = new(parameter.DefaultValue, autoRegister: false);

            context.ParamPoints.Register(midPoint);

            paramCurve.ParamPoints.Add(midPoint.ID);
            paramValues.Add(midPoint.ParamValue);
        }

        ParameterManager.Instance.GetParamSlider(parameter.ID).CreateParamPointHandles(paramValues);
        List<Guid> assignedParams = context.ParamCurves.GetAssignedParamIDsOfMesh(context.GeneralSettings.SelectedMeshID);
        ParameterManager.Instance.HighlightCurves(assignedParams);
    }

    private void ExecuteCreateParameter(CreateParameterIntent create, object state, IModelContext context)
    {
        Parameter parameter = new(create.Min, create.Max, create.Default, create.Name, autoRegister: false)
        {
            ID = create.ParamID
        };
        context.Parameters.Register(parameter);
        ParameterManager.Instance.CreateParameterSlider(create.ParamID);
    }

    private void ExecuteSelectParameter(SelectParameterIntent select, object state, IModelContext context)
    {
        if (context.Parameters.TryGet(select.ParamID, out _))
        {
            context.GeneralSettings.SelectedParamID = select.ParamID;
        }
        else
        {
            context.GeneralSettings.SelectedParamID = Guid.Empty;
        }

    }
}
