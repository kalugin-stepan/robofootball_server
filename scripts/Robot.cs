using System;
using Godot;

public partial class Robot : RigidBody3D {
	public int id;
	public string name;
	public Teams team;
	public Vector2 input = new Vector2(0, 0);
	const float airFriction = 1;
	const float friction = 100;
	const float wheelAcceleration = 60;
	float R;
	float leftSpeed = 0;
	float rightSpeed = 0;
	// DateTime lastPingTime = DateTime.Now;
	double T = 0;
	double lastInputTime = 0;
	public override void _Ready() {
		Name = GetMultiplayerAuthority().ToString();
		R = CenterOfMass.DistanceTo(GetNode<Node3D>("wheel_l").Position);
	}
	public override void _IntegrateForces(PhysicsDirectBodyState3D state) {
		float dt = state.Step;
		var aim = GlobalTransform.Basis;

		float leftAcceleration = input.X * wheelAcceleration;
		float rightAcceleration = input.Y * wheelAcceleration;

		if (Mathf.Sign(leftAcceleration) != Mathf.Sign(leftSpeed)) {
			if (Mathf.Abs(leftSpeed) > friction * dt) {
				leftSpeed -= Mathf.Sign(leftSpeed) * friction * dt;;
			}
			else leftSpeed = 0;
		}
		if (Mathf.Sign(rightAcceleration) != Mathf.Sign(rightSpeed)) {
			if (Mathf.Abs(rightSpeed) > friction * dt) {
				rightSpeed -= Mathf.Sign(rightSpeed) * friction * dt;;
			}
			else rightSpeed = 0;
		}

		leftSpeed += leftAcceleration * dt;
		rightSpeed += rightAcceleration * dt;

		leftSpeed -= Mathf.Sign(leftSpeed) * leftSpeed * leftSpeed * airFriction * dt;
		rightSpeed -= Mathf.Sign(rightSpeed) * rightSpeed * rightSpeed * airFriction * dt;

		LinearVelocity = (leftSpeed + rightSpeed) * aim.X;

		var av = AngularVelocity;
		av.Y = (rightSpeed - leftSpeed) / R;
		AngularVelocity = av;
	}
	public override void _Process(double delta) {
		T += delta;
		if (T - lastInputTime > 0.02) {
			input = Vector2.Zero;
		}
	}
	public void UpdateInput(Vector2 input) {
		this.input = input;
		lastInputTime = T;
	}
}
