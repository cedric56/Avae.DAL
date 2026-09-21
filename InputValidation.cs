using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Avae.DAL;

/// <summary>
/// https://stackoverflow.com/questions/2112143/how-can-i-define-a-idataerrorinfo-error-property-for-multiple-bo-properties
/// </summary>
/// <typeparam name="T"></typeparam>
public static class InputValidation<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>
{
    public static void Init()
    {
        foreach (var prop in typeof(T).GetProperties())
        {
            var validations = prop
                .GetCustomAttributes(typeof(ValidationAttribute), true)
                .Cast<ValidationAttribute>()
                .ToArray();

            if (validations.Length == 0)
                continue;

            Register(
                prop.Name,
                x => prop.GetValue(x),
                validations
            );
        }
    }

    // Registered validators (AOT-safe)
    private static readonly Dictionary<string, ValidatorEntry> _validators = [];

    public static void Register<TValue>(
        string propertyName,
        Func<T, TValue> getter,
        params ValidationAttribute[] rules)
    {
        _validators[propertyName] = new ValidatorEntry
        {
            Getter = src => getter(src)!,
            Rules = rules
        };
    }

    public static string Validate(T source, string columnName)
    {
        if (_validators.TryGetValue(columnName, out var entry))
        {
            var value = entry.Getter(source);
            var errors = entry.Rules
                .Where(v => !v.IsValid(value))
                .Select(v => v.ErrorMessage ?? "")
                .ToArray();

            return string.Join(Environment.NewLine, errors);
        }
        return string.Empty;
    }

    public static ICollection<string> Validate(T source)
    {
        var messages = new List<string>();
        foreach (var entry in _validators.Values)
        {
            var value = entry.Getter(source);
            messages.AddRange(
                entry.Rules
                    .Where(v => !v.IsValid(value))
                    .Select(v => v.ErrorMessage ?? "")
            );
        }
        return messages;
    }

    public static string Error(T source) =>
        string.Join(Environment.NewLine, Validate(source));

    private class ValidatorEntry
    {
        public Func<T, object> Getter { get; set; } = default!;
        public ValidationAttribute[] Rules { get; set; } = default!;
    }
}
