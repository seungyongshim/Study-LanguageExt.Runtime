using System;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Sys.Live;
using static LanguageExt.Prelude;
using static LanguageExt.Sys.Console<LanguageExt.Sys.Live.Runtime>;

namespace ConsoleApp1
{
    internal class Program
    {
        private static Task Main(string[] args) =>
            from x in Main().Run(Runtime.New()).ToRef()
            let r = x.ThrowIfFail()
            select r;

        static Aff<Runtime, Unit> Main() =>
            repeat(from l in readLine.ToAff()
                   from v in parseInt(l).ToAff()
                   from _ in writeLine($"{v}").ToAff()
                   select unit);
    }
}
