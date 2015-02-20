using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Music_Maze
{
    class Game : GameWindow
    {
        string vertexShaderSource = @"
#version 400

// incoming delta matrix for model matrix
uniform mat4 model_matrix;
 
// object space to camera space transformation
uniform mat4 view_matrix;
 
// camera space to clip coordinates
uniform mat4 projection_matrix;
 
// incoming vertex position
layout(location = 1) in vec4 vertex_position;

//colour
layout(location = 2) in vec3 colour;

//colour
layout(location = 3) in vec3 normal;

out vec3 vertexColour;

void main()
{
    // transforming the incoming vertex position
    gl_Position = projection_matrix * view_matrix * model_matrix * vertex_position;

    vertexColour = abs(normalize( mat3(model_matrix) * normal ));
}";

        string fragmentShaderSource = @"
#version 400

flat in vec3 vertexColour;

void main()
{
    gl_FragColor = vec4(vertexColour, 1.0);
}";

        public static int programID, modelMatrixID, viewMatrixID, projectionMatrixID;

        float lookSpeed = 1f;
        float moveSpeed = 0.05f;
        List<GameObject> objects;

        Matrix4 modelMatrix;

        Vector3 pos;
        Vector3 forward;
        Vector3 right;
        Vector3 up;

        Vector2 mouse;
        bool mouseLock = true;

        public struct DebugTime
        {
            public TimeSpan time;
            public string title;
        }

        public static DebugTime[] times = new DebugTime[]{
            new DebugTime{time = TimeSpan.Zero, title = "Total Calc: {0}"},
            new DebugTime{time = TimeSpan.Zero, title = "Point Calc: {0}"},
            new DebugTime{time = TimeSpan.Zero, title = "Tri Split Calc: {0}"},
            new DebugTime{time = TimeSpan.Zero, title = "Buffer: {0}"},
            new DebugTime{time = TimeSpan.Zero, title = "Draw: {0}"},
            new DebugTime{time = TimeSpan.Zero, title = "MusicCalc: {0}"},
        };

        System.Diagnostics.Stopwatch allTime;

        MusicAnalyse music;

        const float pi = (float)Math.PI;

        public Game() : base(1280,800, GraphicsMode.Default, "Music Maze 0.1")
        {
            VSync = VSyncMode.Adaptive;

            allTime = new System.Diagnostics.Stopwatch();

            GL.Viewport(0, 0, Width, Height);
            VSync = VSyncMode.Adaptive;
            
            int depth = 8;
            float size = 2;

            modelMatrix = Matrix4.Identity;

            //Expression<Func<float, float, float, float>> equation1 = (x, y, mod) => (float)((Cos(x / size * pi) * Cos(y / size * pi)) * mod / 2 * size);
            Expression<Func<float, float, float, float>> equation1 = (x, y, mod) => (float)(Math.Cos(x * (1+mod)*2 / size * pi));

            Expression<Func<float, float, float, float>> equation2 = (x, y, mod) => -(float)((Cos(x / size * pi) * Cos(y / size * pi)) * mod / 4 * size);

            //Func<float, float, float, float> equation3 = (x, y, mod) => equation2(x, y, mod) * 3;

            Expression<Func<float, float, float, float>> equationb = (x, y, mod) => -(float)((Cos(x / size * pi) * Cos(y / size * pi)) * x * mod / 4 * size);

            objects = new List<GameObject>()
            {
                new EquationCuboid(new Vector3(3,3,3), Vector3.One*2, Quaternion.Identity, depth, equation2, new Vector3(0,1,1)),

                new EquationCuboid(Vector3.Zero, Vector3.One*200, Quaternion.Identity, depth, equation2, new Vector3(0,1,1)),

                new EquationCuboid(new Vector3(222,0,0), Vector3.One*2, Quaternion.Identity, depth, equation2, new Vector3(0,1,1)),

                new EquationCuboid(new Vector3(0,222,0), Vector3.One*2, Quaternion.Identity, depth, equation2, new Vector3(0,1,1)),

                new EquationCuboid(Vector3.Zero, Vector3.One*2, Quaternion.FromAxisAngle(new Vector3(1,1,0), 1f), depth, equation2, new Vector3(0,1,1)),

                new EquationCuboid(new Vector3(-3,-3,-3), new Vector3(0.5f, 1, 0.5f), Quaternion.Identity, depth, equation1, new Vector3(0,0,1)),

                new EquationCuboid(new Vector3(5,-1,0), Vector3.One, Quaternion.FromAxisAngle(new Vector3(1,-1,0), 1f), depth, equation1, new Vector3(1,1,0)),

                new EquationCuboid(Vector3.Zero, Vector3.One*20, Quaternion.Identity, depth, equation2, new Vector3(1,0,1))
            };

            music = new MusicAnalyse("early.wav");

            music.Play();
        }

        public static void GetError()
        {
            var error = GL.GetError();
            if(error != ErrorCode.NoError)
            {

            }
        }

        protected override void OnLoad(EventArgs e)
        {
            programID = GL.CreateProgram();

            LoadShader(vertexShaderSource, ShaderType.VertexShader);
            LoadShader(fragmentShaderSource, ShaderType.FragmentShader);

            GL.LinkProgram(programID);
            GL.ValidateProgram(programID);
            GL.UseProgram(programID);

            viewMatrixID = GL.GetUniformLocation(programID, "view_matrix");
            projectionMatrixID = GL.GetUniformLocation(programID, "projection_matrix");
            modelMatrixID = GL.GetUniformLocation(programID, "model_matrix");

            allTime.Start();

            foreach (GameObject element in objects)
            {
                element.Buffer(0);
            }
        }

        public int LoadShader(string shader, ShaderType shaderType)
        {
            int shaderID = GL.CreateShader(shaderType);
            GL.ShaderSource(shaderID, shader);
            GL.CompileShader(shaderID);
            GL.AttachShader(programID, shaderID);

            return shaderID;
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
            GL.Viewport(0, 0, Width, Height);
            SetProjection();
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

            var calculatedMoveSpeed = moveSpeed;

            if (state[Key.LShift])
                calculatedMoveSpeed *= 5;

            if (state[Key.W])
                pos += forward * calculatedMoveSpeed;
            if (state[Key.S])
                pos -= forward * calculatedMoveSpeed;
            if (state[Key.A])
                pos -= right * calculatedMoveSpeed;
            if (state[Key.D])
                pos += right * calculatedMoveSpeed;
            if (state[Key.Space])
                pos += up * calculatedMoveSpeed;

            SetModelview();

            var start = new System.Diagnostics.Stopwatch();
            start.Start();

            var curMod = music.CurrentMagnitude();

            times[5].time += start.Elapsed;

            var obj = objects.Last();

            obj.Rotation *= Quaternion.FromAxisAngle(new Vector3(1, 1, 0)+obj.Rotation.Xyz, curMod/50);

            foreach(GameObject element in objects)
            {
                element.Buffer(curMod);

                GetError();
            }
        }

        void SetModelview(Matrix4 matrix = default(Matrix4))
        {
            if(matrix == default(Matrix4))
            {
                matrix = Matrix4.LookAt(pos, pos + forward, up);
            }
            GL.UniformMatrix4(viewMatrixID, false, ref matrix);
        }

        void SetProjection(Matrix4 matrix = default(Matrix4))
        {
            if (matrix == default(Matrix4))
            {
                matrix = Matrix4.CreatePerspectiveFieldOfView(pi/3f, (float)Width / (float)Height, 1f, 50f);
            }
            GL.UniformMatrix4(projectionMatrixID, true, ref matrix);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.Enable(EnableCap.DepthTest);
            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.PolygonSmooth);

            GL.BlendFunc(BlendingFactorSrc.SrcAlphaSaturate, BlendingFactorDest.Src1Color);

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

            //GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            foreach (GameObject element in objects)
            {
                element.Render(e, ref modelMatrix);
                GetError();
            }

            SwapBuffers();

            Console.Clear();
            foreach (var time in times)
            {
                Console.WriteLine(time.title, (time.time.TotalMilliseconds / allTime.Elapsed.TotalMilliseconds));
            }
            Console.WriteLine("FPS: {0}", RenderFrequency);
        }
    }
}
