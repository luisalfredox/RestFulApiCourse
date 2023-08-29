﻿using course.Data;
using course.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace course.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        ApiDbContext _db = new ApiDbContext();
        private IConfiguration _config;

        public UsersController(IConfiguration config)
        {
            _config = config;
        }


        [HttpPost("[action]")]
        public IActionResult Register([FromBody] User user)
        {
            var userExists = _db.Users.FirstOrDefault(x => x.Email == user.Email);
            if(userExists!=null)
            {
                return BadRequest("User with same email already exists");
            }    
            _db.Users.Add(user);
            _db.SaveChanges();
            return new ObjectResult(user) { StatusCode = StatusCodes.Status201Created };


        }
        [HttpPost("[action]")]
        public IActionResult Login([FromBody] User user)
        {
            var currentUser = _db.Users.FirstOrDefault(u => u.Email == user.Email && u.Password == user.Password);
            if(currentUser == null)
            {
                return NotFound();
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(ClaimTypes.Email, user.Email),  
            };
            var token = new JwtSecurityToken(
                issuer: _config["JWT:Issuer"],
                audience: _config["JWT:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(60),
                signingCredentials: credentials);
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return Ok(jwt);
        }
    }
}
