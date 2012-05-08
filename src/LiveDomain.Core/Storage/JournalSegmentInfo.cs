using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDomain.Core
{
    public class JournalSegmentInfo
    {
        public readonly int SequenceNumber;
        public static readonly JournalSegmentInfo Initial = new JournalSegmentInfo(1);

        public JournalSegmentInfo(int sequenceNumber)
        {
            SequenceNumber = sequenceNumber;
        }
    }

}
