using Verse;

namespace StoragebornXenotype
{
    public class StoragebornXenotypeMod : Mod
    {
        public static StoragebornXenotypeMod Instance { get; private set; } = null!;

        public StoragebornSettings Settings { get; }

        public StoragebornXenotypeMod(ModContentPack content) : base(content)
        {
            Instance = this;
            Settings = GetSettings<StoragebornSettings>();
            Log.Message($"[{content.Name}] loaded.");
        }

        public override string SettingsCategory()
        {
            return "StoragebornSettingsCategory".Translate();
        }

        public override void DoSettingsWindowContents(UnityEngine.Rect inRect)
        {
            Settings.DoWindowContents(inRect);
        }
    }
}
