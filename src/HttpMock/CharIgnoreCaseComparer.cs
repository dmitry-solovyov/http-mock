using System.Diagnostics.CodeAnalysis;

namespace HttpMock;

public static class CharComparer
{
    private static CharIgnoreCaseComparer? _ordinalIgnoreCase;
    public static CharIgnoreCaseComparer OrdinalIgnoreCase
    {
        get => _ordinalIgnoreCase ??= new CharIgnoreCaseComparer();
    }
}

public sealed class CharIgnoreCaseComparer : IEqualityComparer<char>
{
    public bool Equals(char x, char y) => Char.ToLowerInvariant(x) == Char.ToLowerInvariant(y);

    public int GetHashCode([DisallowNull] char obj) => Char.ToLowerInvariant(obj).GetHashCode();
}
