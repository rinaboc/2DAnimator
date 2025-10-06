using UnityEngine;
using UnityEngine.InputSystem;

public class ScaleHandle : DraggableHandle
{
    [SerializeField] private Transform Corner;
    private Vector3 originalOffset;
    private Vector3 cornerOffset;
    private Vector3 originalScale;

    protected override void OnClickStarted(InputAction.CallbackContext context)
    {
        if (IsInsideCollider())
        {
            cornerOffset = Corner.position - ClickWorldPosition;
            originalOffset = ParentTransform.position - Corner.position;
            dragging = true;

            originalScale = ParentTransform.gameObject.GetComponent<ArtMesh>().ArtMeshObject.transform.localScale;
        }
    }

    protected override void OnDragFinished(InputAction.CallbackContext context)
    {
        if (dragging)
        {
            ArtMesh artMesh = ParentTransform.gameObject.GetComponent<ArtMesh>();
            artMesh.UpdateTransform(artMesh.ArtMeshObject.transform.localScale, TransformType.SCALE);
        }
        dragging = false;
    }

    void Update()
    {
        if (dragging)
        {
            Corner.position = ClickWorldPosition + cornerOffset;

            Vector3 currentOffset = ParentTransform.position - Corner.position;

            Vector3 offsetRatio = new(
                currentOffset.x / originalOffset.x,
                currentOffset.y / originalOffset.y,
                1f
            );

            ParentTransform.gameObject.GetComponent<ArtMesh>().ScaleArtMesh(originalScale, offsetRatio);
        }
    }
}
