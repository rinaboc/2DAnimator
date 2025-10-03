using System.Collections.Generic;

public struct Parameter
{
    private static ushort _paramCounter = 0;

    public readonly ushort ID;
    public int minValue;
    public int maxValue;
    public int defaultValue;

    public readonly List<ushort> ParamCurves;

    public Parameter(int min, int max, int defaultValue, bool autoRegister = true)
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
