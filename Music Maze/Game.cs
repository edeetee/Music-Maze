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

        public Game() : base(640,480, GraphicsMode.Default, "Music Maze 0.1")
        {
            VSync = VSyncMode.Adaptive;

            SetupView();
            modelView = Matrix4.Identity;

            Func<float, float, float> equation = (x, y) => (float)(Math.Sin(x*2) * Math.Sin(y*2)/2);
            //Func<float, float, float> equation = (x, y) => x;
            int depth = 8;

            elements = new List<IVBO>()
            {
                //new Pyramid(new Vector3(0,1,2), new Vector3(-1,0,1), new Vector3(1,0,1), new Vector3(1,0,3), new Vector3(-1,0,3)),

                new Triangle(new Vector3(0,1,-1), new Vector3(-1,0,-1), new Vector3(1,0,-1), new Vector3(1,1,0)),
                new Triangle(new Vector3(0,-1,-1), new Vector3(-1,0,-1), new Vector3(1,0,-1), new Vector3(1,0,1)),

                new Point(new Vector3(0,0,0)),

                new Point(new Vector3(0,0,1)),
                new Point(new Vector3(1,0,0)),

                new EquationTriangle(new Vector3(4,0,0), new Vector3(0,0,0), new Vector3(0,0,4), equation, depth),
                new EquationTriangle(new Vector3(4,0,0), new Vector3(4,0,4), new Vector3(0,0,4), equation, depth)

            };

            foreach (IVBO element in elements)
            {
                element.Buffer();
            }
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

            foreach(IVBO element in elements)
            {
                element.Buffer();
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

            Console.Clear();
            Console.Write(RenderFrequency);

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
