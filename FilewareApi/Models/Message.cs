using System.ComponentModel.DataAnnotations;

namespace FilewareApi.Models;

public class Message
{
    [Key]
    public int Id { get; set; }

    public string Text { get; set; }
    public DateTime Time { get; set; }
}