using System.ComponentModel.DataAnnotations;

namespace FinalWebapp.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email or Username is required")]
        [Display(Name = "Email or Username")]
        public string EmailOrUsername { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }
}