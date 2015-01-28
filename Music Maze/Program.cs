using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Example
{
    class MyApplication
    {
        [STAThread]
        public static void Main()
        {
            using (var game = new Music_Maze.Game())
            {
                game.Run(60);
            }
        }
    }
}