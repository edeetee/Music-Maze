using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System.Linq.Expressions;

namespace Music_Maze
{
    class EquationRectangle : GameObject
    {
        public EquationRectangle(Vector3 pos, Vector3 scale, Quaternion rotation, int depth, Expression<Func<float, float, float, float>> equation, Vector3 colour) : 
            base(pos, scale, rotation)
        {
            children = new List<GameObject>(){
                new EquationTriangle(Vector3.Zero, Vector3.One, Quaternion.Identity, colour, equation, depth),
                new EquationTriangle(Vector3.Zero, new Vector3(1, 1, 1), Quaternion.FromAxisAngle(Vector3.UnitY, -(float)Math.PI), colour, equation, depth)
            };
        }
    }
}