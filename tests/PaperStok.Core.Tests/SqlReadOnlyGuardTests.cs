/*
 * Copyright (c) 2026 PaperAxis. All rights reserved.
 * This file is part of PaperStok. Unauthorized copying, modification
 * or distribution of this file is strictly prohibited.
 */
using PaperStok.Core.Logo;
using Xunit;

namespace PaperStok.Core.Tests;

public class SqlReadOnlyGuardTests
{
    [Theory]
    [InlineData("SELECT * FROM LG_001_ITEMS")]
    [InlineData("select code from LG_001_ITEMS")]
    [InlineData("WITH cte AS (SELECT 1 AS x) SELECT x FROM cte")]
    [InlineData("SELECT * FROM LG_001_ITEMS;")]
    [InlineData("-- a comment\nSELECT * FROM LG_001_ITEMS")]
    public void EnsureReadOnly_AllowsPlainSelects(string sql)
    {
        SqlReadOnlyGuard.EnsureReadOnly(sql);
    }

    [Theory]
    [InlineData("INSERT INTO LG_001_ITEMS (CODE) VALUES ('X')")]
    [InlineData("UPDATE LG_001_ITEMS SET NAME = 'x'")]
    [InlineData("DELETE FROM LG_001_ITEMS")]
    [InlineData("DROP TABLE LG_001_ITEMS")]
    [InlineData("TRUNCATE TABLE LG_001_ITEMS")]
    [InlineData("ALTER TABLE LG_001_ITEMS ADD X INT")]
    [InlineData("CREATE TABLE X (Y INT)")]
    [InlineData("EXEC sp_who")]
    [InlineData("EXEC xp_cmdshell 'dir'")]
    [InlineData("MERGE INTO LG_001_ITEMS USING x ON 1=1")]
    [InlineData("SELECT * INTO NewTable FROM LG_001_ITEMS")]
    public void EnsureReadOnly_RejectsWritesAndDdl(string sql)
    {
        Assert.Throws<UnsafeQueryException>(() => SqlReadOnlyGuard.EnsureReadOnly(sql));
    }

    [Fact]
    public void EnsureReadOnly_RejectsStackedStatements()
    {
        Assert.Throws<UnsafeQueryException>(() =>
            SqlReadOnlyGuard.EnsureReadOnly("SELECT 1; DELETE FROM LG_001_ITEMS;"));
    }

    [Fact]
    public void EnsureReadOnly_RejectsQueryNotStartingWithSelect()
    {
        Assert.Throws<UnsafeQueryException>(() =>
            SqlReadOnlyGuard.EnsureReadOnly("DECLARE @x INT; SELECT @x"));
    }

    [Fact]
    public void EnsureReadOnly_RejectsEmptyQuery()
    {
        Assert.Throws<UnsafeQueryException>(() => SqlReadOnlyGuard.EnsureReadOnly(""));
    }
}
