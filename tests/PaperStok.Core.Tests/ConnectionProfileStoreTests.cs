/*
 * Copyright (c) 2026 PaperAxis. All rights reserved.
 * This file is part of PaperStok. Unauthorized copying, modification
 * or distribution of this file is strictly prohibited.
 */
using PaperStok.Core.Models;
using PaperStok.Core.Storage;
using Xunit;

namespace PaperStok.Core.Tests;

public class ConnectionProfileStoreTests
{
    [Fact]
    public void Load_ReturnsEmptyList_WhenFileMissing()
    {
        var path = Path.Combine(Path.GetTempPath(), $"paperstok-test-{Guid.NewGuid():N}.json");

        var profiles = new ConnectionProfileStore(path).Load();

        Assert.Empty(profiles);
    }

    [Fact]
    public void SaveAndLoad_RoundTripsProfileFields()
    {
        var path = Path.Combine(Path.GetTempPath(), $"paperstok-test-{Guid.NewGuid():N}.json");
        try
        {
            var store = new ConnectionProfileStore(path);
            var profile = new ConnectionProfile
            {
                Name = "Merkez",
                ServerAddress = "10.0.0.5",
                Port = 1433,
                DatabaseName = "TIGER3",
                AuthMode = LogoAuthMode.SqlServer,
                Username = "logo",
                ProtectedPassword = "already-protected-base64",
                FirmNo = 1,
                PeriodNo = 1
            };

            store.Save([profile]);
            var loaded = store.Load();

            Assert.Single(loaded);
            Assert.Equal("Merkez", loaded[0].Name);
            Assert.Equal(1433, loaded[0].Port);
            Assert.Equal("already-protected-base64", loaded[0].ProtectedPassword);
            Assert.Equal(LogoAuthMode.SqlServer, loaded[0].AuthMode);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}
