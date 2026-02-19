using System;
using System.Collections.Generic;
using System.Linq;

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
        public Tuple<Guid, Guid> SelectedKeyframe { get; set; }
        public Guid SelectedParamID { get; set; }

        public TimelineState()
        {
            IsOpen = false;
            IsPlaying = false;
            IsSettingsOpen = false;
            CurrentFrame = 1;
            MaxFrames = 24;
            FramePerSec = 16;
            Keyframes = new();
            SelectedKeyframe = new(Guid.Empty, Guid.Empty);
            SelectedParamID = Guid.Empty;
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
            SelectedKeyframe = SelectedKeyframe,
            SelectedParamID = SelectedParamID
        };
    }
}
