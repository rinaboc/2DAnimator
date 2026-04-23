using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Utility.MVI;

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
        public List<Guid> LinkedMeshLayers { get; set; }

        public ParameterState()
        {
            ParamPointValues = new();
            LinkedMeshLayers = new();
        }
        public ParameterState(ParameterState ps)
        {
            ID = ps.ID;
            MinValue = ps.MinValue;
            MaxValue = ps.MaxValue;
            DefaultValue = ps.DefaultValue;
            Name = ps.Name;
            IsSelected = ps.IsSelected;
            CurValue = ps.CurValue;
            ParamPointValues = new List<float>(ps.ParamPointValues ?? new());
            LinkedMeshLayers = new List<Guid>(ps.LinkedMeshLayers ?? new());
        }

        public ParameterState(Parameter parameter, ParameterState ps)
        {
            ID = parameter.ID;
            MinValue = parameter.MinValue;
            MaxValue = parameter.MaxValue;
            DefaultValue = parameter.DefaultValue;
            Name = parameter.Name;

            IsSelected = ps.IsSelected;
            CurValue = ps.CurValue;
        }
    }

    public class ParameterStates
    {
        public Dictionary<Guid, ParameterState> Parameters { get; set; }
        public Guid SelectedParamID { get; set; }
        public Guid SelectedMeshLayerID { get; set; }
        public bool IsSettingsOpen { get; set; }

        public ParameterStates()
        {
            Parameters = new();
            SelectedParamID = Guid.Empty;
            SelectedMeshLayerID = Guid.Empty;
            IsSettingsOpen = false;
        }

        public ParameterStates Clone() => new()
        {
            Parameters = Parameters.ToDictionary(
                p => p.Key,
                p => new ParameterState(p.Value)
            ),
            SelectedParamID = SelectedParamID,
            SelectedMeshLayerID = SelectedMeshLayerID,
            IsSettingsOpen = IsSettingsOpen
        };

        public ParameterStates(IModelContext context, ParameterStates ps) : this()
        {
            foreach (Parameter parameter in context.Parameters.GetAll())
            {
                if (!ps.Parameters.TryGetValue(parameter.ID, out var paramState))
                {
                    ParameterState newParamState = new();
                    Parameters.Add(parameter.ID, new ParameterState(parameter, newParamState));
                }
                else
                {
                    Parameters.Add(parameter.ID, new ParameterState(parameter, paramState));
                }
                var param = Parameters[parameter.ID];
                param.IsSelected = parameter.ID == context.SessionInfo.SelectedParamID;
            }
            SelectedParamID = context.SessionInfo.SelectedParamID;
            SelectedMeshLayerID = context.SessionInfo.SelectedMeshID;
            IsSettingsOpen = ps.IsSettingsOpen;
        }
    }
}