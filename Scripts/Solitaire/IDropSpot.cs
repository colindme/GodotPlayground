using Godot;
using System;

namespace Solitaire
{
	public interface IDropSpot
	{
		public Vector2 ChildOffset { get; }

		public void TryDrop(Card droppedCard);
	}
}