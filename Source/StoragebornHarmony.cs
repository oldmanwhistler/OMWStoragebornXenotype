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
   
    [HarmonyPatch]
    public static class StoragebornSleepingBodyRenderPatch
    {
        public static System.Reflection.MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(PawnRenderer), "GetBodyPos", new[]
            {
                typeof(UnityEngine.Vector3), typeof(PawnPosture), typeof(bool).MakeByRefType()
            });
        }

        public static void Postfix(PawnRenderer __instance, PawnPosture posture, ref bool showBody)
        {
            if (posture != PawnPosture.LayingInBed && posture != PawnPosture.LayingInBedFaceUp)
                return;

            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (pawn != null && StoragebornController.HasStoragebornGene(pawn) && pawn.CurrentBed() != null)
                showBody = true;
        }
    }

    [HarmonyPatch(typeof(PawnRenderNodeWorker_Body), nameof(PawnRenderNodeWorker_Body.CanDrawNow))]
    public static class StoragebornBodyNodeVisibilityPatch
    {
        public static void Postfix(PawnRenderNode node, PawnDrawParms parms, ref bool __result)
        {
            if (__result || parms.Portrait || parms.posture == PawnPosture.Standing ||
                parms.flags.FlagSet(PawnRenderFlags.NoBody) || parms.bed == null ||
                !parms.pawn.RaceProps.Humanlike || !node.DebugEnabled)
                return;

            if (parms.pawn.mindState?.duty?.def?.drawBodyOverride == false)
                return;

            if (StoragebornController.HasStoragebornGene(parms.pawn))
                __result = true;
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