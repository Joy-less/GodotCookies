# Godot Cookies

[![NuGet](https://img.shields.io/nuget/v/GodotCookies.svg)](https://www.nuget.org/packages/GodotCookies)

Easily store options on-device in Godot 4. This is useful for storing user settings such as volume and keybindings.

Godot Cookies stores data as indented JSON making it easy to understand and edit manually.

> [!NOTE]
> Godot Cookies v1.1+ only supports Godot 4.4+ due to [a breaking change with FileAccess.StoreString](https://github.com/godotengine/godot/pull/78289).

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
- Cookie files are automatically created upon setting the first cookie.
- Cookie files are locked while in use to prevent data corruption.
- Cookie files should not contain large amounts of data because the entire file is read and rewritten every time a cookie is set.
- Godot types (such as `Color`) don't work well with `System.Text.Json`, so you should use a `[JsonConverter]` attribute.

## About the name

Godot Cookies is inspired by browser cookies, which are small pieces of data stored in the user's browser.
However, they are unrelated.