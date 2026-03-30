using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Utility.MVI;

public class ParameterCommandHandler : ICommandHandler
{
    public Action Execute(IIntent intent, IModelContext context)
    {
        return intent switch
        {
            CreateParameterIntent create => ExecuteCreateParameter(create, context),
            UpdateParameterIntent update => ExecuteUpdateParameter(update, context),
            DeleteSelectedParameterIntent _ => ExecuteDeleteSelectedParameter(context),
            SelectParameterIntent select => ExecuteSelectParameter(select, context),
            OpenParameterCreatorIntent _ => ExecuteOpenParameterCreator(context),
            CreateParamPointsIntent _ => ExecuteCreateParamPoints(context),
            InterpolateParameterIntent interpolate => ExecuteInterpolateParameter(interpolate, context),
            InitializeProjectIntent init => ExecuteInitializeProject(init, context),
            TimelineParameterSliderChangedIntent slider => ExecuteTimelineParameterSliderChanged(slider, context),
            _ => null
        };
    }

    private List<float> GetParamPointValues(float min, float max, float def)
    {
        List<float> ret = new() { min, max };

        if (Math.Abs(min - def) > 0.1f && Math.Abs(max - def) > 0.1f)
            ret.Add(def);

        return ret;
    }

    private Action ExecuteCreateParamPoints(IModelContext context)
    {
        if (!context.Parameters.TryGet(context.SessionInfo.SelectedParamID, out Parameter parameter)) return null;
        if (!context.Meshes.TryGet(context.SessionInfo.SelectedMeshID, out MeshData mesh)) return null;

        if (context.ParamCurves.GetAssignedParamIDsOfMesh(mesh.ID).Contains(parameter.ID)) return null;

        List<float> paramValues = GetParamPointValues(parameter.MinValue, parameter.MaxValue, parameter.DefaultValue);
        ParamCurve paramCurve = new(mesh.ID, parameter.ID);
        foreach (var value in paramValues)
        {
            ParamPoint point = new(value);
            context.ParamPoints.Register(point);
            paramCurve.ParamPoints.Add(point.ID);
        }

        context.ParamCurves.Register(paramCurve);
        parameter.ParamCurves.Add(paramCurve.ID);

        ParameterManager.Instance.GetParamSlider(parameter.ID).CreateParamPointHandles(paramValues);
        ParameterManager.Instance.HighlightCurves(context.ParamCurves.GetAssignedParamIDsOfMesh(mesh.ID));

        return () =>
        {
            foreach (var pointID in paramCurve.ParamPoints)
                context.ParamPoints.Remove(pointID);
            context.ParamCurves.Remove(paramCurve.ID);
            parameter.ParamCurves.Remove(paramCurve.ID);

            ParameterManager.Instance.GetParamSlider(parameter.ID).DeleteParamPointHandles();
            ParameterManager.Instance.HighlightCurves(new());
        };
    }

    private Action ExecuteOpenParameterCreator(IModelContext context)
    {
        var previousID = context.SessionInfo.SelectedParamID;
        context.SessionInfo.SelectedParamID = Guid.Empty;

        return () => context.SessionInfo.SelectedParamID = previousID;
    }

    private Action ExecuteSelectParameter(SelectParameterIntent select, IModelContext context)
    {
        var previousID = context.SessionInfo.SelectedParamID;
        context.SessionInfo.SelectedParamID = select.ParamID;

        return () =>
        {
            context.SessionInfo.SelectedParamID = previousID;
        };
    }

