using System.Collections.Generic;
using UnityEngine;

public static class CardCollection
{
    public static List<string> GetOwnedCards()
    {
        List<string> cards = new List<string>();
        cards.Add("BASIC-001");
        cards.Add("BASIC-002");
        cards.Add("NOR-001");
        return cards;
    }

    public static int GetLevel(string id) => 1;
    public static void AddCard(string id) { Debug.Log("Added: " + id); }
}