using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace TeachingRecordSystem.Core;

public static class EnumHelper
{
    extension<TSource>(TSource input) where TSource : struct, Enum
    {
        public TResult ConvertToEnumByValue<TResult>() where TResult : struct, Enum
        {
            if (!TryConvertToEnumByValue<TSource, TResult>(input, out var result))
            {
                throw new FormatException($"Unknown {typeof(TSource).Name}: '{Convert.ToInt32(input)}'.");
            }

            return result;
        }

        public TResult ConvertToEnumByName<TResult>() where TResult : struct, Enum
        {
            if (!TryConvertToEnumByName<TSource, TResult>(input, out var result))
            {
                throw new FormatException($"Unknown {typeof(TSource).Name}: '{input}'.");
            }

            return result;
        }

        public bool TryConvertToEnumByName<TResult>(out TResult result) where TResult : struct, Enum
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

        public bool TryConvertToEnumByValue<TResult>(out TResult result) where TResult : struct, Enum
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

    public static string? GetDisplayName(this Enum enumValue)
    {
        var displayAttribute = enumValue.GetType()
            .GetMember(enumValue.ToString())
            .Single()
            .GetCustomAttribute<DisplayAttribute>();

        return displayAttribute is null ? enumValue.ToString() : displayAttribute.GetName();
    }

    extension<TEnum>(TEnum input) where TEnum : struct, Enum
    {
        public IReadOnlyCollection<TEnum> SplitFlags()
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

        public bool HasAnyFlag(TEnum flags)
        {
            return ((int)(object)input & (int)(object)flags) > 0;
        }
    }
}
