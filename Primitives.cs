using SFML.Graphics;
using SFML.System;
using System.Runtime.CompilerServices;

public class Ball : CircleShape
{
    new public Vector2f Position
    {
        get => base.Position + new Vector2f(base.Radius, base.Radius);
        set => base.Position = value - new Vector2f(base.Radius, base.Radius);
    }

    new public float Radius
    {
        get => base.Radius;
        set
        {
            base.Position += new Vector2f(base.Radius - value, base.Radius - value);
            base.Radius = value;
        }
    }

    public Vector2f Speed = new Vector2f();
    public Vector2f Acceleration = new Vector2f();

    public List<Ball> trail = new List<Ball>();
    public float trailLength = 1;
    public float trailInterval = 0.1f;
    private float trailTime = 0;

    public void Move(float DeltaTime)
    {
        Speed += DeltaTime * Acceleration;
        Position += DeltaTime * Speed;
    }

    public void NewTrailElement(float DeltaTime)
    {
        trailTime += DeltaTime;

        if (trailTime < trailInterval) return;

        trailTime -= trailInterval;

		trail.Add(new Ball { Position = Position, Radius = Radius, FillColor = FillColor });

		for (int i = 0; i < trail.Count; i++)
		{
			trail[i].Radius -= Radius / trailLength;

			if (trail[i].Radius <= 0)
			{
				trail.RemoveAt(i);
				i--;
				continue;
			}

			float ratio = trail[i].Radius / Radius;
			byte r = (byte)(FillColor.R * ratio);
			byte g = (byte)(FillColor.G * ratio);
			byte b = (byte)(FillColor.B * ratio);

			trail[i].FillColor = new Color(r, g, b);
		}
	}
}

public class Line : Drawable
{
    public Vector2f P1 = new Vector2f();
    public Vector2f P2 = new Vector2f();

    public Color Color = Color.White;
    public float Thickness = 1;

    public void Draw(RenderTarget target, RenderStates states)
    {
        Vector2f N = 0.5f * Thickness * Vector.Normal(P1, P2);

        Vertex[] vertices = new Vertex[4];
        vertices[0] = new Vertex(P1 + N, Color);
        vertices[1] = new Vertex(P2 + N, Color);
        vertices[2] = new Vertex(P2 - N, Color);
        vertices[3] = new Vertex(P1 - N, Color);

        target.Draw(vertices, PrimitiveType.Quads, states);
    }
}

public class RegularPolygon : Drawable
{
    public List<Line> Lines = new List<Line>();

    private Vector2f[] vertices;
    private Vector2f translation;
    private float scale;
    private float rotation;

    private Color color;
    private float thickness;

    public Vector2f Translation
    {
        get => translation;
        set
        {
            Vector2f d = value - translation;

            for (int i = 0; i < Lines.Count; i++)
            {
                Lines[i].P1 += d;
                Lines[i].P2 += d;
            }

            translation = value;
        }
    }

    public float Scale
    {
        get => scale;
        set
        {
            float d = value / scale;
            Vector2f t = (1 - d) * translation;

            for (int i = 0; i < Lines.Count;i++)
            {
                Lines[i].P1 *= d;
                Lines[i].P1 += t;
                Lines[i].P2 *= d;
                Lines[i].P2 += t;
            }

            scale = value;
        }
    }

    public float Rotation
    {
        get => rotation;
        set
        {
            float d = value - rotation;

            for (int i = 0; i < Lines.Count; i++)
            {
                Lines[i].P1 = Vector.Rotate(Lines[i].P1, d, translation);
                Lines[i].P2 = Vector.Rotate(Lines[i].P2, d, translation);
            }

            rotation = value;
        }
    }

    public Color Color
    {
        get => color;
        set
        {
            color = value;

            for (int i = 0; i < Lines.Count; i++)
            {
                Lines[i].Color = Color;
            }
        }
    }

    public float Thickness
    {
        get => thickness;
        set 
        {
            thickness = value;

            for (int i = 0; i < Lines.Count; i++)
            {
                Lines[i].Thickness = thickness;
            }
        }
    }

    public RegularPolygon(int VerticesCount, Vector2f Translation = new Vector2f(), float Scale = 1, float Rotation = 0)
    {
        vertices = new Vector2f[VerticesCount];
        translation = Translation;
        scale = Scale;
        rotation = Rotation;

        Vector2f P = new Vector2f(Scale, 0) + Translation;
        float Angle = 2 * (float)Math.PI / VerticesCount;

        for (int i = 0; i < VerticesCount; i++)
        {
            vertices[i] = Vector.Rotate(P, Angle * i, Translation);
            vertices[i] = Vector.Rotate(vertices[i], Rotation, Translation);
        }

        for (int i = 0; i < VerticesCount; i++)
        {
            Line line = new Line();
            line.P1 = vertices[i];
            line.P2 = vertices[(i + 1) % VerticesCount];
            
            Lines.Add(line);
        }
    }

    public void Draw(RenderTarget target, RenderStates states)
    {
        for (int i = 0; i < Lines.Count; i++)
        {
            target.Draw(Lines[i], states);
        }
    }
}