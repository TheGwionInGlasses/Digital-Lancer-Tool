using UnityEngine;
using Cinemachine;

/// <summary>
/// This class <c>ScreenShake</c> is a singleton used to create a screen shake effect on the player view.
/// </summary>
public class ScreenShake : MonoBehaviour
{
    public static ScreenShake Instance { get; private set; }
    private CinemachineImpulseSource cinemachineImpulseSource;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There's more than one ScreenShake! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;

        cinemachineImpulseSource = GetComponent<CinemachineImpulseSource>();
    }

    /// <summary>
    /// This method is used to shake the screen.
    /// </summary>
    /// <param name="intensity">This is the strength of the screen shake</param>
    public void Shake(float intensity = 1f)
    {
        cinemachineImpulseSource.GenerateImpulse(intensity);
    }
}
