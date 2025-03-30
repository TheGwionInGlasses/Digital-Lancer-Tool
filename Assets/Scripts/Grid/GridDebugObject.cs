using TMPro;
using UnityEngine;

/// <summary>
/// This is a debug object that can be overlayed the tiles in the grid system. The Pathnode debug object also inherits from this class.
/// </summary>
public class GridDebugObject : MonoBehaviour
{
    [SerializeField] private TextMeshPro textMeshPro;

    private object gridObject;

    public virtual void SetGridObject(object gridObject)
    {
        this.gridObject = gridObject;
    }

    protected virtual void Update()
    {
        textMeshPro.text = gridObject.ToString();
    }
}
