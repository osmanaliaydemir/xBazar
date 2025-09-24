using System.Text.RegularExpressions;
using BCrypt.Net;
using Core.Interfaces;

namespace Infrastructure.Services;

public class PasswordService : IPasswordService
{
    private readonly int _workFactor;

    public PasswordService()
    {
        _workFactor = 12; // BCrypt work factor
    }

    public string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Password cannot be null or empty", nameof(password));

        return BCrypt.Net.BCrypt.HashPassword(password, _workFactor);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashedPassword))
            return false;

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
        catch
        {
            return false;
        }
    }

    public bool IsPasswordStrong(string password)
    {
        if (string.IsNullOrEmpty(password) || password.Length < 8)
            return false;

        // En az 8 karakter, büyük harf, küçük harf, rakam ve özel karakter içermeli
        var hasUpperCase = new Regex(@"[A-Z]").IsMatch(password);
        var hasLowerCase = new Regex(@"[a-z]").IsMatch(password);
        var hasDigits = new Regex(@"\d").IsMatch(password);
        var hasSpecialChar = new Regex(@"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]").IsMatch(password);

        return hasUpperCase && hasLowerCase && hasDigits && hasSpecialChar;
    }
}
