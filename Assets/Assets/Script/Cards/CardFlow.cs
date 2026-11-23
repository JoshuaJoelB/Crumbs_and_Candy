using System.Collections.Generic;
using UnityEngine;

public class CardFlow
{
    public List<CardInstance> deck = new List<CardInstance>();
    public List<CardInstance> hand = new List<CardInstance>();
    public List<CardInstance> discard = new List<CardInstance>();
    
    static System.Random rng = new System.Random();

    public void StartBattleWithInstances(List<CardInstance> startingDeck, int openingDraw)
    {
        deck.Clear(); hand.Clear(); discard.Clear();
        deck.AddRange(startingDeck);
        Shuffle(deck);
        Draw(openingDraw);
    }

    public void Draw(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (deck.Count == 0)
            {
                if (discard.Count == 0) return;                 
                deck.AddRange(discard); discard.Clear(); Shuffle(deck);
            }
            CardInstance top = deck[deck.Count - 1];
            deck.RemoveAt(deck.Count - 1);
            hand.Add(top);
        }
    }

    public bool Discard(CardInstance card)
    {
        if (!hand.Remove(card)) return false;                   
        discard.Add(card);
        return true;
    }

    public void AddJunk(int count)
    {
        CardData junkData = CardDatabase.Get("JUNK-001");
        for (int i = 0; i < count; i++)
            deck.Add(new CardInstance(junkData));
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

    // BAGONG METHOD PARA SA 3-CARD LIMIT
    public void EnforceHandLimit(int limit)
    {
        while (hand.Count > limit)
        {
            int randomIndex = rng.Next(0, hand.Count);
            CardInstance cardToRemove = hand[randomIndex];
            hand.RemoveAt(randomIndex);
            deck.Add(cardToRemove); // IBALIK SA DECK, HINDI SA DISCARD
        }
        Shuffle(deck);
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