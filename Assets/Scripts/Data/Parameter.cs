using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Parameter : EntityBase
{
    [SerializeField] private float minValue;
    [SerializeField] private float maxValue;
    [SerializeField] private float defaultValue;
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

    public readonly List<Guid> ParamCurves;

    public Parameter(float min, float max, float defaultValue, string name = "parameter") : base()
    {
        MinValue = min;
        MaxValue = max;
        DefaultValue = defaultValue;
        Name = name;

        ParamCurves = new();
    }
}
