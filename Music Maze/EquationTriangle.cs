using OpenTK;
//using OpenTK.Graphics.OpenGL4;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;


using System.Diagnostics;

namespace Music_Maze
{
    class EquationTriangle : GameObject
    {
        Func<float, float, float, float> equation;
        int depth;

        int totalVerts;

        List<Vector3> vertices;
        int verticesID;

        uint[] indices;
        uint indicesID;

        List<Vector3> colours;
        int coloursID;

        List<Vector3> normals;
        int normalsID;

        Vector3[] boundingPoints;

        Vector3 colourBase;


        public EquationTriangle(Vector3 pos, Vector3 scale, Quaternion rotation, Vector3 colourBase, Func<float, float, float, float> equation, int depth = 1) : 
            base(pos, scale, rotation)
        {
            boundingPoints = new Vector3[] { new Vector3(1, 0, -1), new Vector3(-1, 0, -1), new Vector3(-1, 0, 1) };

            this.equation = equation;
            this.depth = depth;

            totalVerts = (int)Math.Pow(2, depth) * 3;

            vertices = new List<Vector3>();
            indices = new uint[totalVerts];
            colours = new List<Vector3>();
            normals = new List<Vector3>();

            this.colourBase = colourBase;

            verticesID = GL.GenBuffer();
            coloursID = GL.GenBuffer();
            normalsID = GL.GenBuffer();
            GL.GenBuffers(1, out indicesID);
        }

        protected override void Draw() 
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, verticesID);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 0, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, coloursID);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 0, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, normalsID);
            GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, 0, 0);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indicesID);
            GL.DrawElements(BeginMode.Triangles, totalVerts, DrawElementsType.UnsignedInt, 0);
        }

        //left inclusive, right exclusive
        void CalculatePoints(ref Vector3 corner1, ref Vector3 corner2, ref Vector3 corner3, int left, int right, float mod)
        {
            if(left == right-3)
            {
                var a = SetVertex(corner1, mod, left);
                var b = SetVertex(corner2, mod, left + 1);
                var c = SetVertex(corner3, mod, left + 2);

                var normal = Vector3.Cross(c - a, b - a).Normalized();
                //force forward vector
                if(Vector3.Dot(normal, Vector3.UnitY) < 0)
                {
                    normal *= -1;
                }

                for(var i = 0; i < 3; i++)
                {
                    var normalsIndex = (int)indices[left + i];

                    if (normalsIndex < normals.Count)
                    {
                        var q = Vector3.Add(normals[normalsIndex], normal);
                        var z = a / 2;
                        normals[normalsIndex] = (normals[normalsIndex] + normal) / 2;
                    }
                    else
                    {
                        normals.Add(normal);
                    }
                }
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

        Vector3 SetVertex(Vector3 point, float mod, int i)
        {
            float y = equation(point.X, point.Z, mod);
            Vector3 pos = new Vector3(point.X, point.Y + y, point.Z);

            float colourMod = (float)( -0.5f * Math.Cos(y * Math.PI) + 0.55 );

            uint id = (uint)vertices.IndexOf(pos);
            if(id == uint.MaxValue)
            {
                vertices.Add(pos);
                colours.Add(colourBase * colourMod);
                id = (uint)vertices.Count - 1;
            }

            indices[i] = id;

            return pos;
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

        public override void Buffer(float mod)
        {
            CalculatePoints(ref boundingPoints[0], ref boundingPoints[1], ref boundingPoints[2], 0, totalVerts, mod);

            //vertices
            GL.BindBuffer(BufferTarget.ArrayBuffer, verticesID);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Count * Vector3.SizeInBytes), vertices.ToArray(), BufferUsageHint.DynamicDraw);

            GL.EnableVertexAttribArray(1);
            
            //colours
            GL.BindBuffer(BufferTarget.ArrayBuffer, coloursID);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(colours.Count * Vector3.SizeInBytes), colours.ToArray(), BufferUsageHint.DynamicDraw);

            GL.EnableVertexAttribArray(2);

            //normals
            GL.BindBuffer(BufferTarget.ArrayBuffer, normalsID);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(normals.Count * Vector3.SizeInBytes), normals.ToArray(), BufferUsageHint.DynamicDraw);

            GL.EnableVertexAttribArray(3);

            //indices
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indicesID);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(totalVerts * sizeof(uint)), indices, BufferUsageHint.DynamicDraw);

            //empty the lists
            vertices.Clear();
            colours.Clear();
            normals.Clear();
            indices = new uint[totalVerts];
        }
    }
}
