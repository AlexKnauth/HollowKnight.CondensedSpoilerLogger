using Modding;
using MonoMod.ModInterop;
using RandomizerMod.Logging;
using CondensedSpoilerLogger.Loggers;
using System.Collections.Generic;
using RandomizerMod.RC;
using CondensedSpoilerLogger.Interop;
using System.Reflection;

namespace CondensedSpoilerLogger
{
    public class CondensedSpoilerLogger : Mod, IGlobalSettings<GlobalSettings>
    {
        internal static CondensedSpoilerLogger instance;

        public static GlobalSettings GS = new();
        public void OnLoadGlobal(GlobalSettings s)
        {
            GS = s;
            GS.WrittenLogs ??= new();
        }
        public GlobalSettings OnSaveGlobal() => GS;


        public CondensedSpoilerLogger() : base(null)
        {
            instance = this;
            typeof(API.Export).ModInterop();
        }
        
        public override string GetVersion()
        {
            return GetType().Assembly.GetName().Version.ToString();
        }
        
        public static IEnumerable<CslLogger> CreateLoggers()
        {
            yield return new CondensedSpoilerLog();
            yield return new NotchCostSpoiler();
            yield return new AreaSpoilerLog();
            yield return new ItemGroupSpoiler();
            yield return new AreaTransitionSpoiler();
            yield return new ItemProgressionSpoiler();
        }

        public static void WriteCslLogs()
        {
            if (RandomizerMod.RandomizerMod.RS?.Context is not RandoModContext ctx)
            {
                DebugMod.LogToConsole("No rando context found");
                return;
            }

            LogArguments args = new()
            {
                randomizer = default,
                ctx = ctx,
                gs = ctx.GenerationSettings
            };

            foreach (CslLogger logger in CreateLoggers())
            {
                logger.MakeLogRequests(args);
            }
        }

        public override void Initialize()
        {
            Log("Initializing Mod...");

            ModHooks.FinishedLoadingModsHook += () =>
            {
                foreach (CslLogger logger in CreateLoggers())
                {
                    LogManager.AddLogger(logger);
                }
            };

            CslMenu.Hook();

            RBDebug.Hook();

            DebugMod.AddActionToKeyBindList(WriteCslLogs, "Write Csl Logs");
        }
    }
}