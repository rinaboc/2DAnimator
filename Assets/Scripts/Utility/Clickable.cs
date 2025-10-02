using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
abstract public class Clickable : MonoBehaviour
{
    protected Camera cam;
    protected InputAction pointPositionAction;
    protected Collider[] boxCollider;

    protected virtual void Start()
    {
        cam = Camera.main;
        pointPositionAction = InputSystem.actions.FindAction("Point");

        boxCollider = GetComponents<Collider>();
    }

    protected bool IsInsideCollider()
    {
        Ray ray = cam.ScreenPointToRay(pointPositionAction.ReadValue<Vector2>());
        foreach (var col in boxCollider)
        {
            if (col.Raycast(ray, out RaycastHit hit, 100f))
            {
                return true;
            }
        }
        ;

        return false;
    }

}
