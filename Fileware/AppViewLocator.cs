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
            BasePageViewModel basePageViewModel => new BasePage { DataContext = basePageViewModel },
            TilesViewModel tilesViewModel => new TilesPage { DataContext = tilesViewModel },
            ProfileViewModel profileViewModel => new ProfilePage { DataContext = profileViewModel },
            EditProfileViewModel editProfileViewModel => new EditProfilePage { DataContext = editProfileViewModel },
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}