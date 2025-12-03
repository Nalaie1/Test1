    using System.Security.Claims;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using WebApplication1.Application.DTOs;
    using WebApplication1.Application.Interfaces.Jwt;
    using WebApplication1.Application.Services;

    namespace WebApplication1.Presentation.Controllers;

    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;

        public AuthController(IAuthService auth)
        {
            _auth = auth;
        }

        // POST: api/auth/login
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            var res = await _auth.LoginAsync(dto.Username, dto.Password);
            if (res == null) return Unauthorized();
            return Ok(res);
        }

        // POST: api/auth/refresh
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh([FromBody] string refreshToken)
        {
            var res = await _auth.RefreshAsync(refreshToken);
            if (res == null) return Unauthorized();
            return Ok(res);
        }

        // POST: api/auth/logout
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(idStr, out var id))
            {
                await _auth.LogoutAsync(id);
                return Ok();
            }
            return BadRequest();
        }
    }