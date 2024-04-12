// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using INTEX_II_413.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace INTEX_II_413.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IIntexRepository _repo; // Injecting the repository

        public IndexModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IIntexRepository repo) // Adding repository to the constructor
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _repo = repo;
        }


        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Username { get; set; }

        //Testing to see if this is needed
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }
        public string Country { get; set; }
        public char Gender { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            //[Phone]
            //[Display(Name = "Phone number")]
            //public string PhoneNumber { get; set; }

            [Required]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Required]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [Required]
            [DataType(DataType.Date)]
            [Display(Name = "Birth Date")]
            public DateTime BirthDate { get; set; }

            [Required]
            [Display(Name = "Country")]
            public string Country { get; set; }

            [Required]
            [Display(Name = "Gender")]
            public string Gender { get; set; }
        }


        private async Task LoadAsync(IdentityUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            Username = userName;

            var userId = _userManager.GetUserId(User); // Get the user's ASP.NET Identity ID

            // Try to load the customer associated with this user ID
            var customer = _repo.Customers.FirstOrDefault(c => c.AspNetUserId == userId);

            if (customer != null)
            {
                // If a customer record exists, pre-fill the input model with the customer's data
                Input = new InputModel
                {
                    FirstName = customer.FirstName,
                    LastName = customer.LastName,
                    BirthDate = customer.BirthDate,
                    Country = customer.Country,
                    Gender = customer.Gender.ToString() // Assuming Gender is stored as a char in the database
                };
            }
            else
            {
                // If no customer record exists, initialize Input with default values
                Input = new InputModel
                {
                    FirstName = "",
                    LastName = "",
                    BirthDate = DateTime.Now, // Or some sensible default
                    Country = "",
                    Gender = ""
                };
            }
        }



        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }


        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var userId = _userManager.GetUserId(User);
            var existingCustomer = _repo.Customers.FirstOrDefault(c => c.AspNetUserId == userId);

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            if (existingCustomer != null)
            {
                // Update existing customer
                existingCustomer.FirstName = Input.FirstName;
                existingCustomer.LastName = Input.LastName;
                existingCustomer.BirthDate = Input.BirthDate;
                existingCustomer.Country = Input.Country;
                existingCustomer.Gender = Input.Gender[0]; // Assuming Gender is stored as a char
                existingCustomer.Age = DateTime.Today.Year - Input.BirthDate.Year - (DateTime.Today < Input.BirthDate.AddYears(DateTime.Today.Year - Input.BirthDate.Year) ? 1 : 0);

                _repo.EditCustomer(existingCustomer);
            }
            else
            {
                // Create a new Customer instance and populate it with data from the form
                var newCustomer = new Customer
                {
                    FirstName = Input.FirstName,
                    LastName = Input.LastName,
                    BirthDate = Input.BirthDate,
                    Country = Input.Country,
                    Gender = Input.Gender[0], // Assuming Gender is stored as a char
                    Age = DateTime.Today.Year - Input.BirthDate.Year - (DateTime.Today < Input.BirthDate.AddYears(DateTime.Today.Year - Input.BirthDate.Year) ? 1 : 0),
                    AspNetUserId = userId
                };

                _repo.AddCustomer(newCustomer);
            }

            _repo.SaveChanges(); // Commit the transaction

            StatusMessage = "Your profile has been updated";
            await _signInManager.RefreshSignInAsync(user);
            return RedirectToPage();
        }


    }
}
