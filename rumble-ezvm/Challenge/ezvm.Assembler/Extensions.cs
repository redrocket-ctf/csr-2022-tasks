using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezvm.Assembler
{
    public static class Extensions
    {
        public static void Shuffle<T>(this Random rng, T[] array)
        {
            int n = array.Length;
            while (n > 1)
            {
                int k = rng.Next(n--);
                var temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }
        }

        public static T[] Shuffle<T>(this T[] array, System.Random rng = null)
        {
            var copy = new T[array.Length];
            Array.Copy(array, copy, array.Length);
            if (rng == null) rng = new System.Random(Environment.TickCount);
            rng.Shuffle(copy);
            return copy;
        }

        public static bool GetBool(this Random rng)
        {
            return rng.Next(2) == 1;
        }

        /*public static string GetString(this Random rng, int minLength, int maxLength, string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789")
        {
            return GetString(rng, rng.Next(minLength, maxLength), alphabet);
        }


        public static string GetString(this Random rng, int length, string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789")
        {
            var str = new StringBuilder();
            for (var i = 0; i < length; i++)
            {
                str.Append(alphabet[rng.Next(alphabet.Length)]);
            }
            return str.ToString();
        }*/

        public static T Get<T>(this IEnumerator<T> e)
        {
            e.MoveNext();
            var val = e.Current;
            return val;
        }
    }
}
