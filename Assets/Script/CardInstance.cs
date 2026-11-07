public class CardInstance
{
    static int counter = 0;
    public readonly int instanceId;
    public readonly CardData data;
    public CardInstance(CardData data) { this.data = data; instanceId = ++counter; }
}