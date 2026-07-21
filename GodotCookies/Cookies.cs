using GlobalMutexSharp;
using Godot;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;

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
    /// Warning message for dynamic serialization.
    /// </summary>
    private const string UnreferencedCodeMessage = "JSON serialization and deserialization might require types that cannot be statically analyzed.";

    /// <summary>
    /// Stores all entries to the cookies file, overwriting if it already exists.
    /// </summary>
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="TimeoutException"/>
    /// <exception cref="System.IO.IOException" />
    public void SetAll(JsonObject Entries) {
        ArgumentNullException.ThrowIfNull(Entries);

        using GlobalMutex GlobalMutex = new(Path);
        using (GlobalMutex.Acquire(GlobalMutexTimeout)) {
            string EntriesJson = Entries.ToJsonString(JsonOptions);

            using FileAccess CookiesFile = FileAccess.Open(Path, FileAccess.ModeFlags.Write)
                ?? throw new System.IO.IOException("Failed to open cookies file for writing");

            if (!CookiesFile.StoreString(EntriesJson)) {
                throw new System.IO.IOException("Failed to write to cookies file");
            }
        }
    }
    /// <summary>
    /// Stores an entry to the file.
    /// </summary>
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="TimeoutException"/>
    /// <exception cref="System.IO.IOException" />
    /// <exception cref="JsonException"/>
    public void Set(string Key, JsonNode? Value) {
        ArgumentNullException.ThrowIfNull(Key);

        using GlobalMutex GlobalMutex = new(Path);
        using (GlobalMutex.Acquire(GlobalMutexTimeout)) {
            JsonObject Cookies = GetAll();

            Cookies[Key] = Value;

            SetAll(Cookies);
        }
    }
    /// <summary>
    /// Stores an entry to the file.
    /// </summary>
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="TimeoutException"/>
    /// <exception cref="System.IO.IOException" />
    /// <exception cref="JsonException"/>
    public void Set<T>(string Key, T Value, JsonTypeInfo<T> JsonTypeInfo) {
        ArgumentNullException.ThrowIfNull(Key);
        ArgumentNullException.ThrowIfNull(JsonTypeInfo);

        using GlobalMutex GlobalMutex = new(Path);
        using (GlobalMutex.Acquire(GlobalMutexTimeout)) {
            JsonObject Cookies = GetAll();

            Cookies[Key] = JsonSerializer.SerializeToNode(Value, JsonTypeInfo);

            SetAll(Cookies);
        }
    }
    /// <summary>
    /// Stores an entry to the file.
    /// </summary>
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="TimeoutException"/>
    /// <exception cref="System.IO.IOException" />
    /// <exception cref="JsonException"/>
    /// <exception cref="NotSupportedException"/>
    [RequiresUnreferencedCode(UnreferencedCodeMessage), RequiresDynamicCode(UnreferencedCodeMessage)]
    public void Set<T>(string Key, T Value) {
        ArgumentNullException.ThrowIfNull(Key);

        using GlobalMutex GlobalMutex = new(Path);
        using (GlobalMutex.Acquire(GlobalMutexTimeout)) {
            JsonObject Cookies = GetAll();

            Cookies[Key] = JsonSerializer.SerializeToNode(Value, JsonOptions);

            SetAll(Cookies);
        }
    }
    /// <summary>
    /// Removes an entry from the file.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the entry was found and removed.
    /// </returns>
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="TimeoutException"/>
    /// <exception cref="System.IO.IOException" />
    /// <exception cref="JsonException"/>
    public bool Remove(string Key) {
        ArgumentNullException.ThrowIfNull(Key);

        using GlobalMutex GlobalMutex = new(Path);
        using (GlobalMutex.Acquire(GlobalMutexTimeout)) {
            JsonObject Cookies = GetAll();

            bool Removed = Cookies.Remove(Key);

            SetAll(Cookies);

            return Removed;
        }
    }
    /// <summary>
    /// Gets all entries in the file.
    /// </summary>
    /// <returns>
    /// The entries if successful, or an empty dictionary.
    /// </returns>
    /// <exception cref="TimeoutException"/>
    /// <exception cref="System.IO.IOException" />
    /// <exception cref="JsonException"/>
    public JsonObject GetAll() {
        using GlobalMutex GlobalMutex = new(Path);
        using (GlobalMutex.Acquire(GlobalMutexTimeout)) {
            if (!FileAccess.FileExists(Path)) {
                return [];
            }

            using FileAccess CookiesFile = FileAccess.Open(Path, FileAccess.ModeFlags.Read)
                ?? throw new System.IO.IOException("Failed to open cookies file for reading");

            string? Cookies = CookiesFile.GetAsText();
            if (string.IsNullOrWhiteSpace(Cookies)) {
                return [];
            }

            return JsonNode.Parse(Cookies, documentOptions: CreateJsonDocumentOptions(JsonOptions)) as JsonObject
                ?? throw new JsonException("Cookies file does not contain a JSON object");
        }
    }
    /// <summary>
    /// Gets the serialized value stored with the key.
    /// </summary>
    /// <returns>
    /// The serialized value if successful, or <see langword="null"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="TimeoutException"/>
    /// <exception cref="System.IO.IOException" />
    /// <exception cref="JsonException"/>
    public JsonNode? Get(string Key, JsonNode? Default = null) {
        ArgumentNullException.ThrowIfNull(Key);

        using GlobalMutex GlobalMutex = new(Path);
        using (GlobalMutex.Acquire(GlobalMutexTimeout)) {
            JsonObject Cookies = GetAll();

            if (!Cookies.TryGetPropertyValue(Key, out JsonNode? Value)) {
                return Default;
            }

            return Value;
        }
    }
    /// <summary>
    /// Gets and deserializes the value stored with the key.
    /// </summary>
    /// <returns>
    /// The value if successful, or <see langword="default"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="TimeoutException"/>
    /// <exception cref="System.IO.IOException" />
    /// <exception cref="JsonException"/>
    public T? Get<T>(string Key, JsonTypeInfo<T> JsonTypeInfo, T? Default = default) {
        ArgumentNullException.ThrowIfNull(Key);
        ArgumentNullException.ThrowIfNull(JsonTypeInfo);

        using GlobalMutex GlobalMutex = new(Path);
        using (GlobalMutex.Acquire(GlobalMutexTimeout)) {
            JsonObject Cookies = GetAll();

            if (!Cookies.TryGetPropertyValue(Key, out JsonNode? Value)) {
                return Default;
            }

            return JsonSerializer.Deserialize(Value, JsonTypeInfo);
        }
    }
    /// <summary>
    /// Gets and deserializes the value stored with the key.
    /// </summary>
    /// <returns>
    /// The value if successful, or <see langword="default"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="TimeoutException"/>
    /// <exception cref="System.IO.IOException" />
    /// <exception cref="JsonException"/>
    /// <exception cref="NotSupportedException"/>
    [RequiresUnreferencedCode(UnreferencedCodeMessage), RequiresDynamicCode(UnreferencedCodeMessage)]
    public T? Get<T>(string Key, T? Default = default) {
        ArgumentNullException.ThrowIfNull(Key);

        using GlobalMutex GlobalMutex = new(Path);
        using (GlobalMutex.Acquire(GlobalMutexTimeout)) {
            JsonObject Cookies = GetAll();

            if (!Cookies.TryGetPropertyValue(Key, out JsonNode? Value)) {
                return Default;
            }

            return JsonSerializer.Deserialize<T>(Value, JsonOptions);
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

    private static JsonDocumentOptions CreateJsonDocumentOptions(JsonSerializerOptions JsonOptions) {
        return new JsonDocumentOptions() {
            AllowDuplicateProperties = JsonOptions.AllowDuplicateProperties,
            AllowTrailingCommas = JsonOptions.AllowTrailingCommas,
            CommentHandling = JsonOptions.ReadCommentHandling,
            MaxDepth = JsonOptions.MaxDepth,
        };
    }
}