using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

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
}

public class GetUserResponce
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string ShowName { get; set; }
    public DateOnly BirthDate { get; set; }
}