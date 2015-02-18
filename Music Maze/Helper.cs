using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Music_Maze
{
    public static class Helper
    {
        public static Matrix4 Create(Vector3 pos, Vector3 scale, Quaternion rotation)
        {
            return Matrix4.CreateFromQuaternion(rotation) * Matrix4.CreateScale(scale) * Matrix4.CreateTranslation(pos);
        }
    }
}
