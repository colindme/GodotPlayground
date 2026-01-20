using Godot;
using System;

namespace Solitaire
{
	public partial class SolitaireGlobals : Node
	{
		public static SolitaireGlobals Instance { get; private set; }

        public override void _Ready()
        {
           	Instance = this;
			GetViewport().PhysicsObjectPickingSort = true;
			GetViewport().PhysicsObjectPickingFirstOnly = true;
        }

	}
}
