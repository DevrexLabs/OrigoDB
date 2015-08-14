namespace OrigoDB.Core
{
    public class IsolationSettings
    {
        public CloneStrategy Commands { get; set; }
        public CloneStrategy Queries { get; set; }
        public CloneStrategy ReturnValues { get; set; }
    }
}