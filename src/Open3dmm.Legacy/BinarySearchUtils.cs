using System;

public static class BinarySearchUtils
{
    public static bool Any<T, TSearch>(Span<T> span, in TSearch key, out int index)
        where T : struct, IComparable<TSearch>
    {
        var low = 0;
        var high = span.Length - 1;

        while (low <= high)
        {
            index = (low + high) >> 1;
            var cmp = span[index].CompareTo(key);
            switch (cmp)
            {
                case < 0:
                    low = index + 1;
                    break;
                case > 0:
                    high = index - 1;
                    break;
                default:
                    return true;
            }
        }

        index = low;
        return false;
    }

    public static bool First<T, TSearch>(Span<T> span, in TSearch key, out int index)
        where T : struct, IComparable<TSearch>
    {
        var low = 0;
        var high = span.Length - 1;

        while (low <= high)
        {
            index = (low + high) >> 1;
            var cmp = span[index].CompareTo(key);
            switch (cmp)
            {
                case < 0:
                    low = index + 1;
                    break;
                case > 0:
                    high = index - 1;
                    break;
                case 0 when low != index:
                    high = index;
                    break;
                default:
                    return true;
            }
        }

        index = low;
        return false;
    }
}