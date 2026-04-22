using System;

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
        bool IsEditMode { get; set; }

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
        public bool IsEditMode { get; set; }

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
            IsEditMode = false;
        }
    }
}