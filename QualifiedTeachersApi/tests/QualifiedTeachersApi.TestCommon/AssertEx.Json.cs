using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace QualifiedTeachersApi.TestCommon;

public static partial class AssertEx
{
    public static void JsonEquals(string expected, string actual)
        => JsonEquals(JToken.Parse(expected), JToken.Parse(actual));

    public static void JsonObjectEquals(object actual, object expected)
        => JsonEquals(JToken.FromObject(expected), JToken.FromObject(actual));

    public static void JsonEquals(JToken expected, JToken actual)
    {
        if (expected is JValue expectedJValue)
        {
            if (!(actual is JValue))
            {
                ThrowMismatchedTypeException("JValue", actual.GetType().Name, GetPrintablePath(expected.Path));
            }

            var actualJValue = (JValue)actual;

            // We don't care if the types are different, only if the generated JSON is different
            var expectedValue = expectedJValue.ToString(Formatting.None);
            var actualValue = actualJValue.ToString(Formatting.None);

            if (StringComparer.Ordinal.Compare(expectedValue, actualValue) != 0)
            {
                ThrowNotEqualException(expectedValue, actualValue, GetPrintablePath(expected.Path));
            }
        }
        else if (expected is JArray expectedJArray)
        {
            if (!(actual is JArray))
            {
                ThrowMismatchedTypeException("JArray", actual.GetType().Name, GetPrintablePath(expected.Path));
            }

            var actualJArray = (JArray)actual;

            if (expectedJArray.Count != actualJArray.Count)
            {
                throw new JsonNotEqualException(
                    $"JSON not equal\nExpected {expectedJArray.Count} elements\nGot: {actualJArray.Count}\nPath: {GetPrintablePath(expected.Path)}");
            }

            for (var i = 0; i < expectedJArray.Count; i++)
            {
                var expectedElement = expectedJArray[i];
                var actualElement = actualJArray[i];

                JsonEquals(expectedElement, actualElement);
            }
        }
        else if (expected is JObject expectedJObject)
        {
            if (!(actual is JObject))
            {
                ThrowMismatchedTypeException("JObject", actual.GetType().Name, GetPrintablePath(expected.Path));
            }

            var actualJObject = (JObject)actual;

            // Compare each property on `expected` with `actual`
            foreach (var prop in expectedJObject.Properties())
            {
                var actualProp = actualJObject[prop.Name];
                if (actualProp == null)
                {
                    throw new JsonNotEqualException(
                        $"JSON not equal\nMissing property: '{prop.Name}'\nPath: {GetPrintablePath(expected.Path)}");
                }

                JsonEquals(prop.Value, actualProp);
            }

            // Check `actual` doesn't have extra properties over `expected`
            foreach (var prop in actualJObject.Properties())
            {
                var expectedProp = expectedJObject[prop.Name];
                if (expectedProp == null)
                {
                    throw new JsonNotEqualException(
                        $"JSON not equal\nExtra property {prop.Name}\nPath: {GetPrintablePath(expected.Path)}");
                }
            }
        }
        else
        {
            throw new NotSupportedException($"Don't know how to compare JTokens of type '{expected.GetType().Name}'.");
        }

        string GetPrintablePath(string path) => string.IsNullOrEmpty(path) ? "(root)" : path;

        void ThrowNotEqualException(string expectedValue, string actualValue, string path)
            => throw new JsonNotEqualException($"JSON not equal:\nExpected: {expected}\nActual: {actual}\nPath: {path}");

        void ThrowMismatchedTypeException(string expectedType, string actualType, string path)
            => throw new JsonNotEqualException($"Token types not equal\nExpected: {expectedType}\nActual: {actualType}\nPath: {path}");
    }

    public class JsonNotEqualException : Exception
    {
        public JsonNotEqualException(string message)
            : base(message)
        {
        }
    }
}
