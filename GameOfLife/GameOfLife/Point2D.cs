using System;
using System.Drawing;
using System.Globalization;

namespace GameOfLife
{
    public class Point2D
    {
        private float x;
        private float y;
        public static readonly Point Empty;
        public bool IsEmpty { get { if (x == 0) { return y == 0; } return false; } }
        public float X { get { return x; } set { x = value; } }
        public float Y { get { return y; } set { y = value; } }
        public Point2D(float x, float y) { this.x = x; this.y = y; }
        public Point2D(Point p) { this.x = p.X; this.y = p.Y; }
        public Point2D(PointF p) { this.x = p.X; this.y = p.Y; }
        public Point2D(Size p) { this.x = p.Width; this.y = p.Height; }
        public Point2D(int dw) { x = (float)LOWORD(dw); y = (float)HIWORD(dw); }
        public static implicit operator Point2D(Point p) { return new Point2D(p.X, p.Y); }
        public static implicit operator Point2D(PointF p) { return new Point2D(p.X, p.Y); }
        public static implicit operator Point2D(Size p) { return new Point2D(p.Width, p.Height); }
        public static explicit operator Size(Point2D p) { return new Size((int)p.X, (int)p.Y); }
        public static explicit operator Point(Point2D p) { return new Point((int)p.X, (int)p.Y); }
        public static explicit operator PointF(Point2D p) { return new PointF(p.X, p.Y); }
        public static Point2D operator +(Point2D p1, Point2D p2) { return Addition(p1, p2); }
        public static Point2D Addition(Point2D p1, Point2D p2) { return new Point2D(p1.X + p2.X, p1.Y + p2.Y); }
        public static Point2D operator -(Point2D p1, Point2D p2) { return Subtraction(p1, p2); }
        public static Point2D Subtraction(Point2D p1, Point2D p2) { return new Point2D(p1.X - p2.X, p1.Y - p2.Y); }
        public static Point2D operator *(Point2D p1, Point2D p2) { return Multiplication(p1, p2); }
        public static Point2D Multiplication(Point2D p1, Point2D p2) { return new Point2D(p1.X * p2.X, p1.Y * p2.Y); }
        public static Point2D operator /(Point2D p1, Point2D p2) { return Division(p1, p2); }
        public static Point2D Division(Point2D p1, Point2D p2) { return new Point2D(p1.X / p2.X, p1.Y / p2.Y); }
        public static bool operator ==(Point2D left, Point2D right) { if (left.X == right.X) { return left.Y == right.Y; } return false; }
        public static bool operator !=(Point2D left, Point2D right) { return !(left == right); }
        public static Point2D Truncate(Point2D value) { return new Point2D((int)value.X, (int)value.Y); }
        public static Point2D Round(Point2D value) { return new Point2D((int)Math.Round(value.X), (int)Math.Round(value.Y)); }
        public override bool Equals(object obj) { if (!(obj is Point2D point)) return false; if (point.X == X) return point.Y == Y; return false; }
        public override int GetHashCode() { return ((int)x ^ (int)y); }
        public void Offset(float dx, float dy) { X += dx; Y += dy; }
        public void Offset(Point p) { Offset(p.X, p.Y); }
        public void Offset(Point2D p) { Offset(p.X, p.Y); }
        public void Offset(Size p) { Offset(p.Width, p.Height); }
        public override string ToString() { return "{X=" + X.ToString(CultureInfo.CurrentCulture) + ",Y=" + Y.ToString(CultureInfo.CurrentCulture) + "}"; }
        private static int HIWORD(int n) { return (n >> 16) & 0xFFFF; }
        private static int LOWORD(int n) { return n & 0xFFFF; }
        public static Point2D operator +(Point2D p1, float a) { return Addition(p1, a); }
        public static Point2D Addition(Point2D p1, float a) { return new Point2D(p1.X + a, p1.Y + a); }
        public static Point2D operator -(Point2D p1, float a) { return Subtraction(p1, a); }
        public static Point2D Subtraction(Point2D p1, float a) { return new Point2D(p1.X - a, p1.Y - a); }
        public static Point2D operator *(Point2D p1, float a) { return Multiplication(p1, a); }
        public static Point2D Multiplication(Point2D p1, float a) { return new Point2D(p1.X * a, p1.Y * a); }
        public static Point2D operator /(Point2D p1, float a) { return Division(p1, a); }
        public static Point2D Division(Point2D p1, float a) { return new Point2D(p1.X / a, p1.Y / a); }
        public static Point2D operator %(Point2D p1, float a) { return Remainder(p1, a); }
        public static Point2D Remainder(Point2D p1, float a) { return new Point2D(p1.X % a, p1.Y % a); }
    }
}
