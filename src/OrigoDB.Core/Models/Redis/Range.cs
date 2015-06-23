namespace OrigoDB.Models.Redis
{
    public class Range
    {
        public readonly int FirstIdx;
        public readonly int LastIdx;
        public readonly int Length;


        /// <summary>
        /// Create a range
        /// </summary>
        /// <param name="first">index of first element. if negative then relative to last element</param>
        /// <param name="last">index of last element. if negative then relative to last element</param>
        /// <param name="relativeTo">length of sequence to calculate when first/last are negative</param>
        public Range(int first, int last, int relativeTo = 0)
        {
            if (first < 0) first += relativeTo;
            FirstIdx = first;

            if (last < 0) last += relativeTo;
            LastIdx = last;

            Length = LastIdx - FirstIdx + 1;
        }

        public Range Flip(int relativeTo)
        {
            return new Range(relativeTo - LastIdx -1, relativeTo - FirstIdx - 1, relativeTo);
        }
    }
}