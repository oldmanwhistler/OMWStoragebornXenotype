using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace StoragebornXenotype
{
    public class ScenPart_StoragebornRuinSetup : ScenPart
    {
        public override void GenerateIntoMap(Map map)
        {
            if (Find.GameInitData == null)
                return;

            // ThingDef casketDef = DefDatabase<ThingDef>.GetNamedSilentFail("OMW_Storageborn_EmptyCasket");
            // List<Building_CryptosleepCasket> caskets = casketDef == null
            //     ? new List<Building_CryptosleepCasket>()
            //     : map.listerThings.ThingsOfDef(casketDef).OfType<Building_CryptosleepCasket>().OrderBy(casket => casket.Position.x).ToList();
            // List<Pawn> startingPawns = Find.GameInitData.startingAndOptionalPawns.ToList();
            // List<Pawn> storagebornPawns = startingPawns.Where(StoragebornController.HasStoragebornGene).Take(3).ToList();

            // for (int i = 0; i < storagebornPawns.Count && i < caskets.Count; i++)
            // {
            //     Pawn pawn = storagebornPawns[i];
            //     if (pawn.Spawned)
            //         pawn.DeSpawn();

            //     if (!caskets[i].TryAcceptThing(pawn, allowSpecialEffects: false))
            //         GenSpawn.Spawn(pawn, MapGenerator.PlayerStartSpot, map);
            // }
        }
    }
}
