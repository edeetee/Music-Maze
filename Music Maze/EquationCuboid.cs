using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace Music_Maze
{
    class EquationCuboid : GameObject
    {
        static Quaternion up = Quaternion.FromAxisAngle(new Vector3(1,0,0), 0);
        static Quaternion down = Quaternion.FromAxisAngle(new Vector3(1, 0, 0), (float)Math.PI);
        static Quaternion left = Quaternion.FromAxisAngle(new Vector3(1, 0, 0), 0.5f * (float)Math.PI);
        static Quaternion right = Quaternion.FromAxisAngle(new Vector3(-1, 0, 0), 0.5f * (float)Math.PI);
        static Quaternion forward = Quaternion.FromAxisAngle(new Vector3(0, 0, -1), 0.5f * (float)Math.PI);
        static Quaternion back = Quaternion.FromAxisAngle(new Vector3(0, 0, 1), 0.5f * (float)Math.PI);

       // float totalRot;

        public EquationCuboid(Vector3 pos, Vector3 scale, Quaternion angle, int renderDepth, Func<float, float, float, float> equation, Vector3 colour) : base(pos, scale, angle)
        {
            children = new List<GameObject>(){
                new EquationRectangle(new Vector3(0,1,0), Vector3.One, up, renderDepth, equation, colour * 0.7f),
                new EquationRectangle(new Vector3(0,-1,0), Vector3.One, down, renderDepth, equation, colour * 0.8f),
                new EquationRectangle(new Vector3(0,0,1), Vector3.One, left, renderDepth, equation, colour * 0.9f),
                new EquationRectangle(new Vector3(0,0,-1), Vector3.One, right, renderDepth, equation, colour * 1f),
                new EquationRectangle(new Vector3(1,0,0), Vector3.One, forward, renderDepth, equation, colour * 1.1f),
                new EquationRectangle(new Vector3(-1, 0, 0), Vector3.One, back, renderDepth, equation, colour * 1.2f)
            };
        }
    }
}
