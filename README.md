# Godot Cookies

[![NuGet](https://img.shields.io/nuget/v/GodotCookies.svg)](https://www.nuget.org/packages/GodotCookies)

Easily store options on-device in Godot 4.4. This is useful for storing user settings such as volume and keybindings.

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

Remove a cookie:
```cs
Cookies.User.Remove("Health");
```

Get all cookies:
```cs
JsonObject AllCookies = Cookies.User.GetAll();
```

Delete the cookies file:
```cs
Cookies.User.Delete();
```

## Notes

- Cookie files are automatically created upon setting the first cookie.
- Cookie files are locked while in use using a global mutex to prevent data corruption.
- Cookie files should not contain large amounts of data because the entire file is read and rewritten every time a cookie is set.
- Godot types (such as `Color`) don't work well with `System.Text.Json`, so you should use a `[JsonConverter]` attribute.

## About the name

Godot Cookies is inspired by browser cookies, which are small pieces of data stored in the user's browser.
However, they are unrelated.