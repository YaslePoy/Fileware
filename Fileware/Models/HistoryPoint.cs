using System;
using System.Text.Json;

namespace Fileware;

public enum HistoryPointType
{
    File,
    Message
}

public class HistoryPoint
{
    public int Id { get; set; }
    public int Type { get; set; }
    public int LinkedId { get; set; }
    public DateTime Time { get; set; }
    public JsonElement Linked { get; set; }
}