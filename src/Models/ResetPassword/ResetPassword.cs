using System.ComponentModel.DataAnnotations;

namespace ChannelX.Models.Account 
{
    public class ResetPasswordViewModel
    {
      
        [Required]
        public string Password { get; set; }
        
        [Required]
        [Compare("Password", ErrorMessage = "Confirm Password must match.")]
        public string ConfirmPassword{ get; set; }
        [Required]
        public string Key {get;set;}
        
    }
}