using System;

namespace OrigoDB.Core.Types
{
    [Serializable]
    public class Range<T> : IComparable<Range<T>> where T : IComparable<T>
    {
        public readonly T Start;
        public readonly T End;

        public Range(T from, T to)
        {
            if (from.CompareTo(to) > 0) throw new InvalidOperationException();
            Start = from;
            End = to;
        }

        public bool Contains(T value)
        {
            return Start.CompareTo(value) <= 0 && End.CompareTo(value) >= 0;
        }

        public bool Precedes(T value)
        {
            return End.CompareTo(value) < 0;
        }

        public bool Succeeds(T value)
        {
            return Start.CompareTo(value) > 0;
        }

        public bool Precedes(Range<T> other)
        {
            return Precedes(other.Start);
        }

        public bool Succeeds(Range<T> other)
        {
            return Succeeds(other.End);
        }

        public bool Overlaps(Range<T> other)
        {
            return !(End.CompareTo(other.Start) < 0 || Start.CompareTo(other.End) > 0);
        }

        public Range<T> Intersect(Range<T> other)
        {
            if (!Overlaps(other)) throw new InvalidOperationException("Ranges don't overlap");
            return new Range<T>(Max(Start, other.Start), Min(End,other.End));
        }

        public Range<T> Union(Range<T> other)
        {
            if (!Overlaps(other)) throw new InvalidOperationException("Ranges don't overlap");
            return new Range<T>(Min(Start, other.Start), Max(End, other.End));
        }

        public static T Min(T a, T b)
        {
            return a.CompareTo(b) < 0 ? a : b;
        }

        public static T Max(T a, T b)
        {
            return a.CompareTo(b) > 0 ? a : b;
        }

        public int CompareTo(Range<T> other)
        {
            return Start.CompareTo(other.Start);
        }
    }
}