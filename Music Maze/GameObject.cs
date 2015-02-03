using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Music_Maze
{
    abstract class GameObject
    {
        protected Matrix4 matrix;
        protected List<GameObject> children;

        protected GameObject(Vector3 pos, Vector3 scale, Quaternion rotation)
        {
            matrix = Helper.Create(pos, scale, rotation);
            children = new List<GameObject>();
        }

        public virtual void Render(FrameEventArgs e, int matrixID)
        {
            GL.UniformMatrix4(matrixID, true, ref matrix);

            foreach (var child in children)
            {
                child.Render(e, matrixID);
            }
        }

        public virtual void Buffer(float mod)
        {
            foreach (var child in children)
            {
                child.Buffer(mod);
            }
        }
    }
}