    private Action ExecuteDeleteSelectedParameter(IModelContext context)
    {
        if (context.SessionInfo.SelectedParamID == Guid.Empty) return null;
        if (!context.Parameters.TryGet(context.SessionInfo.SelectedParamID, out Parameter parameter)) return null;

        ParameterManager.Instance.GetParamSlider(parameter.ID).DeleteParamPointHandles();
        ParameterManager.Instance.DeleteParameterSlider(parameter.ID);

        List<ParamCurve> deletedCurves = new();
        Dictionary<Guid, ParamPoint> deletedPoints = new();

        foreach (var curve in context.ParamCurves.GetAll())
        {
            if (curve.ParamID == parameter.ID)
            {
                foreach (var pointID in curve.ParamPoints)
                {
                    context.ParamPoints.TryGet(pointID, out ParamPoint point);
                    deletedPoints.Add(pointID, point);

                    context.ParamPoints.Remove(pointID);
                }

                deletedCurves.Add(curve);
                context.ParamCurves.Remove(curve.ID);
            }
        }

        return () =>
        {
            ParameterManager.Instance.CreateParameterSlider(parameter.ID);
            context.Parameters.Register(parameter);

            foreach (var pp in deletedPoints) context.ParamPoints.Register(pp.Value);
            foreach (var pc in deletedCurves)
            {
                context.ParamCurves.Register(pc);
                List<ParamPoint> paramPoints = context.ParamPoints.GetEntries(pc.ParamPoints);
                ParameterManager.Instance.GetParamSlider(pc.ParamID)
                    .CreateParamPointHandles(paramPoints.Select(pp => pp.ParamValue).ToList());
            }
        };
    }

    private Action ExecuteUpdateParameter(UpdateParameterIntent update, IModelContext context)
    {
        if (!context.Parameters.TryGet(update.ParamID, out Parameter parameter)) return null;

        var previousParam = parameter.Clone();

        parameter.MinValue = update.Min;
        parameter.MaxValue = update.Max;
        parameter.DefaultValue = update.Default;
        parameter.Name = update.Name;

        return () =>
        {
            parameter.MinValue = previousParam.MinValue;
            parameter.MaxValue = previousParam.MaxValue;
            parameter.DefaultValue = previousParam.DefaultValue;
            parameter.Name = previousParam.Name;
        };
    }

    private Action ExecuteCreateParameter(CreateParameterIntent create, IModelContext context)
    {
        Parameter parameter = new(create.Min, create.Max, create.Default, create.Name)
        {
            ID = create.ParamID
        };

        context.Parameters.Register(parameter);
        ParameterManager.Instance.CreateParameterSlider(parameter.ID);

        return () =>
        {
            ParameterManager.Instance.DeleteParameterSlider(parameter.ID);
            context.Parameters.Remove(parameter.ID);
        };
    }

    private Action ExecuteTimelineParameterSliderChanged(TimelineParameterSliderChangedIntent slider, IModelContext context)
    {
        AnimationManager.Instance.InterpolateParameter(slider.Value, slider.ParamID, context);
        return null;
    }

    private Action ExecuteInterpolateParameter(InterpolateParameterIntent interpolate, IModelContext context)
    {
        AnimationManager.Instance.InterpolateParameter(interpolate.Value, interpolate.ParamID, context);
        return null;
    }

    private Action ExecuteInitializeProject(InitializeProjectIntent init, IModelContext context)
    {
        context.Parameters.Clear();
        context.ParamCurves.Clear();
        context.ParamPoints.Clear();

        ParameterManager.Instance.ClearParamSliders();
        foreach (Parameter parameter in init.SaveData.Parameters)
        {
            context.Parameters.Register(parameter);
            ParameterManager.Instance.CreateParameterSlider(parameter.ID);
        }

        foreach (ParamPoint point in init.SaveData.ParamPoints)
        {
            context.ParamPoints.Register(point);
        }

        foreach (ParamCurve curve in init.SaveData.ParamCurves)
        {
            context.ParamCurves.Register(curve);
            List<float> paramValues = new();
            foreach (var pointID in curve.ParamPoints)
            {
                if (context.ParamPoints.TryGet(pointID, out ParamPoint point))
                {
                    paramValues.Add(point.ParamValue);
                }
            }
            ParameterManager.Instance.GetParamSlider(curve.ParamID).CreateParamPointHandles(paramValues);
        }

        return null;
    }

}
