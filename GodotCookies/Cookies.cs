using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Encodings.Web;
using Godot;
using GlobalMutexSharp;

namespace GodotCookies;

/// <summary>
/// Stores and retrieves key-value data from a JSON file.
/// </summary>
public readonly struct Cookies(string Path) {
    /// <summary>
    /// The path to the cookie file.
    /// </summary>
    public string Path { get; } = Path;

    /// <summary>
    /// The options to use when serializing/deserializing JSON.
    /// </summary>
    public JsonSerializerOptions JsonOptions { get; } = DefaultJsonOptions;

    /// <summary>
    /// The default options to use when serializing/deserializing JSON.
    /// </summary>
    public static JsonSerializerOptions DefaultJsonOptions { get; set; } = new JsonSerializerOptions() {
        // Base
        AllowTrailingCommas = true,
        IncludeFields = true,
        NewLine = "\n",
        ReadCommentHandling = JsonCommentHandling.Skip,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        // Pretty
        WriteIndented = true,
        IndentCharacter = '\t',
        IndentSize = 1,
    };

    /// <summary>
    /// Gets a static <see cref="Cookies"/> instance for the file <c>user://Cookies.json</c>.
    /// </summary>
    public static Cookies User { get; } = new("user://Cookies.json");

    /// <summary>
    /// The timeout for acquiring the global mutex.
    /// </summary>
    private static readonly TimeSpan GlobalMutexTimeout = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Stores all entries to the cookies file, overwriting if it already exists.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if successful.
    /// </returns>
    /// <exception cref="TimeoutException"/>
    /// <exception cref="NotSupportedException"/>
    public bool SetAll(Dictionary<string, object?> Entries) {
        using GlobalMutex GlobalMutex = new(Path);
        using (GlobalMutex.Acquire(GlobalMutexTimeout)) {
            string EntriesJson = JsonSerializer.Serialize(Entries, JsonOptions);
            using FileAccess? CookiesFile = FileAccess.Open(Path, FileAccess.ModeFlags.Write);
            if (CookiesFile is null) {
                return false;
            }
            return CookiesFile.StoreString(EntriesJson);
        }
    }
    /// <summary>
    /// Stores an entry to the file.
    /// </summary>
    /// <remarks>
    /// Throws a <see cref="JsonException"/> or <see cref="NotSupportedException"/> if the entries could not be deserialized.
    /// </remarks>
    /// <returns>
    /// <see langword="true"/> if successful.
    /// </returns>
    /// <exception cref="TimeoutException"/>
    /// <exception cref="JsonException"/>
    /// <exception cref="NotSupportedException"/>
    public bool Set(string Key, object? Value) {
        using GlobalMutex GlobalMutex = new(Path);
        using (GlobalMutex.Acquire(GlobalMutexTimeout)) {
            Dictionary<string, object?> Cookies = GetAll();
            if (Value is not null) {
                Cookies[Key] = Value;
            }
            else {
                Cookies.Remove(Key);
            }
            return SetAll(Cookies);
        }
    }
    /// <summary>
    /// Gets all entries in the file.
    /// </summary>
    /// <remarks>
    /// Throws a <see cref="JsonException"/> or <see cref="NotSupportedException"/> if the entries could not be deserialized.
    /// </remarks>
    /// <returns>
    /// The entries if successful, or an empty dictionary.
    /// </returns>
    /// <exception cref="TimeoutException"/>
    /// <exception cref="JsonException"/>
    /// <exception cref="NotSupportedException"/>
    public Dictionary<string, object?> GetAll() {
        using GlobalMutex GlobalMutex = new(Path);
        using (GlobalMutex.Acquire(GlobalMutexTimeout)) {
            string? Cookies = FileAccess.GetFileAsString(Path);
            if (string.IsNullOrWhiteSpace(Cookies)) {
                return [];
            }
            return JsonSerializer.Deserialize<Dictionary<string, object?>>(Cookies, JsonOptions) ?? [];
        }
    }
    /// <summary>
    /// Gets the serialized value stored with the key.
    /// </summary>
    /// <remarks>
    /// Throws a <see cref="JsonException"/> or <see cref="NotSupportedException"/> if the value could not be deserialized.
    /// </remarks>
    /// <returns>
    /// The serialized value if successful, or <see langword="null"/>.
    /// </returns>
    /// <exception cref="TimeoutException"/>
    /// <exception cref="JsonException"/>
    /// <exception cref="NotSupportedException"/>
    public object? Get(string Key) {
        using GlobalMutex GlobalMutex = new(Path);
        using (GlobalMutex.Acquire(GlobalMutexTimeout)) {
            return GetAll().GetValueOrDefault(Key);
        }
    }
    /// <summary>
    /// Gets and deserializes the value stored with the key.
    /// </summary>
    /// <remarks>
    /// Throws a <see cref="JsonException"/> or <see cref="NotSupportedException"/> if the value could not be deserialized as <typeparamref name="T"/>.
    /// </remarks>
    /// <returns>
    /// The value if successful, or <see langword="default"/>.
    /// </returns>
    /// <exception cref="TimeoutException"/>
    /// <exception cref="JsonException"/>
    /// <exception cref="NotSupportedException"/>
    public T? Get<T>(string Key) {
        using GlobalMutex GlobalMutex = new(Path);
        using (GlobalMutex.Acquire(GlobalMutexTimeout)) {
            object? Value = Get(Key);
            if (Value is null) {
                return default;
            }
            return JsonSerializer.SerializeToElement(Value, JsonOptions).Deserialize<T>(JsonOptions);
        }
    }
    /// <summary>
    /// Deletes the file.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the file was deleted.
    /// </returns>
    /// <exception cref="TimeoutException"/>
    public bool Delete() {
        using GlobalMutex GlobalMutex = new(Path);
        using (GlobalMutex.Acquire(GlobalMutexTimeout)) {
            return DirAccess.RemoveAbsolute(Path) is Error.Ok;
        }
    }
    /// <summary>
    /// Checks if the file exists.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the file exists.
    /// </returns>
    /// <exception cref="TimeoutException"/>
    public bool Exists() {
        using GlobalMutex GlobalMutex = new(Path);
        using (GlobalMutex.Acquire(GlobalMutexTimeout)) {
            return FileAccess.FileExists(Path);
        }
    }
}