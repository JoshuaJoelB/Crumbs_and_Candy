using UnityEngine;
using System.Collections.Generic;

public static class CardCollection
{
    public static int GetLevel(string cardId)
    {
        return PlayerPrefs.GetInt("card_level_" + cardId, 1);
    }

    public static void AddCard(string cardId)
    {
        if (!HasCard(cardId))
        {
            PlayerPrefs.SetInt("card_owned_" + cardId, 1);
            PlayerPrefs.SetInt("card_level_" + cardId, 1);
            PlayerPrefs.Save();
        }
    }

    public static bool HasCard(string cardId)
    {
        return PlayerPrefs.GetInt("card_owned_" + cardId, 0) == 1;
    }

    public static List<string> GetOwnedCards()
    {
        List<string> owned = new List<string>();
        foreach (string id in CardDatabase.AllIds())
        {
            if (HasCard(id)) owned.Add(id);
        }
        return owned;
    }
}