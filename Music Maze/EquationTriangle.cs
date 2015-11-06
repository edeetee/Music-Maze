using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;


using System.Diagnostics;
using System.Linq.Expressions;

namespace Music_Maze
{
    struct TriData
    {
        public int verticesID;
        public int coloursID;
        public int normalsID;
    }

    class EquationTriangle : GameObject
    {
        Func<float, float, float, float> equation;
        int depth;

        int totalVerts;
        IntPtr bufferSize;

        Vector3[] vertices;
        Vector3[] colours;
        Vector3[] normals;

        static Dictionary<string, TriData> datas = new Dictionary<string, TriData>();

        TriData data { get { return datas[key]; } }

        string key;

        bool isControl = false;

        Vector2[] boundingPoints = new Vector2[] { new Vector2(1, -1), new Vector2(-1, -1), new Vector2(-1, 1) };

        Vector3 colourBase;


        public EquationTriangle(Vector3 pos, Vector3 scale, Quaternion rotation, Vector3 colourBase, Expression<Func<float, float, float, float>> equation, int depth = 1) : 
            base(pos, scale, rotation)
        {
            key = equation.ToString() + colourBase.ToString() + depth.ToString();

            this.equation = equation.Compile();
            this.depth = depth;

            totalVerts = (1<<depth) * 3;
            bufferSize = (IntPtr) (totalVerts * Vector3.SizeInBytes);

            vertices = new Vector3[totalVerts];
            colours = new Vector3[totalVerts];
            normals = new Vector3[totalVerts];

            this.colourBase = colourBase;

            if(!datas.ContainsKey(key))
            {
                isControl = true;
                datas[key] = new TriData
                {
                    verticesID = GL.GenBuffer(),
                    coloursID = GL.GenBuffer(),
                    normalsID = GL.GenBuffer()
                };
            }
        }

        protected override void Draw() 
        {
            var start = new Stopwatch();
            start.Start();
            
            GL.BindBuffer(BufferTarget.ArrayBuffer, data.verticesID);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 0, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, data.coloursID);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 0, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, data.normalsID);
            GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, 0, 0);

            GL.DrawArrays(PrimitiveType.Triangles, 0, totalVerts);
            
            Game.times[4].time += start.Elapsed;
        }

        //left inclusive, right exclusive
        void CalculatePoints(ref Vector2 corner1, ref Vector2 corner2, ref Vector2 corner3, int left, int right, float mod)
       { open
            if(right - left == 3*(1<<depth))
            {
                var start = new Stopwatch();
                start.Start();

                var a = SetVertex(corner1, mod, left);
                var b = SetVertex(corner2, mod, left + 1);
                var c = SetVertex(corner3, mod, left + 2);

                Game.times[1].time += start.Elapsed;

                var normal = Vector3.Cross(c - a, b - a).Normalized();
                //force forward vector
                if(Vector3.Dot(normal, Vector3.UnitY) < 0)
                {
                    normal *= -1;
                }

                for(var i = 0; i < 3; i++)
                {
                    normals[left + i] = normal;
                }
            }
            else
            {
                Vector2 median;
                Vector2 middle;
                Vector2 tri1;
                Vector2 tri2;

                var start = new Stopwatch();
                start.Start();

                SplitTriangle(ref corner1, ref corner2, ref corner3, out median, out  middle, out tri1, out tri2);

                Game.times[2].time += start.Elapsed;

                int mid = left + (right-left) / 2;

                CalculatePoints(ref tri1, ref middle, ref median, left, mid, mod);
                CalculatePoints(ref tri2, ref middle, ref median, mid, right, mod);
            }
        }

        Vector3 SetVertex(Vector2 point, float mod, int i)
        {
            float y = equation(point.X, point.Y, mod);
            Vector3 pos = new Vector3(point.X, y, point.Y);

            vertices[i] = pos;
            colours[i] = colourBase;

            return pos;
        }

        void SplitTriangle(ref Vector2 corner1, ref Vector2 corner2, ref Vector2 corner3,
            out Vector2 median, out Vector2 middle, out Vector2 newCorner1, out Vector2 newCorner2)
        {
            Vector2[] diff = new Vector2[] { (corner2 - corner1), (corner3 - corner2), (corner1 - corner3) };
            Vector2[] verts = new Vector2[] { corner1, corner2, corner3 };
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

        public override void Buffer(float mod)
        {
            if(isControl)
            {
                var start = new Stopwatch();
                start.Start();

                CalculatePoints(ref boundingPoints[0], ref boundingPoints[1], ref boundingPoints[2], 0, totalVerts, mod);

                Game.times[0].time += start.Elapsed;

                var start1 = new Stopwatch();
                start1.Start();

                //vertices
                GL.BindBuffer(BufferTarget.ArrayBuffer, data.verticesID);
                GL.BufferData(BufferTarget.ArrayBuffer, bufferSize, vertices, BufferUsageHint.DynamicDraw);

                GL.EnableVertexAttribArray(1);

                //colours
                GL.BindBuffer(BufferTarget.ArrayBuffer, data.coloursID);
                GL.BufferData(BufferTarget.ArrayBuffer, bufferSize, colours, BufferUsageHint.DynamicDraw);

                GL.EnableVertexAttribArray(2);

                //normals
                GL.BindBuffer(BufferTarget.ArrayBuffer, data.normalsID);
                GL.BufferData(BufferTarget.ArrayBuffer, bufferSize, normals, BufferUsageHint.DynamicDraw);

                GL.EnableVertexAttribArray(3);

                Game.times[3].time += start1.Elapsed;
            }
        }
    }
}
