using System;

namespace OrigoDB.Models.Redis
{
    [Serializable]
    public class ZSetEntry : IComparable<ZSetEntry>
    {
        public readonly double Score;
        public readonly string Member;

        public ZSetEntry(string member, double score)
        {
            Score = score;
            Member = member;
        }

        public int CompareTo(ZSetEntry other)
        {
            int result = Math.Sign(Score - other.Score);
            if (result == 0) result = String.Compare(Member, other.Member, StringComparison.InvariantCulture);
            return result;
        }

        public override bool Equals(object obj)
        {
            var other = obj as ZSetEntry;
            if (ReferenceEquals(other, null)) return false;
            return Member == other.Member;
        }

        public override string ToString()
        {
            return "[" + Member + ", " + Score + "]";
        }

        public override int GetHashCode()
        {
            return Member.GetHashCode();
        }

        internal ZSetEntry Increment(double increment)
        {
            return new ZSetEntry(Member, Score + increment);
        }
    }
}