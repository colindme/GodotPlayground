using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Solitaire
{
    [GlobalClass]
    public partial class NewScryPile : Pile
    {
        [Export]
		public int CardsToScry
		{
			get => _cardsToScry;
			set
			{
				GD.Print($"Cards to Scry: {value}");
				_cardsToScry = value;
				_cardsToShow = Mathf.Max(_cardsToScry, MinCardsToShow);
				//Callable.From(InitializeCards).CallDeferred();
			}
		}

		[Export] public int MinCardsToShow { get; private set; }

        private int _cardsToScry;
		private int _cardsToShow;
        private float _cardAnimTime;
        private const int _moveZIndex = 10;

        public override List<TweenInfo> CreateTweenInfoForMove(Pile source, List<Card> cardList)
        {
            List<TweenInfo> tweens = new List<TweenInfo>();
            
            List<Card> visibleList = new List<Card>();
            LinkedListNode<Card> pileNode = Contents.First;
            while (pileNode != null && pileNode.Value.Visible)
            {
                visibleList.Add(pileNode.Value);
                pileNode = pileNode.Next;
            }

            int amountToShift = visibleList.Count + cardList.Count - _cardsToShow;
            // Move Old Cards
            for (int i = 0; i < visibleList.Count; i++)
            {
                int currentOffset = visibleList.Count - 1 - i;
                bool willBecomeInvisible = (currentOffset - amountToShift) < 0;
                int targetOffset = Math.Max(0, currentOffset - amountToShift);
                tweens.Add(TweenInfo.CreateTweenInfo(visibleList[i], "position", _cardAnimTime, 0, Position + ChildOffset * currentOffset, Position + ChildOffset * targetOffset));
                tweens.Add(TweenInfo.CreateTweenInfo(visibleList[i], "z_index", 0, _cardAnimTime, currentOffset, targetOffset));
                if (willBecomeInvisible)
                {
                    tweens.Add(TweenInfo.CreateTweenInfo(visibleList[i], "visible", 0, _cardAnimTime, true, false));
                }
            }

            // Move new cards
            int startingNewOffset = Math.Min(visibleList.Count, _cardsToShow - cardList.Count);
            for (int i = 0; i < cardList.Count; i++)
            {
                tweens.Add(TweenInfo.CreateTweenInfo(cardList[i], "position", _cardAnimTime, 0, source.Position, Position + ChildOffset * (i + startingNewOffset)));
                // zindex -> Make this a sequence? (Could require a rewrite)
                tweens.Add(TweenInfo.CreateTweenInfo(cardList[i], "visible", 0, 0, false, true));
            }

            return tweens;
        }

        public override List<StateChange> CreateStateChangeForMove(List<Card> cardList)
        {
            List<StateChange> result = new List<StateChange>();
            foreach (Card card in cardList)
            {
                result.Add(StateChange.CreateStateChange(card, "IsFlippedOver", true, false));
            }

            return result;
        }

        public override void UpdateVisuals()
        {
            throw new NotImplementedException();
        }
    }
}
/*
    Current problems:
    ZIndex for moving scale
    Sliding duration based on distance
        Basically the duration of the animation should be determined by the distance
        so MoveAnimation should also allow for a function to be passed in?
        err or TweenInfo?
*/