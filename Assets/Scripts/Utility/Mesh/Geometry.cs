using UnityEngine;

namespace Assets.Scripts.Utility.Mesh
{
    public static class Geometry
    {
        public static float Orientation(Vector2 a, Vector2 b, Vector2 c)
        {
            return (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
        }

        public static bool InCircle(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            float ax_ = a.x - d.x, ay_ = a.y - d.y;
            float bx_ = b.x - d.x, by_ = b.y - d.y;
            float cx_ = c.x - d.x, cy_ = c.y - d.y;

            float det = (ax_ * ax_ + ay_ * ay_) * (bx_ * cy_ - cx_ * by_) -
                        (bx_ * bx_ + by_ * by_) * (ax_ * cy_ - cx_ * ay_) +
                        (cx_ * cx_ + cy_ * cy_) * (ax_ * by_ - bx_ * ay_);

            return det < 0;
        }

        public static bool Intersects(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            float o1 = Orientation(a, b, c);
            float o2 = Orientation(a, b, d);
            float o3 = Orientation(c, d, a);
            float o4 = Orientation(c, d, b);

            return ((o1 > 0 && o2 < 0) || (o1 < 0 && o2 > 0)) &&
                    ((o3 > 0 && o4 < 0) || (o3 < 0 && o4 > 0));
        }
    }
}