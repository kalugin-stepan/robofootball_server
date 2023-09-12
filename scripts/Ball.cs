using Godot;
using System;

public partial class Ball : RigidBody3D {
	const float friction = 1;
	public override void _Ready() {

	}
	public override void _PhysicsProcess(double delta) {
		float dt = (float)delta;
		LinearVelocity -= LinearVelocity.Normalized() * friction * dt;
	}
}
