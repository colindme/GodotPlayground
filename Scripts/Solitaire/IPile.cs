using System.Collections.Generic;
using Godot;
namespace Solitaire
{
    public interface IPile
    {
        /*
        public LinkedList<CardInfo> Pile { get; } 
        public Vector2 ChildOffset { get; }
		public Zone Zone { get; set; }

        public static void Move(IPile source, IPile destination, List<CardInfo> cardInfoList, bool reverseCards = false)
        {
            if (cardInfoList == null) return;
            source.RemoveFromPile(cardInfoList.Count);

            if (reverseCards)
            {
                cardInfoList.Reverse();
            }

            destination.AddToPile(cardInfoList);
        }

        /// <summary>
        /// Attempts to search a pile for a specific CardInfo. This search is conducted from the front of the pile.
        /// </summary>
        /// <param name="cardInfo">The exact card info to search a pile for</param>
        /// <returns>A list of cards, starting with the desired card. Null if not found</returns>
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
                cardInfos.Reverse();
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

        public CardInfo? PopFrontCard()
        {
            CardInfo? result = null;
            if (Pile.Count != 1)
            {
                result = Pile.First.Value;
                RemoveFromPile(1);
            }

            return result;
        }

        public List<LinkedListNode<CardInfo>> AddToPile(List<CardInfo> cardInfo)
        {
            List<LinkedListNode<CardInfo>> newNodes = new List<LinkedListNode<CardInfo>>();
            for (int i = 0; i < cardInfo.Count; i++)
            {
                newNodes.Add(Pile.AddFirst(cardInfo[i]));
            }

            newNodes.Reverse();
            return newNodes;
            //UpdateVisuals();
        }

        public LinkedListNode<CardInfo> AddToPile(CardInfo cardInfo)
        {
            return Pile.AddFirst(cardInfo);
        }

        // Adds a card visual to the pile
        public void AddCardVisual(Card card, LinkedListNode<CardInfo> node)
        {
            card.PileParent = this;
			card.Zone = Zone;
        }

        public void UpdateVisuals();

        public void ClearPile(Vector2 visualClearPosition, float visualClearTime)
        {
            Pile.Clear();

            
        }
        */
    }

    public enum Zone
	{
		NONE,
		SCRY,
		PLAY,
		FINAL,
        DECK,
	}

    // SHOULD IPILE BECOME A NEW BASE CLASS?
    // IPile inherits from Node2D
    // ScryPile, instead of marker2D is just an IPile
    // PlaySpot
    // FinalSpot
    // Deck
}