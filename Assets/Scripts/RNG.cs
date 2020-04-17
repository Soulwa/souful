using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace soulful
{
    //need only 1 instance of system.random class in the project- all should refer to RNG
    public static class RNG
    {
        public static Random rng = new System.Random();

        public static void Shuffle<T>(this IList<T> lst)
        {
            int n = lst.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = lst[k];
                lst[k] = lst[n];
                lst[n] = value;
            }
        }
    }
}
