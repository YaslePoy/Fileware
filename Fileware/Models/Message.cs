using System;

namespace Fileware;

public class Message
{
    public int Id { get; set; }

    public string Text { get; set; }
    public DateTime Time { get; set; }
    public string FormattedTime => Time.ToString("t");
}