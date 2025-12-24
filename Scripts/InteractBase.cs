using Godot;
using System;

public abstract partial class InteractBase : Area2D
{
	//[Export]
	//public CollisionObject2D Collision { get; set; }

    public override void _Ready()
	{
		// TODO: Consider switching to a global assert per this post: https://www.reddit.com/r/godot/comments/obxm0i/is_the_godot_script_assert_available_in_c_and_if/
		//if (Collision == null)
        //{
		//	GD.PrintErr("ERROR: Collision on InteractBase must be set");
		//	throw new ApplicationException("ERROR: Collision on InteractBase must be set");
        //}

		base._Ready();
		
		//Collision
    }
}
