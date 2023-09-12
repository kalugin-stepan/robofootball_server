using Godot;

public partial class Goal : Area3D {
	public delegate void OnGoal(Teams team);
	public static event OnGoal OnGoalEvent;
	Teams team;
	public override void _Ready() {
		team = (Teams)GetMeta("team").AsInt32();
		BodyEntered += OnEnter;
	}
	void OnEnter(Node3D body) {
		if (body is Ball) OnGoalEvent(team);
	}
}
