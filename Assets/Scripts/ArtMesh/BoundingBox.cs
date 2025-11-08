using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class BoundingBox : MonoBehaviour, ISelectable
{
    [SerializeField] private GameObject Center;
    [SerializeField] private GameObject Corners;

    private void Awake()
    {
        if (Center == null || Corners == null) Debug.LogError("Not all fields have been assigned.");
    }

    /// <summary>
    /// Create a bounding box using LineRenderer around a center point.
    /// </summary>
    /// <param name="size">length of the rectangle's sides</param>
    public void CreateBoundingBox(Vector3 center, Vector3 size)
    {
        LineRenderer lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.positionCount = 4;
        Vector3[] corners = {
            new(center.x - size.x / 2, center.y + size.y / 2 ),
            new(center.x + size.x / 2, center.y + size.y / 2 ),
            new(center.x + size.x / 2, center.y - size.y / 2 ),
            new(center.x - size.x / 2, center.y - size.y / 2 ),
        };
        lineRenderer.SetPositions(corners);
        Center.transform.localPosition = center;

        Corners.transform.localPosition = lineRenderer.GetPosition(1);

        SetClipArea();
    }

    /// <summary>
    /// Set the clipping area of all bounding box components. 
    /// The components won't be draw outside the rectangle specified 
    /// by its top left and bottom right corners.
    /// </summary>
    public void SetClipArea()
    {
        ViewportManager viewportManager = ViewportManager.Instance;
        Vector3 topLeft = viewportManager.TopLeftAnchor;
        Vector3 bottomRight = viewportManager.BottomRightAnchor;

        Material lineMaterial = GetComponent<LineRenderer>().material;

        if (lineMaterial != null)
        {
            SetClipInShader(lineMaterial, topLeft, bottomRight);
        }

        Material movementHandleMaterial = Center.GetComponentInChildren<SpriteRenderer>().material;

        if (movementHandleMaterial != null)
        {
            SetClipInShader(movementHandleMaterial, topLeft, bottomRight);
        }

        Material cornerHandleMaterial = Corners.GetComponentInChildren<SpriteRenderer>().material;

        if (cornerHandleMaterial != null)
        {
            SetClipInShader(cornerHandleMaterial, topLeft, bottomRight);
        }
    }

    private void SetClipInShader(Material material, Vector3 topLeft, Vector3 bottomRight)
    {
        material.SetVector("_TopLeftAnchor", topLeft);
        material.SetVector("_BottomRightAnchor", bottomRight);
        material.renderQueue = 3000;
    }

    public void SetSelected(bool isSelected)
    {
        GetComponentInChildren<LineRenderer>().enabled = isSelected;
        Center.SetActive(isSelected);
        Corners.SetActive(isSelected);
    }
}
