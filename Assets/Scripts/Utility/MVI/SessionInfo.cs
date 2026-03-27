namespace Assets.Scripts.Utility.MVI
{
    public interface ISessionInfo
    {
        int UndoCount { get; set; }
        int RedoCount { get; set; }
        bool HistoryReset { get; set; }
    }

    public class SessionInfo : ISessionInfo
    {
        public int UndoCount { get; set; }
        public int RedoCount { get; set; }
        public bool HistoryReset { get; set; }

        public SessionInfo()
        {
            UndoCount = 0;
            RedoCount = 0;
            HistoryReset = true;
        }
    }
}