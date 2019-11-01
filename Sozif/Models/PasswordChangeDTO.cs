namespace Sozif.Models
{
    public class PasswordChangeDTO
    {

        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string NewPasswordConfirmation { get; set; }

    }
}
