using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(BoxCollider))]
public class ScrollHandler : MonoBehaviour
{
    [SerializeField] private RectTransform ViewportScaledTransform;

    private float ViewportScale
    {
        get
        {
            return ScaleManager.instance.CurrentScale;
        }
        set
        {
            ScaleManager.instance.CurrentScale = value;
        }
    }

    private BoxCollider boxCollider;
    private InputAction pointPosition;
    private InputAction scrollAction;
    private InputAction primaryPosition, secondaryPosition;
    private Camera cam;
    private float lastDistance = 0;
    private bool primaryDown = false, secondaryDown = false;

    void Start()
    {
        scrollAction = InputSystem.actions.FindAction("ScrollWheel");
        scrollAction.performed += OnScroll;

        pointPosition = InputSystem.actions.FindAction("Point");

        InputAction primaryContact = InputSystem.actions.FindAction("PrimaryContact");
        primaryContact.started += (ctx) => OnFingerDown(ctx, primaryPosition, ref primaryDown);
        primaryContact.performed += (ctx) => OnFingerUp(ctx, ref primaryDown);

        InputAction secondaryContact = InputSystem.actions.FindAction("SecondaryContact");
        secondaryContact.started += (ctx) => OnFingerDown(ctx, secondaryPosition, ref secondaryDown);
        secondaryContact.performed += (ctx) => OnFingerUp(ctx, ref secondaryDown);

        primaryPosition = InputSystem.actions.FindAction("PrimaryPosition");
        secondaryPosition = InputSystem.actions.FindAction("SecondaryPosition");

        cam = Camera.main;

        if (ViewportScaledTransform == null) Debug.LogError("No ViewportScaled is assigned.");
        boxCollider = GetComponent<BoxCollider>();
    }

    /// <summary>
    /// Zooms on the artmeshes when the mouse scrolls inside the viewport.
    /// </summary>
    public void OnScroll(InputAction.CallbackContext context)
    {
        Ray ray = cam.ScreenPointToRay(pointPosition.ReadValue<Vector2>());
        if (!boxCollider.Raycast(ray, out RaycastHit hit, 100f)) return;

        Vector3 mouseWorldPos = cam.ScreenToWorldPoint(pointPosition.ReadValue<Vector2>());
        mouseWorldPos.z = transform.position.z;

        Vector2 scrollValue = scrollAction.ReadValue<Vector2>();
        if (scrollValue.magnitude > 0)
        {
            Vector3 localPoint = ViewportScaledTransform.InverseTransformPoint(mouseWorldPos);
            Vector3 worldBefore = ViewportScaledTransform.TransformPoint(localPoint);

            ViewportScale += ScaleManager.instance.ScaleSpeed * scrollValue.y * ViewportScale;

            Vector3 worldAfter = ViewportScaledTransform.TransformPoint(localPoint);
            ViewportScaledTransform.position += worldBefore - worldAfter;
        }
    }

    void OnFingerDown(InputAction.CallbackContext context, InputAction fingerPosition, ref bool fingerDown)
    {
        Ray ray = cam.ScreenPointToRay(fingerPosition.ReadValue<Vector2>());
        if (boxCollider.Raycast(ray, out RaycastHit hit, 100f))
        {
            fingerDown = true;
        }
    }

    void OnFingerUp(InputAction.CallbackContext context, ref bool fingerDown)
    {
        fingerDown = false;
        lastDistance = 0;
    }

    /// <summary>
    /// Zooming interaction on mobile devices using multi touch and drag.
    /// </summary>
    void OnMultitouchZoom()
    {
        Vector2 pos0 = primaryPosition.ReadValue<Vector2>();
        Vector2 pos1 = secondaryPosition.ReadValue<Vector2>();

        float currentDistance = Vector2.Distance(pos0, pos1);

        // check previous distance to determine zoom direction
        if (lastDistance > 0)
        {
            Vector3 touchWorldPos = cam.ScreenToWorldPoint((pos0 + pos1) * 0.5f);
            touchWorldPos.z = transform.position.z;
            Vector3 localPoint = ViewportScaledTransform.InverseTransformPoint(touchWorldPos);
            Vector3 worldBefore = ViewportScaledTransform.TransformPoint(localPoint);

            float difference = currentDistance - lastDistance;

            if (Math.Abs(difference) < 1f) return;

            // zooming out
            if (difference < 0f)
            {
                float scaleAmount = 1f / (float)Math.Sqrt(Math.Abs(difference));
                if (scaleAmount < 1f - ScaleManager.instance.ScaleSpeed)
                    scaleAmount = 1f - ScaleManager.instance.ScaleSpeed;
                ViewportScale *= scaleAmount;
            }
            else
            {
                ViewportScale *= 1f + ((float)Math.Sqrt(difference) * 0.01f);
            }

            Vector3 worldAfter = ViewportScaledTransform.TransformPoint(localPoint);
            ViewportScaledTransform.position += worldBefore - worldAfter;
        }

        lastDistance = currentDistance;
    }

    void Update()
    {
        if (primaryDown && secondaryDown)
        {
            OnMultitouchZoom();
        }
    }

}
