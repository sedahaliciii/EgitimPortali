using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using EgitimPortaliAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace EgitimPortaliAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(model.Username);
                if (user == null)
                {
                    return Unauthorized(new { message = "Kullanıcı adı veya şifre hatalı" });
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
                if (!result.Succeeded)
                {
                    return Unauthorized(new { message = "Kullanıcı adı veya şifre hatalı" });
                }

                var userRoles = await _userManager.GetRolesAsync(user);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                foreach (var userRole in userRoles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: _configuration["JWT:Issuer"],
                    audience: _configuration["JWT:Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddHours(3),
                    signingCredentials: creds);

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo,
                    roles = userRoles
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed");
                return StatusCode(500, new { message = "Giriş sırasında sunucu hatası", error = ex.Message });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            try
            {
                if (await _userManager.FindByNameAsync(model.Username) != null)
                {
                    return BadRequest(new { message = "Bu kullanıcı adı zaten kullanımda" });
                }

                if (await _userManager.FindByEmailAsync(model.Email) != null)
                {
                    return BadRequest(new { message = "Bu e-posta adresi zaten kullanımda" });
                }

                var user = new ApplicationUser
                {
                    UserName = model.Username,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    RegistrationDate = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (!result.Succeeded)
                {
                    return BadRequest(new { message = "Kullanıcı oluşturulamadı", errors = result.Errors });
                }

                // Varsayılan olarak "User" rolüne ata
                await _userManager.AddToRoleAsync(user, "User");

                return Ok(new { message = "Kullanıcı başarıyla oluşturuldu" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration failed");
                return StatusCode(500, new { message = "Kayıt sırasında sunucu hatası", error = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetUserProfile()
        {
            try
            {
                var user = await _userManager.FindByNameAsync(User.Identity.Name);
                if (user == null)
                {
                    return NotFound(new { message = "Kullanıcı bulunamadı" });
                }

                var roles = await _userManager.GetRolesAsync(user);

                return Ok(new
                {
                    userId = user.Id,
                    userName = user.UserName,
                    email = user.Email,
                    firstName = user.FirstName ?? "",
                    lastName = user.LastName ?? "",
                    phoneNumber = user.PhoneNumber ?? "",
                    registrationDate = user.RegistrationDate,
                    roles = roles
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user profile");
                return StatusCode(500, new { message = "Profil bilgileri alınırken sunucu hatası", error = ex.Message });
            }
        }

        [Authorize]
        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileModel model)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(User.Identity.Name);
                if (user == null)
                {
                    return NotFound(new { message = "Kullanıcı bulunamadı" });
                }

                // Email değişikliği yapılıyorsa ve yeni email başka bir kullanıcı tarafından kullanılıyorsa
                if (user.Email != model.Email)
                {
                    var existingUser = await _userManager.FindByEmailAsync(model.Email);
                    if (existingUser != null && existingUser.Id != user.Id)
                    {
                        return BadRequest(new { message = "Bu e-posta adresi zaten kullanımda" });
                    }
                }

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Email = model.Email;
                user.PhoneNumber = model.PhoneNumber;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    return BadRequest(new { message = "Profil güncellenemedi", errors = result.Errors });
                }

                return Ok(new { message = "Profil başarıyla güncellendi" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile");
                return StatusCode(500, new { message = "Profil güncellenirken sunucu hatası", error = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(User.Identity.Name);
                if (user == null)
                {
                    return NotFound(new { message = "Kullanıcı bulunamadı" });
                }

                var result = await _userManager.ChangePasswordAsync(
                    user, model.CurrentPassword, model.NewPassword);

                if (!result.Succeeded)
                {
                    return BadRequest(new { message = "Şifre değiştirilemedi", errors = result.Errors });
                }

                return Ok(new { message = "Şifre başarıyla değiştirildi" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return StatusCode(500, new { message = "Şifre değiştirilirken sunucu hatası", error = ex.Message });
            }
        }
    }
}