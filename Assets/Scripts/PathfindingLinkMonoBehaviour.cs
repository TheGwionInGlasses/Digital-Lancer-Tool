using UnityEngine;

/// <summary>
/// This class <c>PathfindingLinkMonoBehaviour</c> models the relationship between the pathfinding link object in the logic and in the unity scenes
/// </summary>
public class PathfindingLinkMonoBehaviour : MonoBehaviour
{
    public Vector3 linkPositionA;
    public Vector3 linkPositionB;

    /// <summary>
    /// This method is used to the return the pathfindingLink game object as a pathfinding link object used in the Pathfinding script
    /// </summary>
    /// <returns>A pathfinding link object</returns>
    public PathfindingLink GetPathfindingLink()
    {
        return new PathfindingLink{
            gridPositionA = LevelGrid.Instance.GetGridPosition(linkPositionA),
            gridPositionB = LevelGrid.Instance.GetGridPosition(linkPositionB)
        };
    }
}
