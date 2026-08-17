/*
 * Copyright (c) 2026 PaperAxis. All rights reserved.
 * This file is part of PaperStok. Unauthorized copying, modification
 * or distribution of this file is strictly prohibited.
 */
namespace PaperStok.Core.Logo;

/// <summary>
/// Thrown when a query — default or custom — would write to the Logo
/// Tiger3 database. PaperStok is a read-only tool by design; see
/// SqlReadOnlyGuard.
/// </summary>
public sealed class UnsafeQueryException(string message) : Exception(message);
