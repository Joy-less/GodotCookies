# Godot Cookies

[![NuGet](https://img.shields.io/nuget/v/GodotCookies.svg)](https://www.nuget.org/packages/GodotCookies)

Easily store options on-device in Godot 4. This is useful for storing user settings such as volume and keybindings.

Godot Cookies stores data as indented JSON making it easy to understand and edit manually.

## Usage

Set a cookie:
```cs
Cookies.User.Set("Health", 50);
```

Get a cookie:
```cs
int Health = Cookies.User.Get<int>("Health");
```

Get all cookies:
```cs
Dictionary<string, object?> Cookies = Cookies.User.GetAll();
```

Remove all cookies:
```cs
Cookies.User.SetAll([]);
Cookies.User.Delete();
```

## Notes

- Cookies can be removed by setting them to `null`.
- The cookie file is automatically created upon setting the first cookie.
- The library should not be used to store large amounts of data, since the entire file is read and rewritten every time a cookie is set.