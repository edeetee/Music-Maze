using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Music_Maze
{
    class Game : GameWindow
    {
        float lookSpeed = 1f;
        float moveSpeed = 0.05f;
        List<IVBO> elements;
        Matrix4 projection;
        Matrix4 modelView;

        Vector3 pos;
        Vector3 forward;
        Vector3 right;
        Vector3 up;

        Vector2 mouse;
        bool mouseLock = true;

        MusicAnalyse music;

        const float pi = (float)Math.PI;

        //float curMod = 1f;
        //float modSpeedMod = 0.3f;

        public Game() : base(1280,800, GraphicsMode.Default, "Music Maze 0.1")
        {
            //VSync = VSyncMode.Adaptive;
            VSync = VSyncMode.Off;
            //TargetRenderFrequency = 60d;
            //TargetUpdateFrequency = 60d;

            SetupView();
            modelView = Matrix4.Identity;
            
            int depth = 6;
            float size = 2;

            Func<float, float, float, float> equation1 = (x, y, mod) => (float)((Cos(x / size * pi) * Cos(y / size * pi)) * mod/2 * size);

            Func<float, float, float, float> equation2 = (x, y, mod) => -(float)((Cos(x / size * pi) * Cos(y / size * pi)) * mod / 4 * size);

            Func<float, float, float, float> equation3 = (x, y, mod) => equation2(x, y, mod) * 3;

            Func<float, float, float, float> equationb = (x, y, mod) => -(float)((Cos(x / size * pi) * Cos(y / size * pi)) * x * mod / 4 * size);

            elements = new List<IVBO>()
            {
                new Point(new Vector3(0,0,0)),

                new Point(new Vector3(0,0,1)),
                new Point(new Vector3(1,0,0)),
                new Point(new Vector3(0,0,-1)),
                new Point(new Vector3(-1,0,0)),

                new EquationCuboid(new Vector3(3,3,3), 2, 2, 2, depth, equation2, new Vector3(0,1,1)),

                new EquationCuboid(new Vector3(-3,0,-3), 0.5f, 2, 0.5f, depth, equation1, new Vector3(0,0,1)),

                new EquationCuboid(Vector3.Zero, 1, 1, 1, depth, equation1, new Vector3(1,1,0)),

                new EquationCuboid(Vector3.Zero, 20, 20, 20, depth, equation3, new Vector3(1,0,1))
            };

            music = new MusicAnalyse("early.wav");

            music.Play();

            var curMod = music.CurrentMagnitude();

            foreach (IVBO element in elements)
            {
                element.Buffer(curMod);
            }
        }

        public float Cos(float x)
        {
            x += pi / 2;

            if (x > pi)   // Original x > pi/2
            {
                x -= 2 * pi;   // Wrap: cos(x) = cos(x - 2 pi)
            }

            return Sin(x);
        }

        public float Sin(float x)
        {
            const float B = 4 / pi;
            const float C = -4 / (pi * pi);

            return (B * x + C * x * x);
        } 

        protected override void OnResize(EventArgs e)
        {
            SetupView();
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            switch(e.Key)
            {
                case Key.Escape:
                    mouseLock = !mouseLock;
                    break;
            }
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            var pre = OpenTK.Input.Mouse.GetCursorState();
            if (mouseLock) { OpenTK.Input.Mouse.SetPosition(Bounds.Left + Width / 2, Bounds.Top + Height / 2); };
            var post = OpenTK.Input.Mouse.GetCursorState();

            mouse -= new Vector2((pre.X - post.X) / (float)Width, (pre.Y - post.Y) / (float)Height) * lookSpeed;

            forward = new Vector3(
                (float)(Math.Cos(mouse.Y) * Math.Sin(mouse.X)),
                (float)Math.Sin(mouse.Y),
                (float)(Math.Cos(mouse.Y) * Math.Cos(mouse.X))
                );

            right = new Vector3(
                (float)Math.Sin(mouse.X - Math.PI / 2.0f),
                0f,
                (float)Math.Cos(mouse.X - Math.PI / 2.0f)
                );

            up = Vector3.Cross(right, forward);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            var state = OpenTK.Input.Keyboard.GetState();

            if (state[Key.W])
                pos += forward * moveSpeed;
            if (state[Key.S])
                pos -= forward * moveSpeed;
            if (state[Key.A])
                pos -= right * moveSpeed;
            if (state[Key.D])
                pos += right * moveSpeed;
            if (state[Key.Space])
                pos += up * moveSpeed;

            modelView = Matrix4.LookAt(pos, pos + forward, up);

            var curMod = music.CurrentMagnitude();

            foreach(IVBO element in elements)
            {
                element.Buffer(curMod);
            }
        }

        void SetupView()
        {
            GL.Viewport(0, 0, Width, Height);
            UpdateProjection();
        }

        void UpdateProjection()
        {
            projection = Matrix4.CreatePerspectiveFieldOfView(2f, Width / (float)Height, 0.01f, 100f);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            //Console.SetCursorPosition(0, 0);
            //Console.Write(RenderFrequency);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelView);

            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.IndexArray);
            GL.EnableClientState(ArrayCap.ColorArray);

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            //GL.Blend

            foreach (IVBO element in elements)
            {
                element.Render(e);
            }

            SwapBuffers();
        }
    }
}
