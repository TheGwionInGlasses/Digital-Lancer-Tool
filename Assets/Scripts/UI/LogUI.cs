using System;
using TMPro;
using UnityEngine;

/// <summary>
/// This class models the behaviour for the action log on the player view. I thought of two ways to keep the log up to date.
/// One was exposing event handlers in the actions with event args describing the details of the event. Another method was to
/// turn this class into a singleton. I've decided to use the latter since the former would expose a lot of the inner workings
/// of the object.
/// </summary>
public class LogUI : MonoBehaviour
{
    public static LogUI Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI logText;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There's more than one LogUI! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void UpdateLog(string logUpdate)
    {
        logText.text += logUpdate;
    }
}
