using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Solitaire
{
    [GlobalClass]
    public partial class NewDeck : Pile
    {
        [Export] public PackedScene CardScene { get; private set;}
        [Export] public NewScryPile ScryPile { get; private set; }
		[Export] public Godot.Collections.Array<PlaySpot> PlaySpots { get; private set; }
        [Export] public Area2D DeckArea { get; private set; }
        [Export] public Sprite2D DeckSprite { get; private set; }
        [Export] public double MoveToDeckAnimTime { get; private set; }
        [Export] public double IndividCardDelayScalar { get; private set; }
		
		[ExportCategory("Deck Depth Settings")]
		[Export] public Sprite2D DeckDepthSprite { get; private set; }
		[Export] public float DeckDepthCardScalar { get; private set; } =  0.02f;

        private const int fullDeckCount = 52;
		private bool _mouseHeld = false;

        public override void _Ready()
        {
            if (DeckArea == null)
            {
                DeckArea = GetNodeOrNull<Area2D>(nameof(DeckArea));
            }

            if (DeckArea != null)
            {
                DeckArea.InputPickable = true;
            }
            else
            {
                GD.PrintErr($"{nameof(DeckArea)} was null and could not be found. Check Deck scene");
            }

            ResetDeck();
        }

        public void ResetDeck()
		{
			Card[] tempDeck = new Card[fullDeckCount];
			for (Suit suit = Suit.HEART; suit <= Suit.CLUB; suit++)
			{
				for (int value = (int)FaceValue.ACE; value <= (int)FaceValue.KING; value++)
				{
					int index = ((int)suit * (int)FaceValue.KING) + (value - 1);

                    // Do I need to set pile parent?
                    Card card = CardScene.Instantiate<Card>();
                    card.Value = value;
                    card.Suit = suit;   
                    card.PileParent = this;
					tempDeck[index] = card;
                    card.Visible = false;
                    AddChild(card);
				}
			}

			// TODO: Seed this?
			Random.Shared.Shuffle(tempDeck);
			if (Contents.Count != 0)
			{
				Contents.Clear();
			}

			AddToPile(tempDeck.ToList());
            UpdateVisuals();
		}

        public override void UpdateVisuals()
		{
			DeckDepthSprite.Scale = new Vector2(DeckDepthSprite.Scale.X, 1 + (DeckDepthCardScalar * Contents.Count));
			DeckSprite.Visible = Contents.Count != 0;
			DeckDepthSprite.Visible = Contents.Count != 0;
		}

        public override void _Input(InputEvent @event)
        {
            if (SolitaireGlobals.Instance.CurrentlyHeldCard != null) return;
            if (@event is InputEventMouseButton mouseButtonEvent)
			{                    
				if (mouseButtonEvent.ButtonIndex == MouseButton.Left)
				{
                    bool overDeck = false;
                    // TBH This might not be the best way to do this vs just using the Area2D input event but I'm doing it for now :P
                    foreach (var objs in GetWorld2D().DirectSpaceState.IntersectPoint(new PhysicsPointQueryParameters2D()
                    {
                        Position = mouseButtonEvent.Position,
                        CollideWithAreas = true
                    }))
                    {
                        if (objs["rid"].As<Rid>() == DeckArea.GetRid())
                        {
                            overDeck = true;
                        }
                    }

                    if (overDeck && mouseButtonEvent.Pressed)
                    {
                        _mouseHeld = true;
                        GetViewport().SetInputAsHandled();
                    }
                    else if (overDeck && mouseButtonEvent.IsReleased() && _mouseHeld)
                    {
                        _mouseHeld = false;

                        if (Contents.Count == 0)
						{
							GlobalMoveSystem.Move move = new GlobalMoveSystem.Move()
                            {
                                Destination = this,
                                Source = ScryPile,
                                CardList = ScryPile.Contents.ToList()
                            };

							GlobalMoveSystem.Instance.ExecuteMove(move);
						}
						else
						{
							GlobalMoveSystem.Move move = new GlobalMoveSystem.Move()
                            {
                                Source = this,
                                Destination = ScryPile,
                                ReverseOnUndo = true
                            };

							int cardsToGrab = Mathf.Min(ScryPile.CardsToScry, Contents.Count);
							List<Card> cards = new List<Card>();
							LinkedListNode<Card> currNode = null;
							for (int i = 0; i < cardsToGrab; i++)
							{
								currNode = currNode == null ? Contents.First : currNode.Next;
								cards.Add(currNode.Value);
							}
							move.CardList = cards;

							GlobalMoveSystem.Instance.ExecuteMove(move);
                            UpdateVisuals();
						}

                        GetViewport().SetInputAsHandled();
                    }                    
				}
			}

            if (@event.IsActionPressed("ui_left"))
			{
				GD.Print("Deck contents:");
				LinkedListNode<Card> node = null;
				for (int i = 0; i < Contents.Count; i++)
				{
					node = node == null ? Contents.First : node.Next;
					GD.Print($"Order: {i  + 1} | Card: {node.Value.Value} of {node.Value.Suit}");
				}
			}
        }

        // TODO: Need to make a call to update deck visuals at the end of the anim?
        public override List<TweenInfo> CreateTweenInfoForMove(Pile source, List<Card> cardList)
        {
            List<TweenInfo> result = new List<TweenInfo>();
            int visibleCount = 0;
            if (source is NewScryPile sp)
            {
                visibleCount = Math.Min(sp.CardsToShow, cardList.Count);
            }

            for (int i = 0; i < cardList.Count; i++)
            {
                int targetZIndex = visibleCount - 1 - i;
                // if target is below 0, indicates the card is already hidden -> move to deck immediately
                if (targetZIndex < 0)
                {
                    result.Add(TweenInfo.CreateTweenInfo(cardList[i], "position", 0, 0, source.Position, Position));
                }
                else
                {
                    result.Add(TweenInfo.CreateTweenInfo(cardList[i], "position", MoveToDeckAnimTime, 0, source.Position + source.ChildOffset * (cardList.Count - 1 - i), Position));
                    // Sets z index to 0 at the end of the anim
                    result.Add(TweenInfo.CreateTweenInfo(cardList[i], "z_index", 0, MoveToDeckAnimTime, targetZIndex, 0));
                    result.Add(TweenInfo.CreateTweenInfo(cardList[i], "visible", 0, MoveToDeckAnimTime, true, false));
                }
            }

            return result;
        }

        public override List<StateChange> CreateStateChangeForMove(List<Card> cardList)
        {
            List<StateChange> result = new List<StateChange>();
            foreach(Card card in cardList)
            {
                result.Add(StateChange.CreateStateChange(card, "IsFlippedOver", false, true));
            }

            return result;
        }
    }
}

