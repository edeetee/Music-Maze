using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

namespace Music_Maze
{
    class EquationTriangle : IVBO
    {
        public float equationMod = 1;
        //X, Z, equationMod : return Y
        Func<float, float, float, float> equation;
        int depth;

        Random ran = new Random();

        int totalVerts;

        List<Vector3> vertices;
        int verticesID;

        uint[] indices;
        uint indicesID;

        List<Vector3> colours;
        int coloursID;

        Vector3[] boundingPoints;

        public EquationTriangle(Vector3 corner1, Vector3 corner2, Vector3 corner3, Func<float,float,float,float> equation, int depth = 1)
        {
            boundingPoints = new Vector3[]{corner1, corner2, corner3};
            this.equation = equation;
            this.depth = depth;

            totalVerts = (int)Math.Pow(2, depth) * 3;

            vertices = new List<Vector3>();
            indices = new uint[totalVerts];
            colours = new List<Vector3>();

            //calculationTask = new Task(() =>
            //{
            //    CalculatePoints(ref boundingPoints[0], ref boundingPoints[1], ref boundingPoints[2], 0, totalVerts, 1f);
            //});
            //calculationTask.RunSynchronously();

            verticesID = GL.GenBuffer();
            coloursID = GL.GenBuffer();
            GL.GenBuffers(1, out indicesID);
        }

        public void Render(FrameEventArgs e)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, verticesID);
            GL.VertexPointer(3, VertexPointerType.Float, Vector3.SizeInBytes, IntPtr.Zero);

            GL.BindBuffer(BufferTarget.ArrayBuffer, coloursID);
            GL.ColorPointer(3, ColorPointerType.Float, Vector3.SizeInBytes, IntPtr.Zero);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indicesID);
            GL.DrawElements(BeginMode.Triangles, totalVerts, DrawElementsType.UnsignedInt, 0);
        }

        //left inclusive, right exclusive
        void CalculatePoints(ref Vector3 corner1, ref Vector3 corner2, ref Vector3 corner3, int left, int right, float mod)
        {
            if(left == right-3)
            {
                SetVertex(corner1, mod, left);
                SetVertex(corner2, mod, left + 1);
                SetVertex(corner3, mod, left + 2);
            }
            else
            {
                Vector3 median;
                Vector3 middle;
                Vector3 tri1;
                Vector3 tri2;

                SplitTriangle(ref corner1, ref corner2, ref corner3, out median, out  middle, out tri1, out tri2);

                int mid = left + (right-left) / 2;

                CalculatePoints(ref tri1, ref middle, ref median, left, mid, mod);
                CalculatePoints(ref tri2, ref middle, ref median, mid, right, mod);
            }
        }

        void SetVertex(Vector3 point, float mod, int i)
        {
            Vector3 pos = new Vector3(point.X, point.Y + equation(point.X, point.Z, mod), point.Z);

            uint id = (uint)vertices.IndexOf(pos);
            if(id == uint.MaxValue)
            {
                vertices.Add(pos);
                colours.Add(new Vector3(pos.X/8f, pos.Y, pos.Z/8f) * ( mod < 0.5f ? 0.5f : mod ) );
                id = (uint)vertices.Count - 1;
            }

            indices[i] = id;
        }

        void SplitTriangle(ref Vector3 corner1, ref Vector3 corner2, ref Vector3 corner3,
            out Vector3 median, out Vector3 middle, out Vector3 newCorner1, out Vector3 newCorner2)
        {
            Vector3[] diff = new Vector3[]{(corner2-corner1), (corner3-corner2), (corner1-corner3)};
            Vector3[] verts = new Vector3[] { corner1, corner2, corner3 };
            int maxI = 0;

            for(int i = 0; i < 3; i++)
            {
                if (diff[maxI].Length < diff[i].Length)
                {
                    maxI = i;
                }
            }

            middle = verts[(maxI - 1).Mod(3)];
            newCorner1 = verts[(maxI).Mod(3)];
            newCorner2 = verts[(maxI + 1).Mod(3)];

            median = diff[maxI]/2 + newCorner1;
        }

        public void Buffer(float mod)
        {
            equationMod += 0.01f;

            CalculatePoints(ref boundingPoints[0], ref boundingPoints[1], ref boundingPoints[2], 0, totalVerts, mod);

            //if(calculationTask.IsCompleted)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, verticesID);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Count * Vector3.SizeInBytes), vertices.ToArray(), BufferUsageHint.DynamicDraw);

                GL.BindBuffer(BufferTarget.ArrayBuffer, coloursID);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(colours.Count * Vector3.SizeInBytes), colours.ToArray(), BufferUsageHint.DynamicDraw);

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, indicesID);
                GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(totalVerts * sizeof(uint)), indices.ToArray(), BufferUsageHint.DynamicDraw);

                Console.SetCursorPosition(0, 1);
                Console.Write(String.Format("Total points:  {0}", totalVerts));

                Console.SetCursorPosition(0, 2);
                Console.Write(String.Format("Vertices:      {0}", vertices.Count));

                vertices = new List<Vector3>();
                indices = new uint[totalVerts];
                colours = new List<Vector3>();

                //calculationTask = new Task(() =>
                //{
                   
                //});

                //calculationTask.Start();
            }
        }
    }
}
