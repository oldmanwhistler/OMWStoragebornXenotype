using System;
using RimWorld;
using Verse;

namespace StoragebornXenotype
{
    public class StoragebornCategoryDef : Def
    {
        public TechLevel worldTechLevel;
    }

    public class StoragebornBodyCategoryExtension : DefModExtension
    {
        public StoragebornCategoryDef OMW_StoragebornCategory = null!;
    }

    public static class WorldTechLevelIntegration
    {
        private const string PackageId = "m00nl1ght.WorldTechLevel";

        public static bool IsAvailable => ModsConfig.IsActive(PackageId);

        public static bool TryGetWorldTechLevel(out TechLevel techLevel)
        {
            techLevel = TechLevel.Undefined;
            if (!IsAvailable) return false;

            Type type = GenTypes.GetTypeInAnyAssembly("WorldTechLevel.WorldTechLevel");
            var property = type?.GetProperty("Current");
            if (property?.GetValue(null) is TechLevel value)
            {
                techLevel = value;
                return true;
            }
            return false;
        }
    }
}
