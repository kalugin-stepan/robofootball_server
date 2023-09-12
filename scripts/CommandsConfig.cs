using Godot;
using System;

public partial class CommandsConfig : Node {
	public static string fCmd;
	public static string bCmd;
	public static string lCmd;
	public static string rCmd;
	public override void _Ready() {
		ConfigFile config = new ConfigFile();
		config.Load("res://commands.cfg");
		fCmd = config.GetValue("commands", "f").AsString();
		bCmd = config.GetValue("commands", "b").AsString();
		lCmd = config.GetValue("commands", "l").AsString();
		rCmd = config.GetValue("commands", "r").AsString();
	}
}
