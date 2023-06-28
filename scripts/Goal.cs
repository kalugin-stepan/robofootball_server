using Godot;

public partial class Goal : Area3D {
	public delegate void OnGoal(Team team);
	public static event OnGoal OnGoalEvent;
	Team team;
	public override void _Ready() {
		team = (Team)GetMeta("team").AsInt32();
		BodyEntered += OnEnter;
	}
	void OnEnter(Node3D body) {
		if (body is Ball) OnGoalEvent(team);
	}
}
