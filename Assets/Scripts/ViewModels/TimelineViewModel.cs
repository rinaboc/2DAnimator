using Assets.Scripts.States;

public class TimelineViewModel : ViewModelBase<ParameterTimelineState, TimelineState>
{
    protected override TimelineState Project(ParameterTimelineState domain)
    {
        return domain.Timeline;
    }
}
