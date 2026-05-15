using System;
using Assets.Scripts.Data.MeshInfo;
using UnityEngine;

namespace Assets.Scripts.Utility.MVI
{
    public interface ISessionInfo
    {
        int UndoCount { get; set; }
        int RedoCount { get; set; }
        bool HistoryReset { get; set; }
        Guid SelectedParamID { get; set; }
        Guid SelectedMeshID { get; set; }
        Guid SelectedKeyframeID { get; set; }
        bool TimelineVisibility { get; set; }
        int CurrentFrame { get; set; }
        Vector2 TexMin { get; set; }
        Vector2 TexSize { get; set; }
        EditModeInfo EditModeInfo { get; set; }
    }

    public class SessionInfo : ISessionInfo
    {
        public int UndoCount { get; set; }
        public int RedoCount { get; set; }
        public bool HistoryReset { get; set; }
        public Guid SelectedParamID { get; set; }
        public Guid SelectedMeshID { get; set; }
        public Guid SelectedKeyframeID { get; set; }
        public bool TimelineVisibility { get; set; }
        public int CurrentFrame { get; set; }
        public Vector2 TexMin { get; set; }
        public Vector2 TexSize { get; set; }
        public EditModeInfo EditModeInfo { get; set; }

        public SessionInfo()
        {
            UndoCount = 0;
            RedoCount = 0;
            HistoryReset = true;
            SelectedParamID = Guid.Empty;
            SelectedMeshID = Guid.Empty;
            SelectedKeyframeID = Guid.Empty;
            TimelineVisibility = false;
            CurrentFrame = 1;
            TexMin = Vector2.zero;
            TexSize = Vector2.zero;
            EditModeInfo = new();
        }
    }
}