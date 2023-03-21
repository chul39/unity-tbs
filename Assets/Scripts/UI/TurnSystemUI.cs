using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TurnSystemUI : MonoBehaviour
{

    [SerializeField] private Button endTurnButton;
    [SerializeField] private TextMeshProUGUI turnNumberText;
    [SerializeField] private GameObject enermyTurnVisualGameObject;

    private void Start()
    {
        endTurnButton.onClick.AddListener(() => {
            TurnSystem.Instance.NextTurn();
        });
        UnitActionSystem.Instance.OnBusyChanged += UnitActionSystem_OnBusyChanged;
        TurnSystem.Instance.OnTurnChanged += TurnSystem_OnTurnChanged;
        UpdateTurnText();
        UpdateEnemyTurnVisual();
    }

    private void UnitActionSystem_OnBusyChanged(object sender, bool isBusy)
    {
        UpdateEndTurnButtonActive(!isBusy);
    }


    private void TurnSystem_OnTurnChanged(object sender, EventArgs e)
    {
        UpdateTurnText();
        UpdateEnemyTurnVisual();
        UpdateEndTurnButtonActive(TurnSystem.Instance.IsPlayerTurn());
    }

    private void UpdateTurnText()
    {
        turnNumberText.text = $"TURN {TurnSystem.Instance.GetTurnNumber()}";
    }

    private void UpdateEndTurnButtonActive(bool isActive)
    {
        endTurnButton.gameObject.SetActive(isActive);
    }

    private void UpdateEnemyTurnVisual()
    {
        enermyTurnVisualGameObject.SetActive(!TurnSystem.Instance.IsPlayerTurn());
    }

}
