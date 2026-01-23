using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.States
{
    public class ParameterState
    {
        public Guid ID { get; set; }
        public float MinValue { get; set; }
        public float MaxValue { get; set; }
        public float DefaultValue { get; set; }
        public string Name { get; set; }
        public bool IsSelected { get; set; }
        public float CurValue { get; set; }
        public List<float> ParamPointValues { get; set; }


        public ParameterState() { ParamPointValues = new(); }
        public ParameterState(ParameterState ps)
        {
            ID = ps.ID;
            MinValue = ps.MinValue;
            MaxValue = ps.MaxValue;
            DefaultValue = ps.DefaultValue;
            Name = ps.Name;
            IsSelected = ps.IsSelected;
            CurValue = ps.CurValue;
            ParamPointValues = ps.ParamPointValues;
        }
    }

    public class ParameterStates
    {
        public Dictionary<Guid, ParameterState> Parameters { get; set; }
        public Guid SelectedParamID { get; set; }
        public bool IsSettingsOpen { get; set; }

        public ParameterStates()
        {
            Parameters = new();
            SelectedParamID = Guid.Empty;
            IsSettingsOpen = false;
        }

        public ParameterStates Clone() => new()
        {
            Parameters = Parameters.ToDictionary(
                p => p.Key,
                p => new ParameterState(p.Value)
            ),
            SelectedParamID = SelectedParamID,
            IsSettingsOpen = IsSettingsOpen
        };

    }
}