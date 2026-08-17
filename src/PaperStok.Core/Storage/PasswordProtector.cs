/*
 * Copyright (c) 2026 PaperAxis. All rights reserved.
 * This file is part of PaperStok. Unauthorized copying, modification
 * or distribution of this file is strictly prohibited.
 */
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;

namespace PaperStok.Core.Storage;

/// <summary>
/// Encrypts/decrypts connection passwords with Windows DPAPI (current-user
/// scope) so profiles.json never carries a plaintext secret. The exe stays
/// portable — DPAPI needs no installer or registry setup — but a profile
/// only decrypts on the Windows account/machine that saved it.
/// </summary>
[SupportedOSPlatform("windows")]
public static class PasswordProtector
{
    private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("PaperStok.ConnectionProfile.v1");

    public static string? Protect(string? plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return null;

        var bytes = Encoding.UTF8.GetBytes(plainText);
        var protectedBytes = ProtectedData.Protect(bytes, Entropy, DataProtectionScope.CurrentUser);
        return Convert.ToBase64String(protectedBytes);
    }

    public static string? Unprotect(string? protectedText)
    {
        if (string.IsNullOrEmpty(protectedText))
            return null;

        var protectedBytes = Convert.FromBase64String(protectedText);
        var bytes = ProtectedData.Unprotect(protectedBytes, Entropy, DataProtectionScope.CurrentUser);
        return Encoding.UTF8.GetString(bytes);
    }
}
