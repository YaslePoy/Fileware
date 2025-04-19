using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Avalonia;
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
            StartPageViewModel startViewModel => new StartPage { DataContext = startViewModel },
            FileChatPageViewModel fileChatViewModel => new FileChatPage { DataContext = fileChatViewModel },
            LoginPageViewModel loginPageViewModel => new LoginPage { DataContext = loginPageViewModel },
            RegisterPageViewModel registerPageViewModel => new RegisterPage { DataContext = registerPageViewModel },
            BasePageViewModel basePageViewModel => new BasePage { DataContext = basePageViewModel },
            TilesViewModel tilesViewModel => new TilesPage { DataContext = tilesViewModel },
            ProfileViewModel profileViewModel => new ProfilePage { DataContext = profileViewModel },
            EditProfileViewModel editProfileViewModel => new EditProfilePage { DataContext = editProfileViewModel },
            FileSpaceViewModel fileSpaceViewModel => new FileSpacePage() { DataContext = fileSpaceViewModel },
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}

public class ReflectionViewLocator : IViewLocator
{
    private static Dictionary<Type, Type> _routeTypes = new();
    public IViewFor? ResolveView<T>(T? viewModel, string? contract = null)
    {
        var type = viewModel.GetType();
        if (!_routeTypes.TryGetValue(type, out var pageType))
        {
            var findPageType = FindPageType(type);
            _routeTypes[type] = findPageType;
            pageType = findPageType;
        }

        var pageInstance = Activator.CreateInstance(pageType) as StyledElement;
        pageInstance.DataContext = viewModel;
        return pageInstance as IViewFor;
    }

    private List<Type>? _cachedTypes = null;
    
    private Type FindPageType(Type type)
    {
        if (_cachedTypes is null)
        {
            var assembly = Assembly.GetAssembly(GetType());

            _cachedTypes = assembly.GetTypes().ToList();
        }
        
        var name = type.Name;
        if (name.EndsWith("ViewModel"))
        {
            name = name[..^9];
        }else if( name.EndsWith("VM"))
        {
            name = name[..^2];
        }

        var pageType = _cachedTypes.Find(i => i.Name == name + "Page");
        if (pageType is not null)
            return pageType;
        if (_cachedTypes.Find(i => i.Name == name) is {} justType)
        {
            return justType;
        }

        return null;

    }
}