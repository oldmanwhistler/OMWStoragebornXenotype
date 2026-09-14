using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace StoragebornXenotype
{
    public class StoragebornWandererQuestRoot : QuestNode_Root_WandererJoin_WalkIn
    {
        public override Pawn GeneratePawn_NewTemp(Map map)
        {
            Pawn pawn = base.GeneratePawn_NewTemp(map);
            XenotypeDef xenotype = DefDatabase<XenotypeDef>.GetNamedSilentFail("omw_storageborn");
            if (xenotype != null)
            {
                pawn.genes.SetXenotype(xenotype);
                StoragebornController.ApplyTo(pawn);
            }
            return pawn;
        }
    }

    public class IncidentWorker_StoragebornWanderer : IncidentWorker_GiveQuest
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            StoragebornSettings settings = StoragebornXenotypeMod.Instance?.Settings!;
            if (settings == null || !settings.StoragebornRefugeeQuestEnabled)
                return false;

            if (Find.TickManager.TicksGame < settings.StoragebornRefugeeQuestDelayTicks)
                return false;

            return base.CanFireNowSub(parms);
        }
    }
}
