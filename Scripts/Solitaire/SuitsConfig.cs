using Godot;
using System;

namespace Solitaire
{
    [Tool]
    [GlobalClass]
    public partial class SuitsConfig : Resource
    {
        [Export]
        public Godot.Collections.Dictionary<Suit, Color> Suits;
    }
}