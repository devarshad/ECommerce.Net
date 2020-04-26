using System.ComponentModel.DataAnnotations;

namespace ECommerce.Net.Identity.API.Models.AccountViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
