using Assets.Scripts.States;

public class TimelineViewModel : ViewModelBase<TimelineState, TimelineState>
{
    protected override TimelineState Project(TimelineState domain)
    {
        return domain;
    }
}
