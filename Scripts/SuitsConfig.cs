using Godot;
using System;

[Tool]
[GlobalClass]
public partial class SuitsConfig : Resource
{
    [Export]
    public Godot.Collections.Dictionary<Suit, Color> Suits;
}
