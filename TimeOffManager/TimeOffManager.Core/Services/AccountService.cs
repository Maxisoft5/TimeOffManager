using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TimeOffManager.Core.DTO;
using TimeOffManager.Core.Services.Interfaces;
using TimeOffManager.Core.Settings;
using TimeOffManager.DataAccess;
using TimeOffManager.DataAccess.Context;
using TimeOffManager.DataAccess.Models;

namespace TimeOffManager.Core.Services
{
    public class AccountService(
        UserManager<User> usersService,
        SignInManager<User> signInManager,
        RoleManager<ManagerRole> roleManager,
        IOptions<JwtOptions> options,
        IEmailService emailService,
        IHttpContextAccessor accessor,
        DataContext dataContext) : IAccountService
    {
        private readonly SignInManager<User> _signInManager = signInManager;
        private readonly UserManager<User> _userManager = usersService;
        private readonly DataContext _dataContext = dataContext;
        private readonly RoleManager<ManagerRole> _roleManager = roleManager;
        private readonly IHttpContextAccessor _httpContextAccessor = accessor;
        private readonly IEmailService _emailService = emailService;
        private readonly IOptions<JwtOptions> _jwtOptions = options;

        public async Task<User?> GetIfAuthorized(ClaimsPrincipal currentUser, bool withCompany = false)
        {
            if (currentUser != null &&
              currentUser.Identity != null)
            {
                var idenitity = await GetIdentity(currentUser);
                if (idenitity != null)
                {
                    var users = _dataContext.Users.AsQueryable();
                    if (withCompany)
                    {
                        users = users.Include(x => x.Team).ThenInclude(x => x.Company);
                    }
                    var user = await users.FirstOrDefaultAsync(x => x.Id == idenitity.Id);
                    if (user != null)
                    {
                        return user;
                    }
                }
            }

            return null;
        }

        public async Task<User?> GetIdentity(ClaimsPrincipal currentUser)
        {
            if (currentUser == null)
            {
                return null;
            }

            return await _userManager.GetUserAsync(currentUser);
        }

        public async Task<Token> RefreshToken(string token)
        {
            var principal = GetPrincipal(token, isAccessToken: false);
            var identity = await GetIdentity(principal);
            var manager = new JwtManager(options);
            var newToken = manager.GenerateToken(identity.Id);
            return newToken;
        }

        public ClaimsPrincipal GetPrincipal(string token, bool isAccessToken = true)
        {
            var key = new SymmetricSecurityKey(isAccessToken ? _jwtOptions.Value.AccessSecret : _jwtOptions.Value.RefreshSecret);

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = _jwtOptions.Value.Issuer,
                ValidAudience = _jwtOptions.Value.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                RequireExpirationTime = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }

        public async Task<DTO.SignInResult> SignIn(string username, string password)
        {
            var exists = await _userManager.FindByNameAsync(username);

            if (exists == null)
            {
                return DTO.SignInResult.Error("Provided email was not found as registered");
            }
            var checkPassword = await _signInManager.PasswordSignInAsync(exists, password, false, false);

            if (!checkPassword.Succeeded)
            {
                return DTO.SignInResult.Error("Password isn't correct");
            }

            var manager = new JwtManager(_jwtOptions);
            var token = manager.GenerateToken(exists.Id);

            return DTO.SignInResult.Ok(token);
        }

        public async Task<SignUpResult> SignUpOwner(string username, string password, string fName, string lName,
            DateTime birthDate, string jobTitle)
        {
            var exists = await _userManager.FindByEmailAsync(username);
            if (exists != null)
            {
                return SignUpResult.Error("Provided email has already registered");
            }

            var manager = new Manager()
            {
                UserName = username,
                Email = username,
                StartWorkDate = DateTime.UtcNow,
                BirthDate = birthDate,
                FirstName = fName,
                LastName = lName,
                JobTitle = jobTitle
            };
            var result = await _userManager.CreateAsync(manager);

            if (!result.Succeeded)
            {
                SignUpResult.Error(string.Join(',', result.Errors.Select(x => x.Description)));
            }

            var setPassword = await _userManager.AddPasswordAsync(manager, password);
            if (!setPassword.Succeeded)
            {
                return SignUpResult.Error(string.Join(',', setPassword.Errors.Select(x => x.Description)));
            }

            var ownerRole = await _dataContext.ManagerRoles.FirstOrDefaultAsync(x => x.Name == DbConstants.OwnerRoleName);
            var addRole = await _userManager.AddToRoleAsync(manager, ownerRole.Name);

            if (!addRole.Succeeded)
            {
                return SignUpResult.Error(string.Join(',', addRole.Errors.Select(x => x.Description)));
            }

            return SignUpResult.Ok();
        }

        public async Task<UpdateResult<User>> UpdateProfile(UpdateProfileDTO toUpdate, User user)
        {
            user.FirstName = toUpdate.FirstName;
            user.LastName = toUpdate.LastName;
            user.Email = toUpdate.Email;
            user.BirthDate = toUpdate.BirthDate;
            await _dataContext.SaveChangesAsync();
            return UpdateResult<User>.Ok(user);
        }

        public async Task<bool> IsOwner(Manager manager)
        {
            var isrole = await _userManager.IsInRoleAsync(manager, DbConstants.OwnerRoleName);
            return isrole;
        }

        public async Task SignOut()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task UpdateAllowanceTimeOffForUser(int userId, int totalTimeOff, int tookTimeOff)
        {
            var user = await _dataContext.Users.FindAsync(userId);
            user.TotalAllowanceTimeOffInYear = totalTimeOff;
            user.TookAllowanceTimeOff = tookTimeOff;
            await _dataContext.SaveChangesAsync();
        }

        public async Task SendInviteToTeam(User user, int teamId)
        {
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var link = $"http://localhost:5173/accept-invite?token={resetToken}&email={user.Email}&fName={user.FirstName}&lName={user.LastName}";
            var message = $"Follow the link to accept invite to the team. Link: {link}";
            await _emailService.Send(user.Email, SmtpSettings.From, message, "Accept invite to your team's workspace!");
        }

        public async Task<User> CreateDefaultUser(string fName, string lName, string email, int teamId)
        {
            var defaultPassword = "defaultPass12345$%==++===,";
            var user = new User()
            {
                UserName = email,
                FirstName = fName,
                LastName = lName,
                Email = email,
                InviteStatus = DataAccess.Enums.InviteStatus.WaitForAccept,
                TeamId = teamId,
                RoleName = "User"
            };

            await _userManager.CreateAsync(user, defaultPassword);

            var setPassword = await _userManager.AddPasswordAsync(user, defaultPassword);


            return user;
        }

        public async Task<bool> IsInviteValid(string email)
        {
            var user = await _dataContext.Users.FirstOrDefaultAsync(x => x.UserName == email);
            return user != null && user.TeamId != 0 && user.InviteStatus == DataAccess.Enums.InviteStatus.WaitForAccept; 
        }
    }
}
