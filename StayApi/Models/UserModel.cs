public class UserModel
{
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Role { get; set; }  = null!;// "Host", "Guest, "Admin"
}
