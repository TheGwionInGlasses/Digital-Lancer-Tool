using UnityEngine;
using TMPro;

/// <summary>
/// This class <c>PathfindingGridDebugObject</c> is used to provide an overlay over every pathnode in the unity scene for debugging
/// </summary>
public class PathfindingGridDebugObject : GridDebugObject
{
    [SerializeField] private TextMeshPro gCostText;
    [SerializeField] private TextMeshPro hCostText;
    [SerializeField] private TextMeshPro fCostText;
    [SerializeField] private SpriteRenderer isWalkableSpriteRenderer;

    private PathNode pathNode;

    public override void SetGridObject(object gridObject)
    {
        base.SetGridObject(gridObject);
        pathNode = (PathNode)gridObject;
        
    }

    /// <summary>
    /// Placing this in Update ensures that the overlay is updated everytime a change is made in the scene.
    /// </summary>
    protected override void Update()
    {
        base.Update();
        gCostText.text = pathNode.GetGCost().ToString();
        hCostText.text = pathNode.GetHCost().ToString();
        fCostText.text = pathNode.GetFCost().ToString();
        isWalkableSpriteRenderer.color = pathNode.IsWalkable() ? Color.green : Color.red;
    }
}
