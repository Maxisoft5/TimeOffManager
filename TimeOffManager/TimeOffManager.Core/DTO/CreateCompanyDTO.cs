using Microsoft.AspNetCore.Http;

namespace TimeOffManager.Core.DTO
{
    public struct CreateCompanyDTO
    {
        public string Name { get; set; }
        public IFormFile Logo { get; set; }
    }
}
