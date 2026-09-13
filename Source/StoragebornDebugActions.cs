using LudeonTK;
using RimWorld;
using Verse;

namespace StoragebornXenotype
{
    public static class StoragebornDebugActions
    {
        [DebugAction("Pawns", "Spawn Storageborn Lifecycle", requiresBiotech: true, displayPriority: 500)]
        public static void SpawnStoragebornLifecycle()
        {
            Map map = Find.CurrentMap;
            if (map == null || Faction.OfPlayer == null)
            {
                Log.Warning("[Storageborn Xenotype] A player map is required.");
                return;
            }

            XenotypeDef xenotype = DefDatabase<XenotypeDef>.GetNamedSilentFail("omw_storageborn");
            if (xenotype == null)
            {
                Log.Error("[Storageborn Xenotype] Could not find omw_storageborn.");
                return;
            }

            int[] ages = { 0, 8, 18, 30, 50, 70 };
            foreach (int age in ages)
            {
                Pawn pawn = PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist, Faction.OfPlayer);
                pawn.ageTracker.AgeBiologicalTicks = age * GenDate.TicksPerYear;
                pawn.genes.SetXenotype(xenotype);
                StoragebornController.ApplyTo(pawn);

                IntVec3 cell = CellFinder.RandomClosewalkCellNear(map.Center, map, 12,
                    c => c.Standable(map) && c.GetFirstPawn(map) == null);
                GenSpawn.Spawn(pawn, cell, map);
            }

            Log.Message("[Storageborn Xenotype] Spawned Flatlet, Tall-boy, Kallax, and Greater Kallax test pawns.");
        }
    }
}
