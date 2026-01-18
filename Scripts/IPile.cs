using System.Collections.Generic;
using System.Dynamic;
using Godot;

public interface IPile
{
    public abstract LinkedList<CardInfo> Pile { get; protected set; } 

    public static void Move(IPile source, IPile destination, List<CardInfo> cardInfo, bool reverseCards = false)
    {
        GD.Print($"CardInfoLength: {cardInfo.Count}");
        if (cardInfo == null) return;
        source.RemoveFromPile(cardInfo.Count);

        if (reverseCards)
        {
            cardInfo.Reverse();
        }

        destination.AddToPile(cardInfo);
    }

    public void RemoveFromPile(int count)
    {
		if (count > Pile.Count)
		{
			GD.PrintErr("RemoveFromPile called with count value higher than Pile.Count");
			return;
		}
		for (int i = 0; i < count; i++)
		{
			Pile.RemoveFirst();
		}

		UpdateVisuals();
    }

    public void AddToPile(List<CardInfo> cardInfo)
    {
        for (int i = 0; i < cardInfo.Count; i++)
		{
			Pile.AddFirst(cardInfo[i]);
		}

		UpdateVisuals();
    }

    public void UpdateVisuals();
}