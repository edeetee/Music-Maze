using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Music_Maze
{
    interface IVBO
    {
        void Render(FrameEventArgs e);
        void Buffer();
    }
}
