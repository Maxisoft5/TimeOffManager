using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TimeOffManager.Core.DTO;
using TimeOffManager.Core.Services.Interfaces;
using TimeOffManager.DataAccess.Context;
using TimeOffManager.DataAccess.Models;

namespace TimeOffManager.Core.Services
{
    public class TeamService(DataContext dataContext,
        IAccountService accountService,
        UserManager<User> userManager) : ITeamService
    {
        private readonly DataContext _dataContext = dataContext;
        private readonly UserManager<User> _userManager = userManager;
        private readonly IAccountService _accountService = accountService;

        public async Task<TimeOffManager.Core.DTO.SignInResult> AcceptInvite(AcceptInviteDTO acceptInvite)
        {
            var user = await _dataContext.Users.FirstOrDefaultAsync(x => x.Email == acceptInvite.Email);
            var reset = await _userManager.ResetPasswordAsync(user, acceptInvite.ResetToken, acceptInvite.NewPassword);
            user.InviteStatus = DataAccess.Enums.InviteStatus.Accepted;
            user.JobTitle = acceptInvite.JobTitle;
            user.BirthDate = acceptInvite.BirthDate;
            user.StartWorkDate = DateTime.UtcNow;
            await _dataContext.SaveChangesAsync();
            var res = await _accountService.SignIn(user.UserName, acceptInvite.NewPassword);
            return res;
        }

        public async Task<Manager> AssignTeamToManager(Manager manager, int teamId)
        {
            manager.TeamId = teamId;
            await _dataContext.SaveChangesAsync();
            return manager;
        }

        public async Task<Team> CreateDefaultTeamForManager(int companyId)
        {
            var team = new Team()
            {
                CompanyId = companyId,
                Name = "Auto-created",
            };
            await _dataContext.Teams.AddAsync(team);
            await _dataContext.SaveChangesAsync();
            return team;
        }

        public async Task EditTeamName(int teamId, string teamName)
        {
            var team = await _dataContext.Teams.FindAsync(teamId);
            team.Name = teamName;
            await _dataContext.SaveChangesAsync();
        }

        public async Task<int> GetManagerIdByTeamId(int teamId)
        {
            var team = await _dataContext.Teams.Include(x => x.Users).FirstOrDefaultAsync(x => x.Id == teamId);
            var man = team.Users.FirstOrDefault(x => x is Manager);
            return man.Id;
        }

        public async Task RemoveFromTeam(int userId)
        {
            var user = await _dataContext.Users.Include(x => x.Team).FirstOrDefaultAsync(x => x.Id == userId);
            await _userManager.DeleteAsync(user);
            await _dataContext.SaveChangesAsync();
        }
    }
}
