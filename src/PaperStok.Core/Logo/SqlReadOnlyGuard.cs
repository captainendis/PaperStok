/*
 * Copyright (c) 2026 PaperAxis. All rights reserved.
 * This file is part of PaperStok. Unauthorized copying, modification
 * or distribution of this file is strictly prohibited.
 */
using System.Text.RegularExpressions;

namespace PaperStok.Core.Logo;

/// <summary>
/// PaperStok never writes to the Logo Tiger3 database — it only pulls
/// stock totals. This guard is an application-level safety net that
/// rejects any query (the default template or a profile's custom one)
/// containing a statement that could modify data or schema, before it
/// ever reaches SqlCommand.
///
/// This is defense in depth, not the primary control: the SQL login used
/// by PaperStok should also be restricted to read-only access (e.g. the
/// db_datareader role) on the Logo Tiger3 database itself. Naive text
/// scanning can theoretically be fooled or produce a false positive on an
/// unlucky string literal (e.g. containing the word "into") — the DB-level
/// permission is what makes the read-only guarantee absolute.
/// </summary>
public static partial class SqlReadOnlyGuard
{
    private static readonly string[] ForbiddenKeywords =
    [
        "INSERT", "UPDATE", "DELETE", "MERGE", "UPSERT",
        "DROP", "ALTER", "CREATE", "TRUNCATE", "RENAME",
        "EXEC", "EXECUTE", "INTO",
        "GRANT", "REVOKE", "DENY",
        "BACKUP", "RESTORE", "SHUTDOWN", "KILL", "DBCC", "RECONFIGURE",
        "OPENROWSET", "OPENQUERY", "OPENDATASOURCE"
    ];

    [GeneratedRegex(@"^\s*(--[^\n]*\n\s*)*\b(SELECT|WITH)\b", RegexOptions.IgnoreCase)]
    private static partial Regex StartsWithReadStatementRegex();

    /// <summary>Throws <see cref="UnsafeQueryException"/> if <paramref name="sql"/> could write to the database.</summary>
    public static void EnsureReadOnly(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
            throw new UnsafeQueryException("Query is empty.");

        if (!StartsWithReadStatementRegex().IsMatch(sql))
            throw new UnsafeQueryException("Query must start with SELECT (or a SELECT-only WITH/CTE). PaperStok is read-only.");

        // Reject statement stacking: only a single, optional trailing
        // semicolon is allowed. Anything else means more than one statement.
        var trimmed = sql.TrimEnd();
        if (trimmed.EndsWith(';'))
            trimmed = trimmed[..^1];
        if (trimmed.Contains(';'))
            throw new UnsafeQueryException("Query must be a single statement (no ';' allowed except one trailing terminator). PaperStok is read-only.");

        foreach (var keyword in ForbiddenKeywords)
        {
            if (Regex.IsMatch(sql, $@"\b{Regex.Escape(keyword)}\b", RegexOptions.IgnoreCase))
                throw new UnsafeQueryException($"Query contains a forbidden write/DDL keyword ('{keyword}'). PaperStok never modifies the Logo Tiger3 database.");
        }

        if (Regex.IsMatch(sql, @"\bsp_\w*|\bxp_\w*", RegexOptions.IgnoreCase))
            throw new UnsafeQueryException("Query calls a system/extended stored procedure. PaperStok never modifies the Logo Tiger3 database.");
    }
}
