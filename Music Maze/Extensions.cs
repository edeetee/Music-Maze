using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Music_Maze
{
    public static class Extensions
    {
        public static int Mod(this int val, int mod)
        {
            int modded = val % mod;
            return modded < 0 ? modded + mod : modded;
        }
    }
}
