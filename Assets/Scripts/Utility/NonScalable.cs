using UnityEngine;

public class NonScalable : MonoBehaviour
{
    void Start()
    {
        ViewportEvents.ScaleChangeEvent.AddListener(() =>
        {
            this.transform.localScale = Vector3.one * ScaleManager.OriginalScale / ScaleManager.Instance.CurrentScale;
        });
    }
}
