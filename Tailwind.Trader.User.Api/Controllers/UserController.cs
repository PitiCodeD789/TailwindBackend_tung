using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tailwind.Trader.User.Api.Command;
using Tailwind.Trader.User.Api.Infrastucture;
using Tailwind.Trader.User.Api.ViewModels;

namespace Tailwind.Trader.User.Api.Controllers
{
    [Route("v1/api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserContext _userContext;
        public UserController(UserContext userContext)
        {
            _userContext = userContext;
        }

        [HttpPost("Register")]
        public ActionResult Register([FromBody]CreateUserCommand createUserCommamd)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            if (CreateUser(createUserCommamd))
                return NoContent();
            else
                return BadRequest("Email Invalid");

        }

        //http://192.168.1.40:32000/v1/api/User/Address
        //Edit Address
        [HttpPost("Address")]
        public ActionResult EditAddress([FromBody]EditAddressCommand editAddressCommand)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            if (EditUserAddress(editAddressCommand))
                return NoContent();
            else
                return BadRequest("Invalid User");

        }


        [HttpPost("Login")]
        public ActionResult Login([FromBody]LoginCommand loginCommand)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            string password = HashPassword(loginCommand.Password);
            var user = _userContext.Users.FirstOrDefault(x => x.Email.Equals(loginCommand.Email) && x.Password.Equals(password));
            if (user == null)
                return BadRequest("Wrong Username or Password ");

            LoginViewModel loginViewModel = new LoginViewModel()
            {
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName
            };
            return Ok(loginViewModel);
        }

        private bool EditUserAddress(EditAddressCommand editAddressCommand)
        {
            var user = _userContext.Users.FirstOrDefault(x => x.Id == editAddressCommand.UserId);
            if (user == null)
                return false;

            try
            {
                user.Address = editAddressCommand.Address;
                user.Country = editAddressCommand.Country;

                _userContext.Users.Update(user);
                _userContext.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                return true;
                throw e;
            }
        }

        private bool CreateUser(CreateUserCommand userCommand)
        {            
            try
            {
                userCommand.Password = HashPassword(userCommand.Password);
                Models.User newUser = userCommand.MapUser();
                _userContext.Add<Models.User>(newUser);
                _userContext.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                return false;
                throw e;
            }

        }

        private static string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("X2"));
                }
                return builder.ToString();
            }
        }

    }
}