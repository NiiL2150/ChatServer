using ChatServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace ChatServer.Controllers
{
    public class JwtController : Controller
    {
        private IConfiguration _config;
        private List<User> Users { get; set; }

        public JwtController(IConfiguration configuration, List<User> users)
        {
            _config = configuration;
            Users = users;
        }

        [HttpPost]
        public IActionResult Registration([FromBody] User user)
        {
            User? user2 = Users.Find(u => u.Username == user.Username);
            if (user2 != null)
            {
                return Json(new { error = "User already exists" });
            }
            Users.Add(user);
            return Json(new { error = $"No error" });
        }

        [HttpPost]
        public IActionResult Login([FromBody] User user)
        {
            User? user2 = Users.Find(u => u.Username == user.Username);
            if (user2 == null)
            {
                return Json(new { error = $"Invalid username {user.Username} {Users.Count}" });
            }
            if (user2.Password != user.Password)
            {
                return Json(new { error = "Invalid password" });
            }
            return GenerateJwtToken(user2);
        }

        private IActionResult GenerateJwtToken(User user)
        {
            var identity = GetIdentity(user);
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: identity.Claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: credentials);

            var token2 = new JwtSecurityTokenHandler().WriteToken(token);
            var obj = new
            {
                token = token2
            };
            return Json(obj);
        }

        private ClaimsIdentity GetIdentity(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.Username),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, "User")
            };
            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            return claimsIdentity;
        }
    }
}
