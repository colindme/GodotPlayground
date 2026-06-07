using Godot;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace Solitaire
{
	public partial class Solitaire : Node2D
	{
		[Export] private NewDeck _deck;
		[Export] private Godot.Collections.Array<NewPlaySpot> _playSpots;
		[Export] private PackedScene _cardScene;
		[Export] private float _cardMovementAnimationTime = 0.75f;
		[Export] private double _cardLayerDelay = 0.05;

		private bool gameRestarting = false;

        public override void _Ready()
        {
            ResetGame();
        }

        public override void _Input(InputEvent @event)
        {
            if (@event.IsActionPressed("test_z"))
			{
				Godot.Collections.Array<Godot.Collections.Dictionary> objectsUnderClick = GetWorld2D().DirectSpaceState.IntersectPoint(
					new PhysicsPointQueryParameters2D()
					{
						Position = GetViewport().GetMousePosition(),
						CollideWithAreas = true
					}
				);

				Card card = objectsUnderClick
					.Select(o => o["collider"].AsGodotObject() as Card)
					.Where(o => o != null)
					.OrderByDescending(node => node.ZIndex)
    				.FirstOrDefault();
					
				if (card != null)
				{
					GD.Print($"{card} : {card.PileParent.PileData.GetCardCountUnderCard(card)}");
					GD.Print(card.PileParent.PileData.GetIndexForCard(card));
				}

				GetViewport().SetInputAsHandled();
			}
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
			List<StateChange> cardDealingStateChange = new List<StateChange>();
			for (int i = 0 ; i < _playSpots.Count; i++)
			{
				bool first = true;
				for (int j = i; j < _playSpots.Count; j++)
				{
					GD.Print((i * _playSpots.Count) + j);
					IPile pile = _playSpots[j];
					Card card = _deck.PileData.PopFrontCard();
					if (card == null)
					{
						GD.PrintErr($"Failed to reset game. Tried to pop a card off the front of deck but it returned null");
						return;
					}

					card.IsFlippedOver = !first;
					card.IsDraggable = first;
					card.Zone = Zone.PLAY;

					pile.PileData.AddToPile(card);
					Vector2 endingPos =  _playSpots[j].GlobalPosition + i * _playSpots[j].ChildOffset;
					double delay = ((i * _playSpots.Count) + j) * _cardLayerDelay;
					cardDealingAnimation.Add(TweenInfo.CreateTweenInfo(card, "position", _cardMovementAnimationTime, delay, _deck.Position, endingPos));
					cardDealingAnimation.Add(TweenInfo.CreateTweenInfo(card, "visible", 0, delay, false, true));
					cardDealingStateChange.Add(StateChange.CreateStateChange(card, "z_index", 0, i));
					
					first = false;
				}
			}

			GlobalMoveSystem.MoveAnimation animation = new GlobalMoveSystem.MoveAnimation(cardDealingAnimation, cardDealingStateChange);
			animation.PlayFromStart();
			
			gameRestarting = false;
		}
	}
}
