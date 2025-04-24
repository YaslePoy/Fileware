using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace FilewareApi.Models;

[Index("Username")]
public class User
{
    [Key]
    public int Id { get; set; }
    public string Username { get; set; }
    public string ShowName { get; set; }
    public string? Password { get; set; }
    public DateOnly BirthDate { get; set; }
    public byte[]? TotpKey { get; set; }
    [JsonIgnore]
    public byte[]? Avatar { get; set; }

    public List<string> AttachedFileSpaces { get; set; } = [];
    public string Preferences { get; set; }
}

public class CommonUserData
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string ShowName { get; set; }
    public DateOnly BirthDate { get; set; }
    public List<string> AttachedFileSpaces { get; set; }
    public int FileCount { get; set; }
    public string Preferences { get; set; }
    
}

public static class BasincExtentions
{
    public static T Also<T>(this T value, Action<T> operation)
    {
        operation(value);
        return value;
    }
}