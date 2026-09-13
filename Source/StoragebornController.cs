using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace StoragebornXenotype
{
    public static class StoragebornController
    {
        private const string StoragebornGeneDefName = "OMW_Storageborn";
        private const string ChildhoodBackstoryDefName = "OMW_StoragebornChildhood";
        private const string AdulthoodBackstoryDefName = "OMW_StoragebornWanderer";
        private const int AgeGigantic = 50;
        private const int AgeLarge = 35;

        private static readonly string[] StageDefNames =
        {
            "OMW_StorageStage_0",
            "OMW_StorageStage_1",
            "OMW_StorageStage_2",
            "OMW_StorageStage_3",
            "OMW_StorageStage_4"
        };

        public static bool HasStoragebornGene(Pawn pawn)
        {
            if (pawn?.genes == null) return false;
            GeneDef def = DefDatabase<GeneDef>.GetNamedSilentFail(StoragebornGeneDefName);
            return def != null && pawn.genes.GetGene(def) != null;
        }

        public static void ApplyTo(Pawn pawn)
        {
            if (pawn == null || pawn.genes == null || !HasStoragebornGene(pawn)) return;

            if (pawn.story != null)
            {
                BackstoryDef childhood = DefDatabase<BackstoryDef>.GetNamedSilentFail(ChildhoodBackstoryDefName);
                BackstoryDef adulthood = DefDatabase<BackstoryDef>.GetNamedSilentFail(AdulthoodBackstoryDefName);
                if (childhood != null) pawn.story.Childhood = childhood;
                if (adulthood != null) pawn.story.Adulthood = adulthood;
            }

            SetAgeStage(pawn);
        }

        public static void SetAgeStage(Pawn pawn)
        {
            if (pawn?.genes == null || !HasStoragebornGene(pawn)) return;

            int targetIndex = GetStageIndex(pawn);
            GeneDef targetDef = DefDatabase<GeneDef>.GetNamedSilentFail(StageDefNames[targetIndex]);
            if (targetDef == null) return;

            List<Gene> existingStages = pawn.genes.GenesListForReading
                .Where(g => StageDefNames.Contains(g.def.defName))
                .ToList();

            bool alreadyCorrect = existingStages.Count == 1 && existingStages[0].def == targetDef;
            if (!alreadyCorrect)
            {
                foreach (Gene gene in existingStages)
                    pawn.genes.RemoveGene(gene);

                pawn.genes.AddGene(targetDef, xenogene: true);
            }
        }

        private static int GetStageIndex(Pawn pawn)
        {
            if (pawn.ageTracker != null && pawn.ageTracker.AgeBiologicalYears >= AgeGigantic)
                return 4;
            if (pawn.ageTracker != null && pawn.ageTracker.AgeBiologicalYears >= AgeLarge)
                return 3;
            if (pawn.DevelopmentalStage.Baby()) return 0;
            if (pawn.DevelopmentalStage.Child()) return 1;
            return 2;
        }
    }
}
