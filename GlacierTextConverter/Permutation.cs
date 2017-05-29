using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlacierTextConverter
{
    class Permutation
    {
        private int elementLevel = -1;
        private int numberOfElements;
        private int[] permutationValue = new int[0];
        public int xorKey;

        private char[] inputSet;
        public char[] InputSet
        {
            get { return inputSet; }
            set { inputSet = value; }
        }

        private int permutationCount = 0;
        public int PermutationCount
        {
            get { return permutationCount; }
            set { permutationCount = value; }
        }

        public char[] MakeCharArray(string InputString, int xorkey)
        {
            xorKey = xorkey;
            char[] charString = InputString.ToCharArray();
            Array.Resize(ref permutationValue, charString.Length);
            numberOfElements = charString.Length;
            return charString;
        }

        public void CalcPermutation(int k)
        {
            elementLevel++;
            permutationValue.SetValue(elementLevel, k);

            if (elementLevel == numberOfElements)
            {
                OutputPermutation(permutationValue);
            }
            else
            {
                for (int i = 0; i < numberOfElements; i++)
                {
                    if (permutationValue[i] == 0)
                    {
                        CalcPermutation(i);
                    }
                }
            }
            elementLevel--;
            permutationValue.SetValue(0, k);
        }

        private void OutputPermutation(int[] value)
        {
            StringBuilder sb = new StringBuilder();
            foreach (int i in value)
            {
                sb.Append(inputSet.GetValue(i - 1));
            }
            byte byteValue = (byte)((Convert.ToByte(sb.ToString(), 2)) ^ (byte)xorKey);
            Console.Write(Char.ConvertFromUtf32(byteValue));

            PermutationCount++;
        }
    }
}
