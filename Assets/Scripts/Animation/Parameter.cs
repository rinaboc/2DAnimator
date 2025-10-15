using System.Collections.Generic;

public class Parameter
{
    private static ushort _paramCounter = 0;

    public readonly ushort ID;
    public float minValue;
    public float maxValue;
    public float defaultValue;

    public readonly List<ushort> ParamCurves;

    public Parameter(float min, float max, float defaultValue, bool autoRegister = true)
    {
        ID = _paramCounter++;
        minValue = min;
        maxValue = max;
        this.defaultValue = defaultValue;

        ParamCurves = new();

        if (autoRegister)
        {
            ParameterRegistry.instance.RegisterParameter(this);
        }
    }
}
