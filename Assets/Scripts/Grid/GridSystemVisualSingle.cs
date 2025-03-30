using UnityEngine;

/// <summary>
/// The class object is used to populated the GridSystemVisual and represents a single highlighted tile on the player view.
/// The texture of the tile is contained in a Mesh Renderer that is edited through the Unity engine in the GridSystemVisual prefab.
/// </summary>
public class GridSystemVisualSingle : MonoBehaviour
{
    [SerializeField]private MeshRenderer meshRenderer;

    public void Show(Material material)
    {
        meshRenderer.enabled = true;
        meshRenderer.material = material;
    }

    public void Hide()
    {
        meshRenderer.enabled = false;
    }
}
