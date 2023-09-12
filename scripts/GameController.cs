using Godot;
using Godot.Collections;
using System;
using System.Text;
using System.Text.Json;

public partial class GameController : Node {
	static int team1Score = 0;
	static int team2Score = 0;
	const float a = 10; // side of the sqare
	static readonly float k = a * MathF.Sqrt(2) / 2; // distance from center to angle
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
	readonly PackedScene robotScene = ResourceLoader.Load<PackedScene>("res://scenes/Robot.tscn");
	HttpRequest request; 
	MQTT mqtt;
	string games_server_url;
	Ball ball;
	Array<Robot> players = new Array<Robot>();
	Message message = new Message();
	BallMessage ballMessage = new BallMessage();
	public override void _Ready() {
		mqtt = GetParent().GetNode<MQTT>("MQTT");
		mqtt.BrokerConnected += BrokerConnected;
		mqtt.ReceivedMessage += OnInput;

		var configFile = new ConfigFile();
		configFile.Load("res://config.cfg");
		games_server_url = configFile.GetValue("config", "games_server_url").AsString();
		var game_name = configFile.GetValue("config", "game_name").AsString();

		request = GetParent().GetNode<HttpRequest>("request");
		request.RequestCompleted += OnStart;
		request.Request($"{games_server_url}/get_game?name={game_name}");
	}

	void OnStart(long res, long code, string[] headers, byte[] data) {
		Game.Init(Json.ParseString(Encoding.UTF8.GetString(data)).AsGodotDictionary<string, Variant>());
		mqtt.ConnectToBroker(Game.server_url);
		AddBall();
 	}
	void BrokerConnected() {
		foreach (var player in Game.team1.players) {
			AddRobot(player.id, player.name, Teams.team1);
		}
		foreach (var player in Game.team2.players) {
			AddRobot(player.id, player.name, Teams.team2);
		}
	}
	void AddRobot(int id, string name, Teams team) {
		var robot = robotScene.Instantiate<Robot>();
		robot.id = id;
		robot.name = name;
		robot.team = team;
		GetParent().AddChild(robot);
		if (team == Teams.team1) {
			robot.GlobalPosition = initialPositionsTeam1[players.Count];
			robot.GlobalRotation = new Vector3(0, teamsInitialRotations[0], 0);
		}
		else if (team == Teams.team2) {
			robot.GlobalPosition = initialPositionsTeam2[players.Count - Game.team1.players.Count];
			robot.GlobalRotation = new Vector3(0, teamsInitialRotations[1], 0);
		}
		var marker = new Message.Marker();
		marker.marker_id = id;
		var (pos0, pos1, pos2, pos3) = CalculateSquare(robot.GlobalPosition, robot.GlobalRotation.Y);
		marker.corners.UpdateCorners(pos0, pos1, pos2, pos3);
		message.markers.Add(marker);
		players.Add(robot);
		mqtt.Subscribe($"{Game.game_name}/{name}");
	}
	void AddBall() {
		var ballScene = ResourceLoader.Load<PackedScene>("res://scenes/Ball.tscn");
		ball = ballScene.Instantiate<Ball>();
		GetParent().AddChild(ball);
		ball.GlobalPosition = ballInitialPosition;
		ballMessage.ball.Add(new BallMessage.Center(ballInitialPosition.X , ballInitialPosition.Z));
	}
	(Vector2, Vector2, Vector2, Vector2) CalculateSquare(Vector3 pos, float angle) {
		Vector2 pos0 = new Vector2();
		Vector2 pos1 = new Vector2();
		Vector2 pos2 = new Vector2();
		Vector2 pos3 = new Vector2();
		pos0.X = pos.X + MathF.Cos(angle + MathF.PI/4 + MathF.PI/2) * k;
		pos0.Y = pos.Z + MathF.Sin(angle + MathF.PI/4 + MathF.PI/2) * k;
		pos1.X = pos.X + MathF.Cos(angle - MathF.PI/4 + MathF.PI/2) * k;
		pos1.Y = pos.Z + MathF.Sin(angle - MathF.PI/4 + MathF.PI/2) * k;
		pos2.X = pos.X + MathF.Cos(angle - 3*MathF.PI/4 + MathF.PI/2) * k;
		pos2.Y = pos.Z + MathF.Sin(angle - 3*MathF.PI/4 + MathF.PI/2) * k;
		pos3.X = pos.X + MathF.Cos(angle + 3*MathF.PI/4 + MathF.PI/2) * k;
		pos3.Y = pos.Z + MathF.Sin(angle + 3*MathF.PI/4 + MathF.PI/2) * k;
		return (pos0, pos1, pos2, pos3);
	}
	double t = 0;
	public override void _PhysicsProcess(double delta) {
		t += delta;
		if (t > 0.02 && mqtt.Connected) {
			t = 0;
			for (int i = 0; i < players.Count; i++) {
				var player = players[i];
				var marker = message.markers[i];
				var (pos0, pos1, pos2, pos3) = CalculateSquare(player.GlobalPosition, player.GlobalRotation.Y);
				marker.corners.UpdateCorners(pos0, pos1, pos2, pos3);
			}
			mqtt.Publish(Game.game_name, JsonSerializer.Serialize(message));
			ballMessage.ball[0].x = ball.GlobalPosition.X;
			ballMessage.ball[0].y = ball.GlobalPosition.Z;
			mqtt.Publish($"{Game.game_name}/ball", JsonSerializer.Serialize(ballMessage));
		}
	}
	void OnInput(string topic, byte[] data) {
		string robot_name = topic.Split('/')[1];
		string cmd = Encoding.UTF8.GetString(data);
		foreach (var player in players) {
			if (player.name == robot_name) {
				if (cmd == CommandsConfig.fCmd) {
					player.UpdateInput(new Vector2(1, 1));
				}
				else if (cmd == CommandsConfig.bCmd) {
					player.UpdateInput(new Vector2(-1, -1));
				}
				else if (cmd == CommandsConfig.lCmd) {
					player.UpdateInput(new Vector2(0, 1));
				}
				else if (cmd == CommandsConfig.rCmd) {
					player.UpdateInput(new Vector2(1, 0));
				}
				break;
			}
		}
	}
}
