using UnityEngine;

public class NonScalable : MonoBehaviour
{
    void Start()
    {
        ScaleManager.instance.onScaleChange.AddListener(() =>
        {
            this.transform.localScale = Vector3.one * ScaleManager.OriginalScale / ScaleManager.instance.CurrentScale;
        });
    }
}
