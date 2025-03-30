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
           
        }
    }
}
