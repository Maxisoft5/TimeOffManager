namespace TimeOffManager.DataAccess.Models
{
    public class Manager : User
    {
        public IEnumerable<TimeOff> TimeOffsApprove { get; set; }
        public IEnumerable<TimeOff> PersonalTimeOffs { get; set; }
        public int ManagerRoleId { get; set; }
        public bool IsExecutive { get; set; }
    }
}
