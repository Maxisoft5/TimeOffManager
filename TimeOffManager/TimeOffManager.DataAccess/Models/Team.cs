namespace TimeOffManager.DataAccess.Models
{
    public class Team
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public IEnumerable<User> Users { get; set; }
        public int CompanyId { get; set; }
        public Company Company { get; set; }
    }
}
