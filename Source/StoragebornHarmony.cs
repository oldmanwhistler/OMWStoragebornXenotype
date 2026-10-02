using HarmonyLib;
using RimWorld;
using Verse;

namespace StoragebornXenotype
{
    [StaticConstructorOnStartup]
    public static class StoragebornHarmony
    {
        static StoragebornHarmony()
        {
            new Harmony("oldmanwhistler.StoragebornXenotype").PatchAll();
        }
    }

    [HarmonyPatch(typeof(RestUtility), nameof(RestUtility.CanUseBedEver), new[] { typeof(Pawn), typeof(ThingDef) })]
    public static class StoragebornBedEligibilityPatch
    {
        public static bool Prefix(Pawn p, ThingDef bedDef, ref bool __result)
        {
            if (!StoragebornController.HasStoragebornGene(p) || StoragebornBedRules.IsAllowedStoragebornSleepingSpot(bedDef))
                return true;

            __result = false;
            return false;
        }
    }

    public static class StoragebornBedRules
    {
        public static bool IsAllowedStoragebornSleepingSpot(ThingDef bedDef)
        {
            return bedDef == ThingDefOf.SleepingSpot || bedDef.defName == "DoubleSleepingSpot" ||
                (ModsConfig.BiotechActive && bedDef.defName == "BabySleepingSpot");
        }
    }

    [HarmonyPatch(typeof(CompAssignableToPawn_Bed), nameof(CompAssignableToPawn_Bed.CanAssignTo))]
    public static class StoragebornBedAssignmentPatch
    {
        public static bool Prefix(CompAssignableToPawn_Bed __instance, Pawn pawn, ref AcceptanceReport __result)
        {
            ThingDef bedDef = __instance.parent.def;
            if (!StoragebornController.HasStoragebornGene(pawn) || StoragebornBedRules.IsAllowedStoragebornSleepingSpot(bedDef))
                return true;

            __result = "StoragebornBedAssignmentRestriction".Translate();
            return false;
        }
    }

    [HarmonyPatch(typeof(Pawn_AgeTracker), "BirthdayBiological")]
    public static class BirthdayBiologicalPatch
    {
        public static void Postfix(Pawn_AgeTracker __instance)
        {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (pawn != null && StoragebornController.HasStoragebornGene(pawn))
                StoragebornController.SetStage(pawn);
        }
    }
}