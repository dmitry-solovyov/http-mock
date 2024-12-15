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
    public bool Equals(char x, char y) => Char.ToLower(x) == Char.ToLower(y);

    public int GetHashCode([DisallowNull] char obj) => Char.ToLower(obj).GetHashCode();
}
