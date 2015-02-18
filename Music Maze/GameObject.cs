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
        public Matrix4 matrix;
        protected List<GameObject> children;

        protected GameObject(Vector3 pos, Vector3 scale, Quaternion rotation)
        {
            matrix = Helper.Create(pos, scale, rotation);
            children = new List<GameObject>();
        }

        public void Render(FrameEventArgs e, ref Matrix4 matrix)
        {
            var internalMatrix = this.matrix * matrix;

            GL.UniformMatrix4(Game.modelMatrixID, false, ref internalMatrix);

            Draw();

            foreach (var child in children)
            {
                child.Render(e, ref internalMatrix);

                Game.GetError();
            }

            GL.UniformMatrix4(Game.modelMatrixID, false, ref matrix);
        }

        protected virtual void Draw()
        {

        }

        public virtual void Buffer(float mod)
        {
            foreach (var child in children)
            {
                child.Buffer(mod);

                Game.GetError();
            }
        }
    }
}
