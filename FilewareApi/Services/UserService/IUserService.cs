using System.Security.Cryptography;
using FilewareApi.Models;
using OtpNet;

namespace FilewareApi.Services.UserService;

public interface IUserService
{
    string GenerateTotpKey(string username);
    Task<int> Register(User user);
    bool IsExists(string username);
    User? Auth(string login, string security);
    User? Get(int id);
}

public class UserService(FilewareDbContext db) : IUserService
{
    public string GenerateTotpKey(string username)
    {
        var uriString = new OtpUri(OtpType.Totp, RandomNumberGenerator.GetBytes(64), username, "Fileware").ToString();
        return uriString;
    }

    public async Task<int> Register(User user)
    {
        if (db.Users.Any(i => i.Username == user.Username))
        {
            return -1;
        }

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return user.Id;
    }

    public bool IsExists(string username)
    {
        return db.Users.Any(i => i.Username == username);
    }
    
    public User? Auth(string login, string security)
    {
        var user = db.Users.FirstOrDefault(i => i.Username == login);
        if (user is null)
            return null;

        if (user.Password == security)
            return user;

        if (new Totp(user.TotpKey).VerifyTotp(security, out _))
        {
            return user;
        }

        return null;
    }

    public User? Get(int id)
    {
        return db.Users.FirstOrDefault(i => i.Id == id);
    }
}