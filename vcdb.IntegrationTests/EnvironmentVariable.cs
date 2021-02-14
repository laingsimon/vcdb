using System;

namespace vcdb.IntegrationTests
{
    internal static class EnvironmentVariable
    {
        public static T Get<T>(string name)
        {
            var variable = Environment.GetEnvironmentVariable(name);
            if (string.IsNullOrEmpty(variable))
            {
                return default;
            }

            return Parse<T>(variable);
        }

        private static T Parse<T>(string variable)
        {
            var underlyingNullableType = Nullable.GetUnderlyingType(typeof(T));
            if (underlyingNullableType != null)
            {
                return (T)Parse(variable, underlyingNullableType);
            }

            return (T)Parse(variable, typeof(T));
        }

        private static object Parse(string variable, Type asType)
        {
            if (asType == typeof(string))
                return variable;

            if (asType == typeof(int))
                return int.Parse(variable);

            if (asType == typeof(bool))
                return bool.Parse(variable);

            throw new NotSupportedException($"Unable to parse `{variable}` into {asType.FullName}; type conversion is not supported");
        }
    }
}
