using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace TeachingRecordSystem.Core;

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

    public static string? GetDisplayName(this Enum enumValue)
    {
        var displayAttribute = enumValue.GetType()
            .GetMember(enumValue.ToString())
            .Single()
            .GetCustomAttribute<DisplayAttribute>();

        return displayAttribute is null ? enumValue.ToString() : displayAttribute.GetName();
    }

    public static IReadOnlyCollection<TEnum> SplitFlags<TEnum>(this TEnum input)
        where TEnum : struct, Enum
    {
        return Impl().ToArray();

        IEnumerable<TEnum> Impl()
        {
            foreach (int v in Enum.GetValuesAsUnderlyingType<TEnum>())
            {
                if (((int)(object)input & v) != 0 &&
                    (v != 0) && ((v & (v - 1)) == 0))  // Exclude 0 and non powers of two
                {
                    yield return (TEnum)(object)v;
                }
            }
        }
    }

    public static bool HasAnyFlag<TEnum>(this TEnum enumValue, TEnum flags)
        where TEnum : struct, Enum
    {
        return ((int)(object)enumValue & (int)(object)flags) > 0;
    }
}
