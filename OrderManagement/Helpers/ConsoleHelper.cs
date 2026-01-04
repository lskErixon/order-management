namespace OrderManagement.Helpers;

public static class ConsoleHelper
{
    public static void PrintHeader(string title)
    {
        Console.WriteLine($"\n------------ {title} ------------");
    }

    public static void PrintSuccess(string message)
    {
        Console.WriteLine($"[OK] {message}");
    }

    public static void PrintError(string message)
    {
        Console.WriteLine($"[ERROR] {message}");
    }

    public static void PrintLine(int length = 65)
    {
        Console.WriteLine(new string('-', length));
    }

    public static int? ReadInt(string prompt)
    {
        Console.Write(prompt);
        if (int.TryParse(Console.ReadLine(), out int result))
            return result;
        return null;
    }

    public static decimal? ReadDecimal(string prompt)
    {
        Console.Write(prompt);
        if (decimal.TryParse(Console.ReadLine(), out decimal result))
            return result;
        return null;
    }

    public static string ReadString(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine() ?? "";
    }

    public static bool Confirm(string message)
    {
        Console.Write($"{message} (y/n): ");
        return Console.ReadLine()?.ToLower() == "y";
    }
}