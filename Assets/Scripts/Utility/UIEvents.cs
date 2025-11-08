using System;
using UnityEngine;
using UnityEngine.Events;

public static class UIEvents
{
    /// <summary>
    /// Event to notify when the timeline frame has changed.
    /// Passes the current frame index.
    /// </summary>
    public static UnityEvent<int> TimelineChangeEvent = new();

    /// <summary>
    /// Event to notify when a keyframe is selected.
    /// Passes the keyframe ID.
    /// </summary>
    public static UnityEvent<Guid> SelectKeyframeEvent = new();

    /// <summary>
    /// Event to notify when the user wants to delete the selected keyframe.
    /// </summary>
    public static UnityEvent DeleteSelectedKeyframeEvent = new();

    /// <summary>
    /// Event to notify when a parameter has been interpolated.
    /// Passes the parameter ID and the new interpolated value.
    /// </summary>
    public static UnityEvent<Guid, float> ParamInterpolatedEvent = new();

    /// <summary>
    /// Event to notify when want to edit parameter info.
    /// Passes the parameter ID.
    /// </summary>
    public static UnityEvent<Guid> EditParameterInfoEvent = new();

    /// <summary>
    /// Event when any of the timeline parameter sliders is changed.
    /// Passes the parameter ID and the new slider value.
    /// </summary>
    public static UnityEvent<Guid, float> TimelineParameterSliderChanged = new();
}
