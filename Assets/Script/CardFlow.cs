using System.Collections.Generic;

public class CardFlow
{
    public List<CardInstance> deck = new List<CardInstance>();
    public List<CardInstance> hand = new List<CardInstance>();
    public List<CardInstance> discard = new List<CardInstance>();
    static System.Random rng = new System.Random();

    public void StartBattle(List<string> acquiredIds, int openingDraw)
    {
        deck.Clear(); hand.Clear(); discard.Clear();           // wipe old test data
        foreach (string id in acquiredIds)                      // acquired list is NOT changed
            deck.Add(new CardInstance(CardDatabase.Get(id)));
        Shuffle(deck);
        Draw(openingDraw);
    }

    public void Draw(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (deck.Count == 0)
            {
                if (discard.Count == 0) return;                 // nothing left: stop
                deck.AddRange(discard); discard.Clear(); Shuffle(deck);
            }
            CardInstance top = deck[deck.Count - 1];
            deck.RemoveAt(deck.Count - 1);
            hand.Add(top);
        }
    }

    public bool Discard(CardInstance card)
    {
        if (!hand.Remove(card)) return false;                   // blocks double-discard
        discard.Add(card);
        return true;
    }

    void Shuffle(List<CardInstance> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            CardInstance t = list[i]; list[i] = list[j]; list[j] = t;
        }
    }
}