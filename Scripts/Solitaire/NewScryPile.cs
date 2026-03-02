using Godot;
using System;
using System.Collections.Generic;

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

        public override List<TweenInfo> CreateTweenInfoForMove(Pile source, List<Card> cardList)
        {
            List<TweenInfo> tweens = new List<TweenInfo>();
            
            // Shift currently displayed cards up
            int cardsToShift = _cardsToShow - 1;
            int amountToShift = cardList.Count;
            for (int i = 0; i < cardsToShift; i++)
            {
                int currentOffset = cardsToShift - i;
                int newOffset = Math.Max(0, currentOffset - amountToShift);
                //tweens.Add(TweenEntry.CreateTween());
            }

            // Move the cards from the source to the destination
            for (int i = 0; i < cardList.Count; i++)
            {
                int offsetToTarget = _cardsToShow - 1 - i;
                //tweens.Add(TweenEntry.CreateTween());
            }

            return tweens;
        }

        public override void UpdateVisuals()
        {
            throw new NotImplementedException();
        }
    }
}