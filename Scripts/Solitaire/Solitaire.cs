using Godot;
using System;
using System.Threading;
using System.Collections.Generic;

namespace Solitaire
{
	public partial class Solitaire : Node2D
	{
		[Export] private NewDeck _deck;
		[Export] private Godot.Collections.Array<NewPlaySpot> _playSpots;
		[Export] private PackedScene _cardScene;
		[Export] private float _cardMovementAnimationTime = 0.75f;
		[Export] private float _cardLayerDelay = 0.05f;

		private bool gameRestarting = false;

        public override void _Ready()
        {
            ResetGame();
        }

		public async void ResetGame()
		{
			if (_deck == null)
			{
				GD.PrintErr($"Failed to reset game. Deck was not set in Solitaire");
				return;
			}

			if (gameRestarting)
			{
				return;
			}

			gameRestarting = true;

			// CLEAR AREAS

			// RESET DECK
			_deck.ResetDeck();

			// RESET SCORE
			
			// DEAL CARDS
			List<TweenInfo> cardDealingAnimation = new List<TweenInfo>();
			for (int i = 0 ; i < _playSpots.Count; i++)
			{
				bool first = true;
				for (int j = i; j < _playSpots.Count; j++)
				{
					Pile pile = _playSpots[j];
					Card card = _deck.PopFrontCard();
					if (card == null)
					{
						GD.PrintErr($"Failed to reset game. Tried to pop a card off the front of deck but it returned null");
						return;
					}

					AddChild(card);
					card.IsFlippedOver = !first;
					card.Zone = Zone.PLAY;

					pile.AddToPile(card);
					Vector2 endingPos =  _playSpots[j].GlobalPosition + i * _playSpots[j].ChildOffset;
					cardDealingAnimation.Add(TweenInfo.CreateTweenInfo(card, "position", _cardMovementAnimationTime, ((i * _playSpots.Count) + j) * _cardLayerDelay, _deck.Position, endingPos));
					
					first = false;
				}
			}

			GlobalMoveSystem.MoveAnimation animation = new GlobalMoveSystem.MoveAnimation(cardDealingAnimation, null);
			animation.PlayFromStart();

			gameRestarting = false;
		}
	}
}
