using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Sys;
using LanguageExt.Sys.Live;
using LanguageExt.Sys.Traits;
using static LanguageExt.Prelude;

namespace ConsoleApp1
{
    internal class Program
    {
        private static Task Main(string[] args) =>
            from x in Main<Runtime>().Run(Runtime.New()).ToRef()
            let r = x.ThrowIfFail()
            select r;

        static Aff<RT, Unit> Main<RT>() where
            RT : struct, HasConsole<RT> =>
                repeat(from l in Console<RT>.readLine.ToAff()
                       from v in parseInt(l).ToAff()
                       from _ in Console<RT>.writeLine($"{v}").ToAff()
                       select unit);
    }
}
