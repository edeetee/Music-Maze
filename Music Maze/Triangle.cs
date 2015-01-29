using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Music_Maze
{
    class Triangle : IVBO
    {
        Vector3[] points;
        int pointsID;

        Vector3[] colours;
        int coloursID;

        int primitiveCount;

        public Triangle(Vector3 point1, Vector3 point2, Vector3 point3)
            : this(point1, point2, point3, new Vector3(1, 1, 1), new Vector3(1, 1, 1), new Vector3(1, 1, 1)) { }

        public Triangle(Vector3 point1, Vector3 point2, Vector3 point3, Vector3 colour)
            : this(point1, point2, point3, colour, colour, colour) { }

        public Triangle(Vector3 point1, Vector3 point2, Vector3 point3, Vector3 colour1, Vector3 colour2, Vector3 colour3)
        {
            this.points = new Vector3[3] { point1, point2, point3 };
            this.colours = new Vector3[3] { colour1, colour2, colour3 };
        }

        public void Render(FrameEventArgs e)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, pointsID);
            GL.VertexPointer(3, VertexPointerType.Float, Vector3.SizeInBytes, IntPtr.Zero);

            GL.BindBuffer(BufferTarget.ArrayBuffer, coloursID);
            GL.ColorPointer(3, ColorPointerType.Float, Vector3.SizeInBytes, IntPtr.Zero);

            GL.DrawArrays(PrimitiveType.Triangles, 0, primitiveCount);
        }

        public void Buffer(float mod)
        {
            primitiveCount = points.Length;

            GL.GenBuffers(1, out pointsID);
            GL.BindBuffer(BufferTarget.ArrayBuffer, pointsID);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(points.Length * Vector3.SizeInBytes), points, BufferUsageHint.StaticDraw);

            GL.GenBuffers(1, out coloursID);
			GL.BindBuffer(BufferTarget.ArrayBuffer, coloursID);
			GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(colours.Length * Vector3.SizeInBytes), colours, BufferUsageHint.StaticDraw);
        }
    }
}