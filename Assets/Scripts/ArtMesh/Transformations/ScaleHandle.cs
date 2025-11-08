using UnityEngine;
using UnityEngine.InputSystem;

public class ScaleHandle : DraggableHandle
{
    [SerializeField] private Transform Corner;
    private Vector3 originalOffset;
    private Vector3 cornerOffset;
    private Vector3 originalScale;
    private Vector3 offsetRatio;

    protected override void OnClickStarted(InputAction.CallbackContext context)
    {
        if (IsInsideCollider())
        {
            cornerOffset = Corner.position - ClickWorldPosition;
            originalOffset = ParentTransform.position - Corner.position;
            dragging = true;

            originalScale = ParentTransform.gameObject.GetComponent<MeshController>().ArtMeshObject.transform.localScale;
        }
    }

    protected override void OnDragFinished(InputAction.CallbackContext context)
    {
        if (dragging)
        {
            ParentTransform.gameObject.GetComponent<MeshController>().SaveTransform(TransformType.SCALE);
        }
        dragging = false;
    }

    void Update()
    {
        if (dragging)
        {
            Corner.position = ClickWorldPosition + cornerOffset;

            Vector3 originalOffsetLocal = ParentTransform.InverseTransformVector(originalOffset);
            Vector3 currentOffsetLocal = ParentTransform.InverseTransformVector(ParentTransform.position - Corner.position);

            offsetRatio = new Vector3(
                currentOffsetLocal.x / originalOffsetLocal.x,
                currentOffsetLocal.y / originalOffsetLocal.y,
                1f
            );

            Vector3 newScale = Vector3.Scale(originalScale, offsetRatio);
            ParentTransform.gameObject.GetComponent<MeshController>().UpdateTransform(newScale, TransformType.SCALE);
        }
    }
}
