using Godot;
using Godot.Collections;

public partial class Main : Node3D {
	int team1Score = 0;
	int team2Score = 0;

	static readonly Vector3[] initialPositionsTeam1 = {
		new Vector3(-5, 0.194f, 0),
		new Vector3(-5, 0.194f, 20),
		new Vector3(-5, 0.194f, -20)
	};
	static readonly Vector3[] initialPositionsTeam2 = {
		new Vector3(5, 0.194f, 0),
		new Vector3(5, 0.194f, 20),
		new Vector3(5, 0.194f, -20)
	};
	static readonly float[] teamsInitialRotations = {
		0,
		Mathf.Pi
	};
	static readonly Vector3 ballInitialPosition = new Vector3(0, 5, 0);
	const int port = 9999;
	readonly Array<Dictionary<string, Variant>> team1 = new Array<Dictionary<string, Variant>>();
	readonly Array<Dictionary<string, Variant>> team2 = new Array<Dictionary<string, Variant>>();
	readonly ENetMultiplayerPeer multiplayerPeer = new ENetMultiplayerPeer();
	readonly PackedScene playerScene = ResourceLoader.Load<PackedScene>("res://scenes/Robot.tscn");
	Ball ball;
	public override void _Ready() {
		multiplayerPeer.CreateServer(port);
		Multiplayer.MultiplayerPeer = multiplayerPeer;
		multiplayerPeer.PeerConnected += OnPeerConnection;
		multiplayerPeer.PeerDisconnected += OnPeerDisconnection;
		var ballScene = ResourceLoader.Load<PackedScene>("res://scenes/Ball.tscn");
		ball = ballScene.Instantiate<Ball>();
		ball.SetMultiplayerAuthority(multiplayerPeer.GetUniqueId());
		AddChild(ball);
		ball.GlobalPosition = ballInitialPosition;
		Goal.OnGoalEvent += OnGoal;
	}
	async void OnPeerConnection(long id) {
		await ToSignal(GetTree().CreateTimer(0.01), "timeout");
		RpcId(id, nameof(SetTeamsData), team1, team2);
	}
	void OnPeerDisconnection(long id) {
		bool removed = false;
		for (int i = 0; i < team1.Count; i++) {
			if (team1[i]["id"].AsInt64() == id) {
				team1.RemoveAt(i);
				removed = true;
				break;
			}
		}
		if (!removed) {
			for (int i = 0; i < team2.Count; i++) {
				if (team2[i]["id"].AsInt64() == id) {
					team2.RemoveAt(i);
					break;
				}
			}
		}
		RemoveChild(GetNode(id.ToString()));
		Rpc(nameof(OnPlayerDisconnection), id);
	}
	void AddRobotCharacter(long id, Vector3 initPos, float initRotation) {
		var robot = playerScene.Instantiate() as Robot;
		robot.SetMultiplayerAuthority((int)id);
		AddChild(robot);
		robot.GlobalPosition = initPos;
		robot.Rotation = new Vector3(0, initRotation, 0);
	}
	void OnGoal(Team team) {
		if (team == Team.team1) {
			team1Score++;
		}
		else if (team == Team.team2) {
			team2Score++;
		}
		ball.GlobalPosition = ballInitialPosition;
		ball.LinearVelocity = Vector3.Zero;
		ball.AngularVelocity = Vector3.Zero;
		
		for (int i = 0; i < team1.Count; i++) {
			var robot = GetNode<Robot>(team1[i]["id"].ToString());
			robot.GlobalPosition = initialPositionsTeam1[i];
			robot.Rotation = new Vector3(0, teamsInitialRotations[0], 0);
		}
		for (int i = 0; i < team2.Count; i++) {
			var robot = GetNode<Robot>(team2[i]["id"].ToString());
			robot.GlobalPosition = initialPositionsTeam2[i];
			robot.Rotation = new Vector3(0, teamsInitialRotations[1], 0);
		}
		Rpc(nameof(UpdateScore), team1Score, team2Score);
	}
	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	void JoinGame(string username, int team) {
		long id = Multiplayer.GetRemoteSenderId();
		var player = new Dictionary<string, Variant>();
		player["id"] = id;
		player["username"] = username;
		if ((Team)team == Team.team1) {
			Rpc(nameof(OnPlayerConnection), id, username, team, team1.Count);
			AddRobotCharacter(id, initialPositionsTeam1[team1.Count], teamsInitialRotations[0]);
			team1.Add(player);
			return;
		}
		if ((Team)team == Team.team2) {
			Rpc(nameof(OnPlayerConnection), id, username, team, team2.Count);	
			AddRobotCharacter(id, initialPositionsTeam2[team2.Count], teamsInitialRotations[1]);
			team2.Add(player);
			return;
		}
	}
	[Rpc]
	void SetTeamsData(Array<Dictionary<string, Variant>> team1, Array<Dictionary<string, Variant>> team2) {}
	[Rpc]
	void OnPlayerConnection(long id, string username, int team, int num) {}
	[Rpc]
	void OnPlayerDisconnection(long id) {}
	[Rpc]
	void UpdateScore(int team1Score, int team2Score) {}
}
