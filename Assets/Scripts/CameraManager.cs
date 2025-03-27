using System;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private GameObject actionCamerObject;

    private void Start()
    {
           BaseAction.OnAnyActionStarted += BaseAction_OnAnyActionStarted;
           BaseAction.OnAnyActionCompleted += BaseAction_OnAnyActionCompleted;
    }

    private void ShowActionCamera()
    {
        actionCamerObject.SetActive(true);
    }

    private void HideActionCamera()
    {
        actionCamerObject.SetActive(false);
    }

    private void BaseAction_OnAnyActionStarted(object sender, EventArgs e)
    {
        switch(sender)
        {
            case ShootAction shootAction:
                Unit shooterUnit = shootAction.GetUnit();
                Unit targetUnit = shootAction.GetTargetUnit();

                Vector3 cameraCharacterHeight = Vector3.up * 1.7f;

                Vector3 shootDir = (targetUnit.GetWorldPosition() - shooterUnit.GetWorldPosition()).normalized;

                float shoulderOffsetAmount = 0.5f;
                Vector3 shoulderOffset = Quaternion.Euler(0, 90, 0) * shootDir * shoulderOffsetAmount;

                Vector3 actionCameraPosition = shooterUnit.GetWorldPosition() + cameraCharacterHeight + shoulderOffset + (shootDir * -1);

                actionCamerObject.transform.position = actionCameraPosition;
                actionCamerObject.transform.LookAt(targetUnit.GetWorldPosition() + cameraCharacterHeight);
                
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

    private void BaseAction_OnAnyActionCompleted(object sender, EventArgs e)
    {
        HideActionCamera();
    }
}
