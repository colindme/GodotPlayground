using Godot;
using System;

namespace Solitaire
{
	public partial class SolitaireGlobals : Node
	{
		public static SolitaireGlobals Instance { get; private set; }

		public Card CurrentlyHeldCard { get; set; }
		public const int MoveZIndex = 10;

        public override void _Ready()
        {
           	Instance = this;
			GetViewport().PhysicsObjectPickingSort = true;
			GetViewport().PhysicsObjectPickingFirstOnly = true;
        }
	}
}
