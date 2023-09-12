using System.Collections.Generic;

public class BallMessage {
    public List<Center> ball { get; set; } = new List<Center>();
    public class Center {
        public float x { get; set; }
        public float y { get; set; }
        public Center(float x, float y) {
            this.x = x;
            this.y = y;
        }
    }
}