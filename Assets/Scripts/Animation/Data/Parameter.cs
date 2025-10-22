using System.Collections.Generic;

public class Parameter
{
    private static ushort _paramCounter = 0;

    public readonly ushort ID;
    private float minValue;
    private float maxValue;
    private float defaultValue;
    public string Name { get; set; }
    public float MinValue
    {
        get => minValue;
        set
        {
            if (value > maxValue || value > defaultValue) return;
            minValue = value;
        }
    }
    public float MaxValue
    {
        get => maxValue;
        set
        {
            if (value < minValue || value < defaultValue) return;
            maxValue = value;
        }
    }
    public float DefaultValue
    {
        get => defaultValue;
        set
        {
            if (value < minValue || value > maxValue) return;
            defaultValue = value;
        }
    }

    public readonly List<ushort> ParamCurves;

    public Parameter(float min, float max, float defaultValue, string name = "parameter", bool autoRegister = true)
    {
        ID = _paramCounter++;
        MinValue = min;
        MaxValue = max;
        DefaultValue = defaultValue;
        Name = name;

        ParamCurves = new();

        if (autoRegister)
        {
            ParameterRegistry.Instance.RegisterParameter(this);
        }
    }
}
