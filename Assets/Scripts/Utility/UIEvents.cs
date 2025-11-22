using System;

public static class UIEvents
{
    /// <summary>
    /// Event to notify when a parameter has been selected.
    /// Passes the parameter ID.
    /// </summary>
    public static event Action<Guid> ParameterSelectEvent;

    /// <summary>
    /// Event to notify when a layer has been deselected.
    /// </summary>
    public static event Action LayerDeselectEvent;

    /// <summary>
    /// Event to notify when a layer has been selected.
    /// Passes the mesh ID.
    /// </summary>
    public static event Action<Guid> LayerSelectEvent;

    /// <summary>
    /// Event to notify when a layer has been deleted.
    /// Passes the mesh ID.
    /// </summary>
    public static event Action<Guid> LayerDeleteEvent;

    /// <summary>
    /// Event to notify when the timeline frame has changed.
    /// Passes the current frame index.
    /// </summary>
    public static event Action<int> TimelineChangeEvent;

    /// <summary>
    /// Event to notify when a keyframe is selected.
    /// Passes the keyframe ID.
    /// </summary>
    public static event Action<Guid> SelectKeyframeEvent;

    /// <summary>
    /// Event to notify when the user wants to delete the selected keyframe.
    /// </summary>
    public static event Action DeleteSelectedKeyframeEvent;

    /// <summary>
    /// Event to notify when a parameter has been interpolated.
    /// Passes the parameter ID and the new interpolated value.
    /// </summary>
    public static event Action<Guid, float> ParamInterpolatedEvent;

    /// <summary>
    /// Event to notify when want to edit parameter info.
    /// Passes the parameter ID.
    /// </summary>
    public static event Action<Guid> EditParameterInfoEvent;

    /// <summary>
    /// Event when any of the timeline parameter sliders is changed.
    /// Passes the parameter ID and the new slider value.
    /// </summary>
    public static event Action<Guid, float> TimelineParameterSliderChanged;

    public static void RaiseParameterSelect(Guid id) => ParameterSelectEvent?.Invoke(id);
    public static void RaiseLayerDeselect() => LayerDeselectEvent?.Invoke();
    public static void RaiseLayerSelect(Guid id) => LayerSelectEvent?.Invoke(id);
    public static void RaiseLayerDelete(Guid id) => LayerDeleteEvent?.Invoke(id);
    public static void RaiseTimelineChange(int frame) => TimelineChangeEvent?.Invoke(frame);
    public static void RaiseSelectKeyframe(Guid id) => SelectKeyframeEvent?.Invoke(id);
    public static void RaiseDeleteSelectedKeyframe() => DeleteSelectedKeyframeEvent?.Invoke();
    public static void RaiseParamInterpolated(Guid id, float v) => ParamInterpolatedEvent?.Invoke(id, v);
    public static void RaiseEditParameterInfo(Guid id) => EditParameterInfoEvent?.Invoke(id);
    public static void RaiseTimelineParameterSliderChanged(Guid id, float v) => TimelineParameterSliderChanged?.Invoke(id, v);

}
