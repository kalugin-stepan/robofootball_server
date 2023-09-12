using System.Collections.Generic;
using Godot;

public static class Game {
	public static string game_name;
	public static string server_url;
	public static Team team1 = new Team();
	public static Team team2 = new Team();
	public class Team {
		public string name;
		public List<Player> players = new List<Player>();
	}
	public class Player {
		public int id;
		public string name;
		public Player(int id, string name) {
			this.id = id;
			this.name = name;
		}
	}
	public static void Init(Godot.Collections.Dictionary<string, Variant> game) {
		game_name = game["name"].AsString();
		server_url = game["server_url"].AsString();
		var t1 = game["team1"].AsGodotDictionary<string, Variant>();
		var t2 = game["team2"].AsGodotDictionary<string, Variant>();
		team1.name = t1["name"].AsString();
		team2.name = t2["name"].AsString();
		var t1_players = t1["players"].AsGodotArray<Godot.Collections.Dictionary<string, Variant>>();
		var t2_players = t2["players"].AsGodotArray<Godot.Collections.Dictionary<string, Variant>>();
		foreach (var player in t1_players) {
			team1.players.Add(new Player(player["id"].AsInt32(), player["name"].AsString()));
		}
		foreach (var player in t2_players) {
			team2.players.Add(new Player(player["id"].AsInt32(), player["name"].AsString()));
		}
	}
}
