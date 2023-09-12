using System.Collections.Generic;
using System.Text.Json.Serialization;

public class Message {
    public List<Marker> markers { get; set; } = new List<Marker>();
    public class Marker {
        [JsonPropertyName("marker-id")]
        public int marker_id { get; set; }
        public Corners corners { get; set; } = new Corners();
    }
    public class Corners {
        [JsonPropertyName("1")]
        public Point corner1 { get; set; } = new Point();
        [JsonPropertyName("2")]
        public Point corner2 { get; set; } = new Point();
        [JsonPropertyName("3")]
        public Point corner3 { get; set; } = new Point();
        [JsonPropertyName("4")]
        public Point corner4 { get; set; } = new Point();
        public void UpdateCorners(Godot.Vector2 pos0, Godot.Vector2 pos1, Godot.Vector2 pos2, Godot.Vector2 pos3) {
            corner1.x = pos0.X;
			corner1.y = pos0.Y;
			corner2.x = pos1.X;
			corner2.y = pos1.Y;
			corner3.x = pos2.X;
			corner3.y = pos2.Y;
			corner4.x = pos3.X;
			corner4.y = pos3.Y;
        }
    }
    public class Point {
        public float x { get; set; }
        public float y { get; set; }
    }
}