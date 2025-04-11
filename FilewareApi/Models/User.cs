using System.ComponentModel.DataAnnotations;
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
    public string? TotpKey { get; set; }
}