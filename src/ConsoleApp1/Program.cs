using System;
using LanguageExt;
using LanguageExt.Sys.Live;
using static LanguageExt.Prelude;
using static LanguageExt.Sys.Console<LanguageExt.Sys.Live.Runtime>;

namespace ConsoleApp1
{
    internal class Program
    {
        private static void Main(string[] args) =>
            Main().Run(Runtime.New()).ThrowIfFail();

        static Eff<Runtime, Unit> Main() =>
            repeat(from l in readLine
                   from v in parseInt(l).ToEff()
                   from _ in writeLine($"{v}")
                   select unit);
    }
}
