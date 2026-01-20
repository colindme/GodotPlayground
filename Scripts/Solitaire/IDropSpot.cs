using Godot;
using System;

namespace Solitaire
{
	public interface IDropSpot
	{
		public bool TryDrop(Card droppedCard);
	}
}