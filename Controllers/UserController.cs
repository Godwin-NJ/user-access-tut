using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace VerifyEmailForgotPasswordTut.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly DataContext _context;
        public UserController(DataContext context)
        {
            _context = context;
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDto request)
        {
            if(_context.Users.Any(u => u.Email == request.Email)) 
            { 
                return BadRequest("User already exist");
            }

            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var user = new User
            {
                Email = request.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                VerificationToken = CreateRandomToken(),
            };

             _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok("user successfully created");

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if(user == null)
            {
                return BadRequest("invalid user credential");
            }

            if (!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                return BadRequest("password incorrect");
            }

            if (user.VerifiedAt == null)
            {
                return BadRequest("user is not verified");
            }

           
            return Ok($"welcome back, {user.Email}");

        }


        [HttpPost("verify")]
        public async Task<IActionResult> Verify(string token)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.VerificationToken == token);
            if (user == null)
            {
                return BadRequest("invalid token");
            }

            user.VerifiedAt = DateTime.Now;

            await _context.SaveChangesAsync();


            return Ok($"user verified!");

        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return BadRequest("user does not exist");
            }

            user.PasswordResetToken = CreateRandomToken();

            user.ResetTokenExpires = DateTime.Now.AddDays(1);

            await _context.SaveChangesAsync();


            return Ok($"You may now reset your password!");

        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto resetDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == resetDto.Token);
            if (user == null || user.ResetTokenExpires < DateTime.Now)
            {
                return BadRequest("invalid token");
            }


            CreatePasswordHash(resetDto.Password, out byte[] passwordHash, out byte[] passwordsalt);
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordsalt;
            user.ResetTokenExpires = null;
          
            await _context.SaveChangesAsync();


            return Ok($"Password reset successful!");

        }




        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordsalt) 
        { 
            using(var hmac = new HMACSHA512())
            {
                passwordsalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

            }
        
        }

        private bool VerifyPasswordHash(string password,  byte[] passwordHash,  byte[] passwordsalt)
        {
            using (var hmac = new HMACSHA512(passwordsalt))
            {
              
               var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);

            }

        }

        private string CreateRandomToken()
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
        }

    }
}
