using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using ReactiveUI;

namespace Fileware.Models;

public class User : INotifyPropertyChanged
{
    private byte[]? _avatar;
    public int Id { get; set; }
    public string Username { get; set; }
    public string ShowName { get; set; }
    public int FileCount { get; set; }
    public int FileWeigth { get; set; }
    public DateOnly BirthDate { get; set; }
    public byte[]? TotpKey { get; set; }

    public byte[]? Avatar
    {
        get => _avatar;
        set
        {
            if (Equals(value, _avatar)) return;
            _avatar = value;

            using (var ms = new MemoryStream(_avatar))
            {
                AvatarImage = new Bitmap(ms);
                OnPropertyChanged(nameof(AvatarImage));
            }

            OnPropertyChanged();
        }
    }

    public IImage AvatarImage { get; set; }
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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