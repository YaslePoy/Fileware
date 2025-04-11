using System;
using Fileware.ViewModels;
using Fileware.Views;
using ReactiveUI;

namespace Fileware;

public class AppViewLocator : IViewLocator
{
    public IViewFor ResolveView<T>(T? viewModel, string? contract = null)
    {
        return viewModel switch
        {
            StartViewModel startViewModel => new Start { DataContext = startViewModel },
            FileChatViewModel fileChatViewModel => new FileChat { DataContext = fileChatViewModel },
            LoginPageViewModel loginPageViewModel => new LoginPage { DataContext = loginPageViewModel },
            RegisterPageViewModel registerPageViewModel => new RegisterPage { DataContext = registerPageViewModel },
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}