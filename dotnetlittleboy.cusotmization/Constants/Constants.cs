namespace dotnetlittleboy.cusotmization.Constants
{
    /// <summary>
    /// Defines the <see cref="Constants" />.
    /// </summary>
    internal class Constants
    {
        public enum Semester
        {
            Semester1 = 990000000,
            Semester2 = 990000001,
            Semester3 = 990000002,
            Semester4 = 990000004,
            Semester5 = 990000005,
            Semester6 = 990000006,
            Semester7 = 990000007,
            Semester8 = 990000008
        }

        public enum Cycle
        {
            PCycle = 990000000,
            CCycle = 990000001,
            Both = 990000002
        }
        public enum ApplicationStatus
        {
            Applied= 990000000,
            InProgress=990000001,
            Approved = 990000002,
            Pending = 990000003,
            RequiredInformation = 990000004
        }
    }
}
