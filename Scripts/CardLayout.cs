using Godot;
using System;

[Tool]
[GlobalClass]
public partial class CardLayout : Resource
{
    [Export]
    public Godot.Collections.Array<Vector2> PipPositions;

    [Export]
    public SpriteFrames SpriteFramesOverride;
}
