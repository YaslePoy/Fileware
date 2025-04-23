using System.Security.Cryptography;
using FilewareApi.Models;
using Microsoft.AspNetCore.Mvc;
using OtpNet;

namespace FilewareApi.Services.UserService;

public interface IUserService
{
    string GenerateTotpKey(string username);
    Task<int> Register(User user);
    bool IsExists(string username);
    User? Auth(string login, string security);
    User? Get(int id);
    Task SetupAvatar(int id, byte[] avatar);
    byte[] GetAvatar(int id);
    int GetFileCount(int userId);
    Task Update(CommonUserData user);
    bool VerifyTotp(string username, string totpKey);
}

public class UserService(FilewareDbContext db) : IUserService
{
    private static Dictionary<string, byte[]> TotpKeys = new();

    public string GenerateTotpKey(string username)
    {
        var key = RandomNumberGenerator.GetBytes(32);
        if (!TotpKeys.TryAdd(username, key))
            TotpKeys[username] = key;

        var uriString = new OtpUri(OtpType.Totp, key, username, "Fileware").ToString();
        return uriString;
    }

    public async Task<int> Register(User user)
    {
        if (db.Users.Any(i => i.Username == user.Username))
        {
            return -1;
        }

        if (user.TotpKey is not null)
        {
            user.TotpKey = TotpKeys[user.Username];
            TotpKeys.Remove(user.Username);
            user.Password = null;
        }
        else
            user.TotpKey = null;
        db.Users.Add(user);

        await db.SaveChangesAsync();
        user.AttachedFileSpaces = [$"user_{user.Id}:master"];
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

        if (user.TotpKey is not null && new Totp(user.TotpKey).VerifyTotp(security, out _))
        {
            return user;
        }

        return null;
    }

    public User? Get(int id)
    {
        return db.Users.FirstOrDefault(i => i.Id == id);
    }

    public async Task SetupAvatar(int id, byte[] avatar)
    {
        var user = db.Users.FirstOrDefault(i => i.Id == id);
        user.Avatar = avatar;
        db.Users.Update(user);
        await db.SaveChangesAsync();
    }

    public byte[] GetAvatar(int id)
    {
        var user = db.Users.FirstOrDefault(i => i.Id == id);
        if (user.Avatar is null)
        {
            return File.ReadAllBytes("");
        }

        return user.Avatar;
    }

    public int GetFileCount(int userId)
    {
        return db.HistoryPoints.Count(i => i.FileSpaceKey.StartsWith($"user_{userId}:"));
    }

    public async Task Update(CommonUserData user)
    {
        var fromDb = Get(user.Id);
        Utils.TransferData(fromDb, user);
        db.Users.Update(fromDb);
        await db.SaveChangesAsync();
    }

    public bool VerifyTotp(string username, string totpKey)
    {
        if (TotpKeys.TryGetValue(username, out var key))
        {
            return  new Totp(key).VerifyTotp(totpKey, out _);
        }

        return false;
    }
}