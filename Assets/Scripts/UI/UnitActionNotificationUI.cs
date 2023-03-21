using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UnitActionNotificationUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI actionText;
    
    private void Start() 
    {
        UnitActionSystem.Instance.OnBusyChanged += UnitActionSystem_OnBusyChanged;
        Hide();
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void UnitActionSystem_OnBusyChanged(object sender, bool isBusy)
    {
        if (isBusy)
        {
            actionText.text = UnitActionSystem.Instance.GetSelectedAction().GetActionName();
            Show();
        }
        else
        {
            Hide();
        }
    }
}
