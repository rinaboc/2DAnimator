using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Utility.MVI;

namespace Assets.Scripts.States
{
    public class KeyframeState
    {
        public int Frame { get; set; }
        public bool IsSelected { get; set; }
        public float Value { get; set; }

        public KeyframeState() { }
        public KeyframeState(KeyframeState ks)
        {
            Frame = ks.Frame;
            IsSelected = ks.IsSelected;
            Value = ks.Value;
        }
    }

    public class TimelineState
    {
        public bool IsOpen { get; set; }
        public bool IsPlaying { get; set; }
        public bool IsSettingsOpen { get; set; }
        public int CurrentFrame { get; set; }
        public int MaxFrames { get; set; }
        public int FramePerSec { get; set; }

        public Dictionary<Guid, Dictionary<Guid, KeyframeState>> Keyframes { get; set; }

        public TimelineState()
        {
            IsOpen = false;
            IsPlaying = false;
            IsSettingsOpen = false;
            CurrentFrame = 1;
            MaxFrames = 24;
            FramePerSec = 16;
            Keyframes = new();
        }

        public TimelineState Clone() => new()
        {
            IsOpen = IsOpen,
            IsPlaying = IsPlaying,
            IsSettingsOpen = IsSettingsOpen,
            CurrentFrame = CurrentFrame,
            MaxFrames = MaxFrames,
            FramePerSec = FramePerSec,
            Keyframes = Keyframes.ToDictionary(
                p => p.Key,
                p => p.Value.ToDictionary(
                    p => p.Key,
                    p => new KeyframeState(p.Value)
                )
            ),
        };

        public TimelineState(IModelContext context, TimelineState ts) : this()
        {
            IsOpen = context.SessionInfo.TimelineVisibility;
            CurrentFrame = context.SessionInfo.CurrentFrame;
            MaxFrames = context.GeneralSettings.MaxFrames;
            FramePerSec = context.GeneralSettings.FramePerSec;

            IsPlaying = ts.IsPlaying;
            IsSettingsOpen = ts.IsSettingsOpen;

            foreach (Parameter parameter in context.Parameters.GetAll())
            {
                Keyframes.Add(parameter.ID, new Dictionary<Guid, KeyframeState>());
            }

            foreach (KeyFrame keyFrame in context.KeyFrames.GetAll())
            {
                Keyframes[keyFrame.ParamID][keyFrame.ID] = new KeyframeState()
                {
                    Frame = keyFrame.Frame,
                    IsSelected = context.SessionInfo.SelectedKeyframeID == keyFrame.ID,
                    Value = keyFrame.ParamValue
                };
            }
        }
    }
}