/*
List<TweenInfo> result = new List<TweenInfo>();
            
            int amountFromEndNotHidden = 0;
            if (source is NewScryPile sp)
            {
                amountFromEndNotHidden = Math.Min(sp.CardsToShow, cardList.Count);
            }

            int zIndexOffset = amountFromEndNotHidden - cardList.Count;
            double fullAnimTime = (cardList.Count - 1) * IndividCardDelayScalar + MoveToDeckAnimTime;
            for (int i = 0; i < cardList.Count; i++)
            {
                double delay = IndividCardDelayScalar * i;
                bool shouldStartHidden = i < cardList.Count - amountFromEndNotHidden;
                List<TweenAction> visibilityTweenActions =
                [
                    new ActionActive(StateChange.CreateStateChange(cardList[i], "visible", shouldStartHidden, true), 0),
                    new ActionDelay(fullAnimTime),
                    new ActionActive(StateChange.CreateStateChange(cardList[i], "visible", true, false), 0),
                ];
                result.Add(TweenInfo.CreateTweenInfo(cardList[i], "visible", visibilityTweenActions));

                List<TweenAction> zIndexTweenActions =
                [
                    new ActionActive(StateChange.CreateStateChange(cardList[i], "z_index", Math.Min(0, i + zIndexOffset), i + zIndexOffset), 0),
                    new ActionDelay(fullAnimTime),
                    new ActionActive(StateChange.CreateStateChange(cardList[i], "z_index", i + zIndexOffset, 0), 0),
                ];
                result.Add(TweenInfo.CreateTweenInfo(cardList[i], "z_index", zIndexTweenActions));

                result.Add(TweenInfo.CreateTweenInfo(cardList[i], "position", MoveToDeckAnimTime, delay, cardList[i].Position, Position));
            }

            return result;
*/

// NEXT UP: 
// ANIMATING FROM THE DECK 
// Modifying the move structure and animations

// Animation list
// - Start of game to piles (NOT A MOVE)

// - Deck to scry pile (MOVE) -> DONE

// Undo Deck to scry pile -> Not tested

// - Scry pile to deck (MOVE) -> ALMOST DONE
// todo: update deck visuals at the end of the anim

// Undo -> Not tested

// - Scry pile to final pile (MOVE)
// First check it's valid move
// Move single card to final spot
// Update visibility for final spot top card and 2nd to top card
// Scry pile move back into place... hmmm this is tough
// On undo -> Scry pile readjusts...
// Undo

// - Scry pile to play spot (MOVE)
// First check it's valid move
// Move single card to play spot
// Scry pile move back into place... hmmm this is tough
// On undo -> Scry pile readjusts...
// Undo

// - Play spot to play spot (MOVE)
// First check it's valid move
// move cards to play spot
// potentially reveal hidden card
// undo -> hide hidden card
// Undo

// - Play spot to final spot (MOVE)
// Undo

// - Final spot to play spot (MOVE)
// Undo

// - End of game all cards to final piles (NOT A MOVE)

// - End of game back to deck (NOT A MOVE) -> ALMOST DONE - not tested