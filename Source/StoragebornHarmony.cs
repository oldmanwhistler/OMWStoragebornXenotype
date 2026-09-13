using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
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

    [HarmonyPatch(typeof(Pawn_AgeTracker), "BirthdayBiological")]
    public static class BirthdayBiologicalPatch
    {
        public static void Postfix(Pawn_AgeTracker __instance)
        {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (pawn != null && StoragebornController.HasStoragebornGene(pawn))
                StoragebornController.SetAgeStage(pawn);
        }
    }
}