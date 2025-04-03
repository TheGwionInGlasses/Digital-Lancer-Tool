using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// A simple testing script
/// </summary>
public class Testing : MonoBehaviour
{

    [SerializeField] private Unit unit;
    [SerializeField]private GridSystemVisual gridSystemVisual;
    private void Start()
    {

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            GridPosition testGridPosition = LevelGrid.Instance.GetGridPosition(MouseWorld.GetPosition());
            CubeGridPosition testCubeGridPosition = LevelGrid.Instance.OffsetToCube(testGridPosition);
            GridPosition retestGridPosition = LevelGrid.Instance.CubeToOffset(testCubeGridPosition);
            Debug.Log(testGridPosition);
            Debug.Log(testCubeGridPosition);
            Debug.Log(retestGridPosition);
        }
    }
}
