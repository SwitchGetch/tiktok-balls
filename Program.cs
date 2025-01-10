using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System.Diagnostics;

public static class Game
{
    private static RenderWindow Window = new RenderWindow(new VideoMode(1600, 900), "tiktok ball");
    private static bool Running = true;
    private static Vector2f Center { get => (Vector2f)Window.Size / 2; }
    private static Vector2f MousePosition { get => (Vector2f)Mouse.GetPosition(Window); }

    private static Color RandomColor { get => new Color((byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256)); }

    private static List<Ball> balls = new List<Ball>();
    private static Ball outer = new Ball();

    private static Stopwatch timer = new Stopwatch();
    private static float time = 0;
    private static float deltaTime = 0;
    private static int fps = 0;
    private static int frames = 0;
    private static int seconds = 0;

    private static Random random = new Random();

    public static void Run()
    {
        Start();

        while (Running)
        {
            Update();

            Render();
        }
    }

    private static void Start()
    {
        Window.SetVerticalSyncEnabled(true);

        Window.Closed += (s, e) => Running = false;

        Window.MouseButtonPressed += (s, e) =>
        {
            if (e.Button == Mouse.Button.Left) NewBall();
            else if (e.Button == Mouse.Button.Right) DeleteBall();
        };

        outer.Position = Center;
        outer.Radius = 400;
        outer.FillColor = Color.Black;
        outer.OutlineColor = Color.White;
        outer.OutlineThickness = 10;
        outer.SetPointCount(100);

        timer.Start();
    }

    private static void Update()
    {
        TimeHandler();

        Window.DispatchEvents();

        if (Keyboard.IsKeyPressed(Keyboard.Key.Space)) NewBall();
        if (Keyboard.IsKeyPressed(Keyboard.Key.LShift)) DeleteBall();

        for (int i = 0; i < balls.Count; i++)
        {
            balls[i].Move(deltaTime);

            OuterCollision(i);
            //WindowCollision(i);
        }

        for (int i = 0; i < balls.Count; i++)
        {
            for (int j = i + 1; j < balls.Count; j++)
            {
                BallCollision(i, j);
            }
        }
    }

    private static void Render()
    {
        Window.Clear();

        Window.Draw(outer);

        for (int i = 0; i < balls.Count; i++)
        {
            for (int j = 0; j < balls[i].trail.Count; j++)
            {
                Window.Draw(balls[i].trail[j]);
            }
        }

        for (int i = 0; i < balls.Count; i++) Window.Draw(balls[i]);

        Window.Display();
    }

    private static void TimeHandler()
    {
        deltaTime = (float)timer.Elapsed.TotalSeconds;
        time += deltaTime;
        frames++;

        if ((int)time > seconds)
        {
            fps = frames / ((int)time - seconds);
            seconds = (int)time;
            frames = 0;

            Window.SetTitle($"FPS: {fps}");
        }

        timer.Restart();
    }

    private static void NewBall()
    {
        Ball ball = new Ball();
        ball.Radius = 50;
        ball.Position = MousePosition;
        ball.Acceleration = new Vector2f(0, 1000);
        ball.FillColor = RandomColor;
        ball.OutlineColor = Color.White;
        ball.OutlineThickness = 1;

        balls.Add(ball);
    }

    private static void DeleteBall()
    {
        for (int i = 0; i < balls.Count; i++)
        {
            if (Vector.Distance(MousePosition, balls[i].Position) < balls[i].Radius)
            {
                balls.RemoveAt(i);
                i--;
            }
        }
    }

    private static bool WindowCollision(int i)
    {
        if (balls[i].Position.X - balls[i].Radius < 0)
        {
            balls[i].Position = new Vector2f(balls[i].Radius, balls[i].Position.Y);
            balls[i].Speed.X *= -1;

            return true;
        }

        if (balls[i].Position.X + balls[i].Radius > Window.Size.X)
        {
            balls[i].Position = new Vector2f(Window.Size.X - balls[i].Radius, balls[i].Position.Y);
            balls[i].Speed.X *= -1;

            return true;
        }

        if (balls[i].Position.Y - balls[i].Radius < 0)
        {
            balls[i].Position = new Vector2f(balls[i].Position.X, balls[i].Radius);
            balls[i].Speed.Y *= -1;

            return true;
        }

        if (balls[i].Position.Y + balls[i].Radius > Window.Size.Y)
        {
            balls[i].Position = new Vector2f(balls[i].Position.X, Window.Size.Y - balls[i].Radius);
            balls[i].Speed.Y *= -1;

            return true;
        }

        return false;
    }

    private static bool BallCollision(int i, int j)
    {
        float R = balls[i].Radius + balls[j].Radius;
        float D = Vector.Distance(balls[i].Position, balls[j].Position);

        if (R < D) return false;

        Vector2f n = Vector.Normalize(balls[i].Position - balls[j].Position);
        Vector2f d = 0.5f * (R - D) * n;

        balls[i].Position += d;
        balls[j].Position -= d;

        balls[i].Speed = Vector.Reflect(balls[i].Speed, n);
        balls[j].Speed = Vector.Reflect(balls[j].Speed, -n);

        return true;
    }

    private static bool OuterCollision(int i)
    {
        float R = outer.Radius - balls[i].Radius;
        float D = Vector.Distance(outer.Position, balls[i].Position);

        if (R > D) return false;

        Vector2f n = Vector.Normalize(outer.Position - balls[i].Position);

        balls[i].Position += (D - R) * n;
        balls[i].Speed = Vector.Reflect(balls[i].Speed, n);

        return true;
    }

    private static bool LineCollision(ref Ball ball, ref Line line)
    {
        //if (Vector.Distance(line.P1, ball.Position) < ball.Radius || Vector.Distance(line.P2, ball.Position) < ball.Radius)
        //{
        //    Vector2f n = Vector.Normal(line.P1, line.P2);

        //    if (Vector.Dot(n, ball.Position - line.P1) < 0) n *= -1;

        //    ball.Speed = Vector.Reflect(ball.Speed, n);

        //    return true;
        //}

        Vector2f D = line.P2 - line.P1;
        Vector2f A = line.P1 - ball.Position;
        float a = Vector.Dot(D);
        float b = 2 * Vector.Dot(D, A);
        float c = Vector.Dot(A) - ball.Radius * ball.Radius;
        float d = b * b - 4 * a * c;

        if (d >= 0)
        {
            float t = -b / (2 * a);

            if (0 <= t && t <= 1)
            {
                Vector2f p = line.P1 + t * D;
                Vector2f v = ball.Position - p;
                float l = Vector.Length(v);
                Vector2f n = v / l;

                ball.Position += n * (ball.Radius - l);
                ball.Speed = Vector.Reflect(ball.Speed, n);

                return true;
            }
        }

        return false;
    }

    private static bool PolyCollision(ref Ball ball, ref RegularPolygon poly)
    {
        for (int i = 0; i < poly.Lines.Count; i++)
        {
            Line line = poly.Lines[i];

            if (LineCollision(ref ball, ref line))
            {
                return true;
            }
        }

        return false;
    }
}

class Program
{
    static void Main(string[] args)
    {
        Game.Run();
    }
}