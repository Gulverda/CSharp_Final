using LibraryManagement.Application.Contracts.Infrastructure;
using LibraryManagement.Application.DTOs.AuthDtos;
using LibraryManagement.Infrastructure.Identity;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace LibraryManagement.Api.Controllers
{
    [RoutePrefix("api/auth")]
    public class AuthController : ApiController
    {
        private AppUserManager UserManager => Request.GetOwinContext().GetUserManager<AppUserManager>();

        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public AuthController(IJwtTokenGenerator jwtTokenGenerator)
        {
            _jwtTokenGenerator = jwtTokenGenerator ?? throw new ArgumentNullException(nameof(jwtTokenGenerator));
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <param name="model">User registration data.</param>
        [HttpPost]
        [Route("register")]
        [AllowAnonymous]
        [ResponseType(typeof(string))]
        public async Task<IHttpActionResult> Register(UserRegistrationDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new AppUser { UserName = model.Email, Email = model.Email };
            IdentityResult result = await UserManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                return GetErrorResult(result);

            // Assign the "User" role by default
            await UserManager.AddToRoleAsync(user.Id, Roles.User);

            return Ok("User registered successfully.");
        }

        /// <summary>
        /// Authenticates user and returns JWT token.
        /// </summary>
        /// <param name="model">User login data.</param>
        [HttpPost]
        [Route("login")]
        [AllowAnonymous]
        [ResponseType(typeof(TokenResponseDto))]
        public async Task<IHttpActionResult> Login(UserLoginDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            AppUser user = await UserManager.FindAsync(model.Email, model.Password);

            if (user == null)
                return Unauthorized();

            var tokenString = await _jwtTokenGenerator.GenerateTokenAsync(user.Id);
            var userRoles = await UserManager.GetRolesAsync(user.Id);

            var tokenResponse = new TokenResponseDto
            {
                Token = tokenString,
                Email = user.Email,
                Roles = userRoles,
                Expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(ConfigurationManager.AppSettings["jwt:DurationInMinutes"]))
            };

            return Ok(tokenResponse);
        }

        /// <summary>
        /// Handles IdentityResult errors.
        /// </summary>
        /// <param name="result">IdentityResult from user operations.</param>
        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
                return InternalServerError();

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error);
                    }
                }

                if (ModelState.IsValid) // No errors added, return generic message
                    return BadRequest("User operation failed. Please check the details.");

                return BadRequest(ModelState);
            }

            return null;
        }
    }
}
