using System;
using System.Collections.Generic;

public class ParameterStates
{
    public class ParameterState
    {
        public Guid ID { get; set; }
        public float MinValue { get; set; }
        public float MaxValue { get; set; }
        public float DefaultValue { get; set; }
        public string Name { get; set; }
        public bool IsSelected { get; set; }
    }

    public Dictionary<Guid, ParameterState> Parameters { get; set; }
}