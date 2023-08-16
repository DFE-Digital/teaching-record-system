namespace TeachingRecordSystem.Api;

public static class EnumHelper
{
    public static TResult ConvertToEnum<TSource, TResult>(this TSource input)
        where TSource : struct, Enum
        where TResult : struct, Enum
    {
        if (!TryConvertToEnum<TSource, TResult>(input, out var result))
        {
            throw new FormatException($"Unknown {typeof(TSource).Name}: '{input}'.");
        }

        return result;
    }

    public static bool TryConvertToEnum<TSource, TResult>(this TSource input, out TResult result)
        where TSource : struct, Enum
        where TResult : struct, Enum
    {
        var inputAsInt = Convert.ToInt32(input);

        if (Enum.IsDefined(typeof(TResult), inputAsInt))
        {
            result = (TResult)Enum.ToObject(typeof(TResult), inputAsInt);
            return true;
        }
        else
        {
            result = default;
            return false;
        }
    }
}
