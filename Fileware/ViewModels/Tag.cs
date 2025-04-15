using Avalonia.Media;
using ReactiveUI;

namespace Fileware.ViewModels;

public class Tag : ReactiveObject
{
    public string Name { get; set; }
    public bool Active { get; set; }
    public IBrush Color { get; set; }
}