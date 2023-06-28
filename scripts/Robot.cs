using System;
using Godot;

public partial class Robot : RigidBody3D {
	Vector2 input;
	const float airFriction = 1;
	const float friction = 100;
	const float wheelAcceleration = 60;
	float R;
	float leftSpeed = 0;
	float rightSpeed = 0;
	DateTime lastPingTime = DateTime.Now;
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

		Rpc(nameof(UpdatePlayerData), Position, Rotation.Y);
	}
	public override void _Process(double delta) {
		var curTime = DateTime.Now;
		if ((curTime - lastPingTime).TotalSeconds > 4) {
			RpcId(GetMultiplayerAuthority(), nameof(Ping));
			lastPingTime = curTime;
		}
	}
	
	[Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
	void SetInput(Vector2 inp) {
		input = inp;
	}
	[Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
	void UpdatePlayerData(Vector3 pos, float rotation) {}
	[Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
	void Ping() {
		Rpc(nameof(RemoteUpdatePing), (DateTime.Now - lastPingTime).Milliseconds);
	}
	[Rpc]
	void RemoteUpdatePing(int ms) {}
}
