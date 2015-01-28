using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;

namespace Music_Maze
{
    class Pyramid : IVBO
    {
        IVBO[] elements;

        public Pyramid(Vector3 top, Vector3 corner1, Vector3 corner2, Vector3 corner3, Vector3 corner4)
        {
            elements = new IVBO[] {
                new Triangle(top, corner1, corner2, new Vector3(1,0,0), new Vector3(1,1,1), new Vector3(0,0,0)),
                new Triangle(top, corner2, corner3, new Vector3(1,1,0), new Vector3(1,1,1), new Vector3(0,0,0)),
                new Triangle(top, corner3, corner4, new Vector3(0,1,0), new Vector3(1,1,1), new Vector3(0,0,0)),
                new Triangle(top, corner4, corner1, new Vector3(0,1,1), new Vector3(1,1,1), new Vector3(0,0,0))
            };
        }



        public void Render(FrameEventArgs e)
        {
            foreach(var element in elements)
            {
                element.Render(e);
            }
        }

        public void Buffer()
        {
            foreach (var element in elements)
            {
                element.Buffer();
            }
        }
    }
}
