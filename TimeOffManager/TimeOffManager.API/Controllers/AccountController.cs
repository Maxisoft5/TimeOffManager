using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TimeOffManager.Core.DTO;
using TimeOffManager.Core.Services.Interfaces;
using TimeOffManager.DataAccess.Models;

namespace TimeOffManager.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly ITeamService _teamsService;
        private readonly ICompanyService _companyService;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IOptions<JwtOptions> _jwtOptions;

        public AccountController(IAccountService accountService, IHttpContextAccessor contextAccessor,
            ICompanyService companyService,
            ITeamService teamService,
            IOptions<JwtOptions> options)
        {
            _jwtOptions = options;
            _teamsService = teamService;
            _companyService = companyService;
            _accountService = accountService;
            _contextAccessor = contextAccessor;
        }

        [AllowAnonymous]
        [HttpGet("get-authorized")]
        public async Task<IActionResult> GetAuthorizedCompany([FromQuery] bool withCompany)
        {
            var signined = await _accountService.GetIfAuthorized(_contextAccessor.HttpContext.User, withCompany);
            if (signined != null)
            {
                var teams = await _companyService.GetAllTeamsFromCompany(signined.Team.CompanyId);
                var teamsDto = new List<Team>();
                foreach (var team in teams)
                {
                    teamsDto.Add(new Team()
                    {
                        Name = team.Name,
                        Id = team.Id
                    });
                }
                signined.Team = new DataAccess.Models.Team()
                {
                    CompanyId = signined.Team.CompanyId,
                    Company = new DataAccess.Models.Company()
                    {
                        Name = signined.Team.Company.Name,
                        LogoBase64 = signined.Team.Company.Logo == null ? "" :
                            Convert.ToBase64String(signined.Team.Company.Logo),
                        Teams = teamsDto
                    },
                    Name = signined.Team.Name
                };
                var user = await _accountService.GetIdentity(_contextAccessor.HttpContext.User);
                if (user as Manager != null && await _accountService.IsOwner(user as Manager))
                {
                    signined.RoleName = "Owner";
                }
            }

            return Ok(signined);
        }

        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> GenerateTokenAsync([FromBody] Token token)
        {
            var refreshed = await _accountService.RefreshToken(token.RefreshToken);
            return Ok(refreshed);
        }

        [AllowAnonymous]
        [HttpPost("login-to-company-workspace")]
        public async Task<IActionResult> Login([FromBody] LoginToCompanyDTO loginToCompany)
        {
            var result = await _accountService.SignIn(loginToCompany.Email, loginToCompany.Password);

            if (result.Success)
            {
                return Ok(result);
            }

            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("sign-up")]
        public async Task<IActionResult> SignUp([FromBody] SignUpDTO singUp)
        {
            var res = await _accountService.SignUpOwner(singUp.Email, singUp.Password, singUp.FirstName, singUp.LastName,
                singUp.BirthDate, singUp.JobTitle ?? "");

            return Ok(res);
        }

        [AllowAnonymous]
        [HttpGet("is-invite-valid")]
        public async Task<IActionResult> IsInviteValid([FromQuery] string emeil)
        {
            var isValid = await _accountService.IsInviteValid(emeil);
            return Ok(isValid);
        }

        [AllowAnonymous]
        [HttpPost("accept-invite")] 
        public async Task<IActionResult> AcceptInvite([FromBody] AcceptInviteDTO acceptInvite)
        {
            var res = await _teamsService.AcceptInvite(acceptInvite);
            return Ok(res);
        }

        [HttpPost("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDTO updateProfile)
        {
            var user = await _accountService.GetIfAuthorized(_contextAccessor.HttpContext.User);
            var res = await _accountService.UpdateProfile(updateProfile, user);
            return Ok(res);
        }

        [HttpPost("sign-out")]
        public async Task<IActionResult> SignOut()
        {
            var user = await _accountService.GetIfAuthorized(_contextAccessor.HttpContext.User);
            if (user is null)
            {
                return Unauthorized();
            }
            await _accountService.SignOut();
            await _contextAccessor.HttpContext.SignOutAsync(JwtBearerDefaults.AuthenticationScheme);

            return Ok();
        }

        [Authorize]
        [HttpPatch("update-allowance-timeoff")]
        public async Task<IActionResult> UpdateAllowanceTimeOff(UpdateAllowanceTimeOffDTO timeOffDTO)
        {
            await _accountService.UpdateAllowanceTimeOffForUser(timeOffDTO.UserId, timeOffDTO.TotalTimeOff,
                timeOffDTO.TookTimeOff);

            return Ok();
        }
    }
}
