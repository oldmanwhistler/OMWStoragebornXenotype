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
            Harmony harmony = new Harmony("oldmanwhistler.StoragebornXenotype");
            harmony.PatchAll();
            TryPatchShowMeYourHands(harmony);
        }

        private static void TryPatchShowMeYourHands(Harmony harmony)
        {
            System.Type handDrawerType = AccessTools.TypeByName("ShowMeYourHands.HandDrawer");
            if (handDrawerType == null)
                return;

            System.Reflection.MethodInfo postDraw = AccessTools.Method(handDrawerType, "PostDraw");
            if (postDraw == null)
            {
                Log.Warning("[Storageborn Xenotype] Could not find Show Me Your Hands HandDrawer.PostDraw; Storageborn hand suppression was not applied.");
                return;
            }

            harmony.Patch(postDraw, prefix: new HarmonyMethod(typeof(StoragebornHarmony), nameof(SkipShowMeYourHandsForStorageborn)));
        }

        public static bool SkipShowMeYourHandsForStorageborn(object __instance)
        {
            ThingWithComps parent = Traverse.Create(__instance).Field("parent").GetValue<ThingWithComps>();
            return !(parent is Pawn pawn) || !StoragebornController.HasStoragebornGene(pawn);
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