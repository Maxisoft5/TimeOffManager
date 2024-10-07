namespace TimeOffManager.DataAccess.Models
{
    public class Employee : User
    {
        public IEnumerable<TimeOff> TimeOffs { get; set; }
    }
}
