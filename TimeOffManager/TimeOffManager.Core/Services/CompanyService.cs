using Microsoft.EntityFrameworkCore;
using TimeOffManager.Core.Services.Interfaces;
using TimeOffManager.DataAccess.Context;
using TimeOffManager.DataAccess.Models;

namespace TimeOffManager.Core.Services
{
    public class CompanyService(DataContext dataContext, ITeamService teamService) : ICompanyService
    {
        private readonly DataContext _dataContext = dataContext;
        private readonly ITeamService _teamService = teamService;

        public async Task<Manager> AssignCompanyToManager(int companyId, Manager manager)
        {
            var defaultTeam = await _teamService.CreateDefaultTeamForManager(companyId);
            manager.TeamId = defaultTeam.Id;
            await _dataContext.SaveChangesAsync();
            return manager;
        }

        public async Task<Company> CreateCompany(Company company)
        {
            await _dataContext.Companies.AddAsync(company);
            await _dataContext.SaveChangesAsync();
            return company;
        }

        public async Task<IEnumerable<Team>> GetAllTeamsFromCompany(int companyId)
        {
            var teams = await _dataContext.Teams.Where(x => x.CompanyId == companyId).ToListAsync();
            return teams;
        }

        public async Task<IEnumerable<User>> GetAllUsersFromCompany(int companyId)
        {
            var company = await _dataContext.Companies
                .Include(x => x.Teams).ThenInclude(x => x.Users)
                .FirstOrDefaultAsync(x => x.Id == companyId);

            return company.Teams.SelectMany(x => x.Users);
        }
    }
}
