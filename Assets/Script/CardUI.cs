using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardUI : MonoBehaviour
{
    public TMP_Text nameText, descText, costText;
    public Button button;
    CardInstance instance;

    public void Setup(CardInstance card, Action<CardInstance> onClick)
    {
        instance = card;
        nameText.text = card.data.cardName;
        descText.text = card.data.description;
        costText.text = card.data.cost.ToString();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick(instance));
    }
}