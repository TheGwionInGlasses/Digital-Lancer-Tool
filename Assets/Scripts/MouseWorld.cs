using UnityEngine;

/// <summary>
/// This Class <c>MouseWorld</c> is used to manage interactions with the mouse and the MousePlane.
/// </summary>
public class MouseWorld : MonoBehaviour
{
    private static MouseWorld instance;

    [SerializeField] private LayerMask mousePlaneLayerMask;

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        // Physics based function. Requires to be pointed at an object with a collider.
        transform.position = MouseWorld.GetPosition();
    }

    /// <summary>
    /// This method is used to get the position of the mouse cursor. It fires a raycast from the screen to the mouse
    /// and collides with objects on the mousePlaneLayerMask.
    /// </summary>
    /// <returns>A fine position(Vector3) in the Unity scene where the mouse interacts with the level</returns>
    public static Vector3 GetPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, instance.mousePlaneLayerMask);
        return raycastHit.point;
    }
}
