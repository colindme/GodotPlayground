using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static Solitaire.GlobalMoveSystem;

namespace Solitaire
{
    [GlobalClass]
    public partial class NewDeck : Pile
    {
        [Export] Node2D TestingNode;

        [Export] public ScryPile ScryPile { get; private set; }
		[Export] public Godot.Collections.Array<PlaySpot> PlaySpots { get; private set; }
        [Export] public Area2D DeckArea { get; private set; }
        [Export] public Sprite2D DeckSprite { get; private set; }
		
		[ExportCategory("Deck Depth Settings")]
		[Export] public Sprite2D DeckDepthSprite { get; private set; }
		[Export] public float DeckDepthCardScalar { get; private set; } =  0.02f;

        private const int fullDeckCount = 52;
		private bool _mouseHeld = false;
        private MoveAnimation _testAnimation;

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
					tempDeck[index] = new Card()
					{
						Suit = suit,
						Value = value	
					};
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

            if (@event.IsActionPressed("test_z"))
            {
                GD.Print("Test_Z Pressed");
                // Testing MoveAnimation
                if (_testAnimation == null)
                {
                    TweenInfo t = TweenInfo.CreateTweenInfo(TestingNode, "position", 0.5, TestingNode.Position, new Vector2(100, 100));
                    TweenInfo x = TweenInfo.CreateTweenInfo(TestingNode, "scale", 0.0, Vector2.One, new Vector2(2, 2));
                    _testAnimation = new MoveAnimation(new List<TweenInfo>() { t, x });
                }
            }

            if (@event.IsActionPressed("test_x"))
            {
                GD.Print("Test_X Pressed");
                if (_testAnimation != null && _testAnimation.Finished)
                {
                    _testAnimation.Reverse();
                }
            }
        }

        public override List<TweenInfo> CreateTweenInfoForMove(Pile source, List<Card> cardList)
        {
            throw new NotImplementedException();
        }
    }

/*
    public partial class TweenManager : Node
    {
        private Dictionary<ulong, Dictionary<string, TweenInfo>> _tweenDictionary;

        public TweenManager()
        {
            _tweenDictionary = new Dictionary<ulong, Dictionary<string, TweenInfo>>();
        }

        public void CreateTween(Node node, string property, double duration, Variant startVariant, Variant endVariant)
        {
            Tween t = node.CreateTween();
            t.TweenProperty(node, "position", new Vector2(0, 0), 0.5);
        }

        public void ReverseTween(Node node, string property)
        {
            if (node == null || _tweenDictionary == null) return;
            ulong id = node.GetInstanceId();
            if (_tweenDictionary.TryGetValue(id, out Dictionary<string, TweenInfo> nodeTweensDict))
            {
                if (nodeTweensDict.TryGetValue(property, out TweenInfo entry))
                {
                    ulong timeDiff = Time.GetTicksMsec() - entry.StartTimeTicks;
                }
            }
        }
    }
    */
}

// NEXT UP: 
// ANIMATING FROM THE DECK 
// Modifying the move structure and animations

// Animation list
// - Start of game to piles (NOT A MOVE)
// - Deck to scry pile (MOVE)
// take (UP TO) cards to scry from deck and move them to scry pile
// make any cards that are currently not at the top position of the scry pile move up
// - Scry pile to deck (MOVE)
// - Scry pile to final pile (MOVE)
// - Scry pile to play spot (MOVE)
// - Play spot to play spot (MOVE)
// - Play spot to final spot (MOVE)
// - Final spot to play spot (MOVE)
// - End of game all cards to final piles (NOT A MOVE)
// - End of game back to deck (NOT A MOVE)

// Moves will have a List<TweenEntry>
// Pile.Source