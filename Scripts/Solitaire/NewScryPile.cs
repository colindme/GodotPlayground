using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Solitaire
{
    [GlobalClass]
    public partial class NewScryPile : Node2D, IPile
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

        [Export] public Vector2 ChildOffset	{ get; private set; }
		[Export] public Zone Zone { get; private set; }
		[Export] public int MinCardsToShow { get; private set; }
        [Export] public float CardAnimTime { get; private set;}
        public int CardsToShow { get => _cardsToShow; }

        public PileData PileData { get; private set; }


        private int _cardsToScry;
		private int _cardsToShow;
        private const int _moveZIndex = 10;

        public override void _Ready()
        {
            PileData = new PileData();
            base._Ready();
        }

        public List<TweenInfo> CreateTweenInfoForMove(IPile source, List<Card> cardList)
        {
            List<TweenInfo> tweens = new List<TweenInfo>();
            List<Card> visibleList = new List<Card>();
            LinkedListNode<Card> pileNode = PileData.Contents.First;
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
                tweens.Add(TweenInfo.CreateTweenInfo(visibleList[i], "position", CardAnimTime, 0, Position + ChildOffset * currentOffset, Position + ChildOffset * targetOffset));
                tweens.Add(TweenInfo.CreateTweenInfo(visibleList[i], "z_index", 0, CardAnimTime, currentOffset, targetOffset));
                if (willBecomeInvisible)
                {
                    tweens.Add(TweenInfo.CreateTweenInfo(visibleList[i], "visible", 0, CardAnimTime, true, false));
                }
            }

            // Move new cards
            int startingNewOffset = Math.Min(visibleList.Count, _cardsToShow - cardList.Count);
            for (int i = 0; i < cardList.Count; i++)
            {
                int offset = i + startingNewOffset;
                tweens.Add(TweenInfo.CreateTweenInfo(cardList[i], "position", CardAnimTime, 0, source.Position, Position + ChildOffset * offset));
                List<TweenAction> zIndexActions = new List<TweenAction>()
                {
                  new ActionActive(StateChange.CreateStateChange(cardList[i], "z_index", 0, _moveZIndex + offset), 0),
                  new ActionDelay(CardAnimTime),
                  new ActionActive(StateChange.CreateStateChange(cardList[i], "z_index", _moveZIndex + offset, offset), 0)  
                };

                tweens.Add(TweenInfo.CreateTweenInfo(cardList[i], "z_index", zIndexActions));
                tweens.Add(TweenInfo.CreateTweenInfo(cardList[i], "visible", 0, 0, false, true));
            }

            return tweens;
        }

        public List<StateChange> CreateStateChangeForMove(List<Card> cardList)
        {
            List<StateChange> result = new List<StateChange>();
            foreach (Card card in cardList)
            {
                result.Add(StateChange.CreateStateChange(card, "IsFlippedOver", true, false));
            }

            return result;
        }

        public void UpdateVisuals()
        {
            throw new NotImplementedException();
        }
    }
}