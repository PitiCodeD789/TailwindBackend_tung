using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Tailwind.Trader.User.Api.Command
{
    public class CreateUserCommand
    {
        [Required]
        [RegularExpression("^[a-zA-Z ]*$")]
        public string FirstName { get; set; }

        [Required]
        [RegularExpression("^[a-zA-Z ]*$")]
        public string LastName { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string Country { get; set; }

        [Phone]
        [Required]
        public string PhoneNumber { get; set; }

        [EmailAddress]
        [Required]
        public string Email { get; set; }

        public Models.User MapUser() =>
            new Models.User()
            {
                Password = this.Password,
                FirstName = this.FirstName,
                LastName = this.LastName,
                Address = this.Address,
                Country = this.Country,
                PhoneNumber = this.PhoneNumber,
                Email = this.Email
            };
    }
}
