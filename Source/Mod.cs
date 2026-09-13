using Verse;

namespace StoragebornXenotype
{
    public class StoragebornXenotypeMod : Mod
    {
        public StoragebornXenotypeMod(ModContentPack content) : base(content)
        {
            Log.Message($"[{content.Name}] loaded.");
        }
    }
}
