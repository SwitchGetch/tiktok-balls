using SFML.System;
using System.Runtime.Serialization;

public static class Vector
{
    public static float Length(Vector2f V) => (float)Math.Sqrt(Dot(V));

    public static float Distance(Vector2f V1, Vector2f V2) => (float)Math.Sqrt(Dot(V2 - V1));

    public static float Dot(Vector2f V) => V.X * V.X + V.Y * V.Y;

    public static float Dot(Vector2f V1, Vector2f V2) => V1.X * V2.X + V1.Y * V2.Y;

    public static Vector2f Normalize(Vector2f V) => V / Length(V);

    public static Vector2f Rotate(Vector2f V, float Angle, Vector2f R = new Vector2f())
    {
        Vector2f P = V - R;
        float Cos = (float)Math.Cos(Angle);
        float Sin = (float)Math.Sin(Angle);

        return new Vector2f(P.X * Cos - P.Y * Sin, P.X * Sin + P.Y * Cos) + R;
    }

    public static Vector2f Normal(Vector2f V1, Vector2f V2)
    {
        Vector2f D = V2 - V1;
        Vector2f N = new Vector2f(-D.Y, D.X);

        return Normalize(N);
    }

    public static Vector2f Reflect(Vector2f V, Vector2f N) => V - 2 * Dot(V, N) * N;
}