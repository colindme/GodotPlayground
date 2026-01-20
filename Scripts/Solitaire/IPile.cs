using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Godot;
namespace Solitaire
{
    public interface IPile
    {
        public LinkedList<CardInfo> Pile { get; } 
        public Vector2 ChildOffset { get; }
		public Zone Zone { get; set; }

        public static void Move(IPile source, IPile destination, List<CardInfo> cardInfo, bool reverseCards = false)
        {
            if (cardInfo == null) return;
            source.RemoveFromPile(cardInfo.Count);

            if (reverseCards)
            {
                cardInfo.Reverse();
            }

            destination.AddToPile(cardInfo);
        }

        public List<CardInfo> SearchUntilCard(CardInfo cardInfo)
        {
            List<CardInfo> cardInfos = new List<CardInfo>();
            bool found = false;
            LinkedListNode<CardInfo> node = Pile.First;
            while (node != null)
            {
                cardInfos.Add(node.Value);
                if (node.Value.Value == cardInfo.Value
                    && node.Value.Suit == cardInfo.Suit)
                {
                    found = true;
                    break;
                }

                node = node.Next;
            }
            
            if (found)
            {
                return cardInfos;
            }
            return null;
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

    public enum Zone
	{
		NONE,
		SCRY,
		PLAY,
		FINAL,
        DECK,
	}
}