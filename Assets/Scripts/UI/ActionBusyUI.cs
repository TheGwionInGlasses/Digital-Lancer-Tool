using UnityEditor.Search;
using UnityEngine;

public class ActionBusyUI : MonoBehaviour
{
    [SerializeField] private GameObject actionBusyUI;

    private void Start()
    {
        UnitActionSystem.Instance.OnBusyChanged += UnitActionSystem_OnBusyChanged;
        Hide();
    }

    private void Show()
    {
        actionBusyUI.SetActive(true);
    }

    private void Hide()
    {
        actionBusyUI.SetActive(false);
    }
    private void UnitActionSystem_OnBusyChanged(object sender, bool isBusy)
    {
        if (isBusy)
        {
            Show();
        } else {
            Hide();
        }
    }
}
