using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ezvm.Assembler
{
    public class CharCombinator
    {
        public BigInteger TotalStrings => BigInteger.MinusOne;

        private string alphabet;

        public CharCombinator(string alphabet)
        {
            this.alphabet = new string(alphabet.Distinct().ToArray());
        }

        // Treat alphabet like a numbering system; simply translate to that system
        private string StepToString(BigInteger step)
        {
            var b = new StringBuilder();
            var vbase = alphabet.Length;
            while (step > 0)
            {
                var remainder = step % vbase;
                b.Append(alphabet[(int)remainder]);
                step /= vbase;
            }
            return b.ToString();
        }

        public IEnumerable<string> GetStrings()
        {
            if (alphabet.Length > 1)
            {
                BigInteger step = 1;
                yield return alphabet.Substring(0, 1);
                while (true)
                    yield return StepToString(step++);
            }
            else
            {
                var b = new StringBuilder();
                while (true)
                {
                    b.Append(alphabet[0]);
                    yield return b.ToString();
                }
            }
        }
    }
}
