namespace Zapto.Mediator.Generator;

public static class StringExtensions
{
    public static string RemoveSuffix(this string name, string suffix)
    {
        if (name.EndsWith(suffix) && name.Length != suffix.Length)
        {
            name = name.Substring(0, name.Length - suffix.Length);
        }

        return name;
    }


    public static string RemovePrefix(this string name, string suffix)
    {
        if (name.StartsWith(suffix) && name.Length != suffix.Length)
        {
            name = name.Substring(suffix.Length, name.Length - suffix.Length);
        }

        return name;
    }
}
