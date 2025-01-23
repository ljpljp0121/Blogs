public static class AccountFormatUtility
{
    public static bool CheckName(string name)
    {
        return !string.IsNullOrEmpty(name) && name.Length >= 5 && name.Length <= 12;
    }

    public static bool CheckPassword(string password)
    {
        return !string.IsNullOrEmpty(password) && password.Length >= 6 && password.Length <= 16;
    }
}
