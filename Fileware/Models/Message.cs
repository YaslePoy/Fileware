using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace Fileware;

public class Message : INotifyPropertyChanged
{
    public int Id { get; set; }

    public string Text { get; set; }
    public DateTime Time { get; set; }
    public string FormattedTime => Time.ToString("t");
    public event PropertyChangedEventHandler? PropertyChanged;
    [ForeignKey("User")]
    public int UserId { get; set; }
    public int User { get; set; }
    
    public virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}