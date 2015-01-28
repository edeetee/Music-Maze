using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Music_Maze
{
    class Point : IVBO
    {
        Vector3 pos;
        Vector3 colour;

        public Point(Vector3 position) : this(position, new Vector3(1, 1, 1)) { }

        public Point(Vector3 position, Vector3 colour)
        {
            this.pos = position;
            this.colour = colour;
        }

        public void Render(FrameEventArgs e)
        {
            GL.Begin(PrimitiveType.Points);
            GL.Color3(colour);
            GL.Vertex3(pos);
            GL.End();
        }

        public void Buffer() { }
    }
}
