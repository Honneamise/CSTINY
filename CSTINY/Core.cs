using System.Globalization;

namespace CSTINY;

public static class Core
{
    private static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs ex)
    {
        if (ex.ExceptionObject is Exception e)
        {
            Console.WriteLine("[*ERROR*]\n" + e.Message.ToString());

            Environment.Exit(-1);
        }
    }

    public static void Init()
    {
        CultureInfo.CurrentCulture = new CultureInfo("en-US");

        AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;
    }

    public static void Usage()
    {
        Console.WriteLine("USAGE: CSTINY.EXE [params]");
        Console.WriteLine("Possible params:");
        Console.WriteLine("-t srcfile           [display the ast on stdout]");
        Console.WriteLine("-i srcfile           [execute the interpreter]");
        Console.WriteLine("-n srcfile dstfile   [call the nasm transpiler]");
    }
}