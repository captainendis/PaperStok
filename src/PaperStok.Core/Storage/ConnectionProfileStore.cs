/*
 * Copyright (c) 2026 PaperAxis. All rights reserved.
 * This file is part of PaperStok. Unauthorized copying, modification
 * or distribution of this file is strictly prohibited.
 */
using System.Text.Json;
using System.Text.Json.Serialization;
using PaperStok.Core.Models;

namespace PaperStok.Core.Storage;

/// <summary>
/// Persists connection profiles as a JSON file next to the executable so
/// PaperStok stays portable — no registry, no per-machine installer state.
/// Passwords are stored already DPAPI-protected (see PasswordProtector);
/// this class never sees a plaintext password.
/// </summary>
public sealed class ConnectionProfileStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly string _filePath;

    public ConnectionProfileStore(string? filePath = null)
    {
        _filePath = filePath ?? Path.Combine(AppContext.BaseDirectory, "profiles.json");
    }

    public List<ConnectionProfile> Load()
    {
        if (!File.Exists(_filePath))
            return [];

        var json = File.ReadAllText(_filePath);
        if (string.IsNullOrWhiteSpace(json))
            return [];

        return JsonSerializer.Deserialize<List<ConnectionProfile>>(json, JsonOptions) ?? [];
    }

    public void Save(IEnumerable<ConnectionProfile> profiles)
    {
        var json = JsonSerializer.Serialize(profiles, JsonOptions);
        File.WriteAllText(_filePath, json);
    }
}
