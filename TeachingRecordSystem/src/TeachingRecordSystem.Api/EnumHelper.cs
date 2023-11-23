namespace TeachingRecordSystem.Api;

public static class EnumHelper
{
    public static TResult ConvertToEnumByValue<TSource, TResult>(this TSource input)
        where TSource : struct, Enum
        where TResult : struct, Enum
    {
        if (!TryConvertToEnumByValue<TSource, TResult>(input, out var result))
        {
            throw new FormatException($"Unknown {typeof(TSource).Name}: '{Convert.ToInt32(input)}'.");
        }

        return result;
    }

    public static TResult ConvertToEnumByName<TSource, TResult>(this TSource input)
        where TSource : struct, Enum
        where TResult : struct, Enum
    {
        if (!TryConvertToEnumByName<TSource, TResult>(input, out var result))
        {
            throw new FormatException($"Unknown {typeof(TSource).Name}: '{input}'.");
        }

        return result;
    }

    public static bool TryConvertToEnumByName<TSource, TResult>(this TSource input, out TResult result)
        where TSource : struct, Enum
        where TResult : struct, Enum
    {
        var inputAsName = input.ToString();

        if (Enum.TryParse(typeof(TResult), inputAsName, out var resultObj))
        {
            result = (TResult)resultObj;
            return true;
        }
        else
        {
            result = default;
            return false;
        }
    }

    public static bool TryConvertToEnumByValue<TSource, TResult>(this TSource input, out TResult result)
        where TSource : struct, Enum
        where TResult : struct, Enum
    {
        var inputAsValue = Convert.ToInt32(input);

        if (Enum.IsDefined(typeof(TResult), inputAsValue))
        {
            result = (TResult)Enum.ToObject(typeof(TResult), inputAsValue);
            return true;
        }
        else
        {
            result = default;
            return false;
        }
    }
}
