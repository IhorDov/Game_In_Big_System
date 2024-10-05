using LoginApi.Models;
using LoginApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LoginApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class UserAuthController : ControllerBase
    {
        private readonly UserService _userService;

        public UserAuthController(UserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserDto userDto)
        {
            try
            {
                await _userService.RegisterUser(userDto);
                return Ok("User registered successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserDto userDto)
        {
            var result = await _userService.LoginUser(userDto);
            return Ok(result);
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var result = await _userService.GetAllUsers();
            return Ok(result);
        }
    }
}
