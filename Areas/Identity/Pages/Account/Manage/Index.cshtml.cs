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

namespace AuthLab2.Areas.Identity.Pages.Account.Manage
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
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            Input = new InputModel
            {
                //PhoneNumber = phoneNumber,
                // Load additional properties from your database or user manager
                FirstName = "Loaded First Name", // Replace with actual data retrieval
                LastName = "Loaded Last Name",   // Replace with actual data retrieval
                BirthDate = DateTime.Now,       // Replace with actual data retrieval
                Country = "Loaded Country",     // Replace with actual data retrieval
                Gender = "Loaded Gender"        // Replace with actual data retrieval
            };
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

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            // Create a new Customer instance and populate it with data from the form
            var customer = new Customer
            {
                FirstName = Input.FirstName,
                LastName = Input.LastName,
                BirthDate = Input.BirthDate,
                Country = Input.Country,
                Gender = Input.Gender[0], // Assuming Gender is stored as a char
                Age = DateTime.Today.Year - Input.BirthDate.Year - (DateTime.Today < Input.BirthDate.AddYears(DateTime.Today.Year - Input.BirthDate.Year) ? 1 : 0)
                //id = _userManager.GetUserId(User)
            };

            // Using the repository to add the new customer
            _repo.AddCustomer(customer); // Assuming you have an AddCustomer method or similar
            _repo.SaveChanges(); // Commit the transaction

            StatusMessage = "Your profile has been updated";
            await _signInManager.RefreshSignInAsync(user);
            return RedirectToPage();
        }

    }
}
