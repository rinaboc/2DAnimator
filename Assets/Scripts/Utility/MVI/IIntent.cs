using System;
using Assets.Scripts.Utility.MVI;

namespace Assets.Scripts.Utility.MVI
{
    public interface IIntent { }
}

public record UpdateTransformIntent(TransformData Data, TransformType Type) : IIntent;
public record SaveTransformIntent(TransformType Type) : IIntent;
public record InterpolateParameterIntent(Guid ParamID, float Value) : IIntent;
public record InterpolateTransformIntent(Guid MeshID, TransformData Delta) : IIntent;
public record ResetInterpolationIntent(Guid MeshID) : IIntent;
public record SelectParameterIntent(Guid ParamID) : IIntent;
public record DeselectParameterIntent() : IIntent;
public record CreateParameterIntent(Guid ParamID, float Min, float Max, float Default, string Name) : IIntent;
public record DeleteSelectedParameterIntent() : IIntent;
public record CreateParamPointsIntent(Guid MeshID) : IIntent;


