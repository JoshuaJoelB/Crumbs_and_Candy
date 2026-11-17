using System.Collections.Generic;

public class CardFlow
{
    public List<CardInstance> deck    = new List<CardInstance>();
    public List<CardInstance> hand    = new List<CardInstance>();
    public List<CardInstance> discard = new List<CardInstance>();
    static System.Random rng = new System.Random();

    public void StartBattle(List<string> acquiredIds, int openingDraw)
    {
<<<<<<< HEAD:Assets/Script/CardFlow.cs
        deck.Clear(); hand.Clear(); discard.Clear();           
        foreach (string id in acquiredIds)                      
=======
        deck.Clear(); hand.Clear(); discard.Clear();
        foreach (string id in acquiredIds)
>>>>>>> 96c19ab4c6152bd1848ff7cc66b07217454bb857:Assets/Script/Cards/CardFlow.cs
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
<<<<<<< HEAD:Assets/Script/CardFlow.cs
                if (discard.Count == 0) return;                 
=======
                if (discard.Count == 0) return;
>>>>>>> 96c19ab4c6152bd1848ff7cc66b07217454bb857:Assets/Script/Cards/CardFlow.cs
                deck.AddRange(discard); discard.Clear(); Shuffle(deck);
            }
            CardInstance top = deck[deck.Count - 1];
            deck.RemoveAt(deck.Count - 1);
            hand.Add(top);
        }
    }

    public bool Discard(CardInstance card)
    {
<<<<<<< HEAD:Assets/Script/CardFlow.cs
        if (!hand.Remove(card)) return false;                   
=======
        if (!hand.Remove(card)) return false;
>>>>>>> 96c19ab4c6152bd1848ff7cc66b07217454bb857:Assets/Script/Cards/CardFlow.cs
        discard.Add(card);
        return true;
    }

    public void AddJunk(int count)
    {
        CardData junk = CardDatabase.Get("JUNK-001");
        for (int i = 0; i < count; i++)
            deck.Add(new CardInstance(junk));
        Shuffle(deck);
    }

    public int PurgeJunkFromHand()
    {
        int removed = 0;
        for (int i = hand.Count - 1; i >= 0; i--)
        {
            if (hand[i].data.effect == CardEffectType.Junk)
            {
                discard.Add(hand[i]);
                hand.RemoveAt(i);
                removed++;
            }
        }
        return removed;
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