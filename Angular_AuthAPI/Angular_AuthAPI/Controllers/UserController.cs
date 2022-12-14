using Angular_AuthAPI.Context;
using Angular_AuthAPI.Helpers;
using Angular_AuthAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.RegularExpressions;

namespace Angular_AuthAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _authContext;
        public UserController(AppDbContext appDbContext)
        {
            _authContext = appDbContext;
        }

        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] User userObj)
        {
            if (userObj == null)
                return BadRequest();

            var user = await _authContext.Users.FirstOrDefaultAsync(x => x.UserName == userObj.UserName && x.Password == userObj.Password);
            if (user == null)
                return NotFound(new { Message = "User Not Found!" });

            return Ok(new
            {
                Message = "Login Sucess!"
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] User userObj)
        {
            if (userObj == null)
                return BadRequest();

            // Check Username
            if (await CheckUserNameExistAsync(userObj.UserName))
                return BadRequest(new { Message = "Username Already Exist!" });

            // Check Email
            if (await CheckEmailExistAsync(userObj.Email))
                return BadRequest(new { Message = "Email Already Exist!" });

            // Check password strength
            var pass = CheckPasswordStrength(userObj.Password);
            if(!string.IsNullOrEmpty(pass))
                return BadRequest(new { Message = pass.ToString() });


            // Hashing the password
            userObj.Password = PasswordHasher.HashPassword(userObj.Password);
            // Passing the user role as default
            userObj.Role = "User";
            // TODO JWT Token
            userObj.Token = "";
            // Send and Save To DataBase
            await _authContext.Users.AddAsync(userObj);
            await _authContext.SaveChangesAsync();
            return Ok(new
            {
                Message = "User Registered!"
            });
        }

        private Task<bool> CheckUserNameExistAsync(string username)
            => _authContext.Users.AnyAsync(x => x.UserName == username);

        private Task<bool> CheckEmailExistAsync(string email)
            => _authContext.Users.AnyAsync(x => x.Email == email);

        private string CheckPasswordStrength(string password)
        {
            // Checking password strength...
            // if it contains 8 characters
            StringBuilder sb = new StringBuilder();
            if(password.Length < 8)
            {
                sb.Append("Password must contain at least 8 characters" + Environment.NewLine);
            }
            // if it contains Alpha Numeric.
            if (!(Regex.IsMatch(password, "[a-z]") && Regex.IsMatch(password, "[A-Z]") && Regex.IsMatch(password, "[0-9]"))) 
            {
                sb.Append("Password must contain Alphanumeric" + Environment.NewLine);
            }
            // if it contains special characters.
            if (!Regex.IsMatch(password, "[<,>,@,!,#,$,%,^,&,*,(,),_,+,\\[,\\],{,},?,:,;,|,',\\,.,/,~,`,-,=]"))
            {
                sb.Append("Password must contain special characters" + Environment.NewLine);
            }

            return sb.ToString();
        }
    }
}
