using System;
using Andeart.ThreadJobs.Core;



namespace Andeart.ThreadJobs.Examples
{

    public class ArraySumResultArgs : EventArgs
    {

        public int Sum { get; private set; }

        public ArraySumResultArgs (int sum)
        {
            Sum = sum;
        }

    }


    public class ArraySumJob : ThreadJob<ArraySumResultArgs>
    {

        private readonly int[] _myInts;

        public int Sum { get; private set; }

        public ArraySumJob (int[] intArray)
        {
            _myInts = intArray;
        }

        protected override void Execute ()
        {
            Sum = 0;
            for (int i = 0; i < _myInts.Length; i++)
            {
                Sum += _myInts[i];
            }
        }

        protected override ArraySumResultArgs GetResultArgs ()
        {
            return new ArraySumResultArgs (Sum);
        }

    }

}