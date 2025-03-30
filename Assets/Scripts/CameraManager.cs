using System;
using UnityEngine;

/// <summary>
/// This class <c>CameraManager</c> models the behaviour of the compliment named Unity object in the scenes.
/// It coodinates which camera objects the Cinemachine Virtual Camera follow.
/// </summary>
public class CameraManager : MonoBehaviour
{
    [SerializeField] private GameObject actionCameraObject;

    /// <summary>
    /// This method subscribes to events in the base action script which alert when aactions of interest activate and finish.
    /// </summary>
    private void Start()
    {
           BaseAction.OnAnyActionStarted += BaseAction_OnAnyActionStarted;
           BaseAction.OnAnyActionCompleted += BaseAction_OnAnyActionCompleted;
    }

    /// <summary>
    /// The Action Caamera Object is a higher priority than the Level camera, therefore when its activate the Cinemachine Virtual Camera follows this object.
    /// </summary>
    private void ShowActionCamera()
    {
        actionCameraObject.SetActive(true);
    }

    /// <summary>
    /// When the action is completed, the actionCamera unity object is deactivated so that normal viewning of the game can resume.
    /// </summary>
    private void HideActionCamera()
    {
        actionCameraObject.SetActive(false);
    }


    /// <summary>
    /// This method is subscribed to every action and provides the logic for handling the action camera on an actions activation.
    /// </summary>
    /// <param name="sender">The action component firing the event.</param>
    /// <param name="e">Any arguments included in the event.</param>
    private void BaseAction_OnAnyActionStarted(object sender, EventArgs e)
    {
        switch(sender)
        {
            case ShootAction shootAction:
                // If the action is a shoot action, adjust the position of the action camera to be over the agent's shoulder and face the target.

                Unit shooterUnit = shootAction.GetUnit();
                Unit targetUnit = shootAction.GetTargetUnit();

                Vector3 cameraCharacterHeight = Vector3.up * 1.7f;

                Vector3 shootDir = (targetUnit.GetWorldPosition() - shooterUnit.GetWorldPosition()).normalized;

                float shoulderOffsetAmount = 0.5f;
                Vector3 shoulderOffset = Quaternion.Euler(0, 90, 0) * shootDir * shoulderOffsetAmount;

                Vector3 actionCameraPosition = shooterUnit.GetWorldPosition() + cameraCharacterHeight + shoulderOffset + (shootDir * -1);

                actionCameraObject.transform.position = actionCameraPosition;
                actionCameraObject.transform.LookAt(targetUnit.GetWorldPosition() + cameraCharacterHeight);
                
                ShowActionCamera();
                break;
            case SpinAction:
                break;
            case GrenadeAction:
                break;
            case MoveAction:
                break;
        }
    }

    /// <summary>
    /// This method is subscribed to the completion of every action component and handles the cleaning up of the action camera after the action is completed.
    /// </summary>
    /// <param name="sender">The action object that fired the event</param>
    /// <param name="e">Any arguments passed along with the action</param>
    private void BaseAction_OnAnyActionCompleted(object sender, EventArgs e)
    {
        HideActionCamera();
    }
}
