public interface IIntent { }

public record UpdateTransformIntent(TransformData Data, TransformType Type) : IIntent;
public record SaveTransformIntent(TransformType Type) : IIntent;

