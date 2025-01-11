namespace NewEra_Cash_Carry.DTOs.UserDTOs;

public class UserRegisterDto
{
    public string UserName { get; set; }
    public string PasswordHash { get; set; }
    public string Role { get; set; }
}
