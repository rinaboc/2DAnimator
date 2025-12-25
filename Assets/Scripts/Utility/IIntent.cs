using System;

public interface IIntent { }

public record UpdateTransformIntent(TransformData Data, TransformType Type) : IIntent;
public record SaveTransformIntent(TransformType Type) : IIntent;
public record InterpolateParameterIntent(Guid ParamID, float Value) : IIntent;
public record InterpolateTransformIntent(Guid MeshID, TransformData Delta) : IIntent;
public record ResetInterpolationIntent(Guid MeshID) : IIntent;

