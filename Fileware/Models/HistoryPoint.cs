using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Fileware;

public enum HistoryPointType
{
    File,
    Message
}

public class HistoryPoint
{
    [Key]
    public int Id { get; set; }
    public int Type { get; set; }
    public int LinkedId { get; set; }
    public DateTime Time { get; set; }
    public JsonElement Linked { get; set; }
    public string Key { get; set; }
}