namespace TimeOffManager.Core.DTO
{
    public struct AcceptInviteDTO
    {
        public string ResetToken {  get; set; }
        public string Email { get; set; }
        public string NewPassword { get; set; }
        public DateTime BirthDate { get; set; }
        public string JobTitle { get; set; }
    }
}
