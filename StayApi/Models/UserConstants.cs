public static class UserConstants
{
    public static List<UserModel> Users = new List<UserModel>()
    {
        new UserModel() { Username = "host1", Password = "password", Role = "Host" },
        new UserModel() { Username = "guest1", Password = "password", Role = "Guest" },
        new UserModel() { Username = "admin1", Password = "password", Role = "Admin" }
    };
}