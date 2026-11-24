using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardUI : MonoBehaviour
{
    public TMP_Text nameText, descText, costText;
    public TMP_Text levelText; // Optional field for level number badge
    public Button button;
    CardInstance instance;

    public void Setup(CardInstance card, Action<CardInstance> onClick)
    {
        instance = card;
        nameText.text = card.data.cardName;
        descText.text = card.GetDescription();
        costText.text = card.data.cost.ToString();

        // Level badge handling (shows 1, 2, 3...)
        if (levelText != null)
        {
            levelText.text = card.Level.ToString();
        }
        else
        {
            // Auto-detect any child text named "Level", "LevelText", or "LevelBadge"
            TMP_Text foundLevel = transform.Find("Level")?.GetComponent<TMP_Text>() 
                               ?? transform.Find("LevelText")?.GetComponent<TMP_Text>()
                               ?? transform.Find("LevelBadge")?.GetComponent<TMP_Text>();
            if (foundLevel != null)
            {
                foundLevel.text = card.Level.ToString();
            }
            else if (card.Level > 1)
            {
                // Fallback: append level number to card name if no dedicated badge text exists
                nameText.text = $"{card.data.cardName} (Lv.{card.Level})";
            }
        }

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClick?.Invoke(instance));
        }

        if (card.data.effect == CardEffectType.Junk && nameText != null)
            nameText.color = Color.gray;
    }
}
