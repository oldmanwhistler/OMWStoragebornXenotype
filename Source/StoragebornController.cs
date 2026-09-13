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
        
        private static readonly string[] StageDefNames =
        {
            "OMW_StorageStage_0",
            "OMW_StorageStage_0",
            "OMW_StorageStage_1",
            "OMW_StorageStage_2",
            "OMW_StorageStage_3",
            "OMW_StorageStage_4"
        };

        private static readonly int[] StageAgeMin =
        {
            0,
            8,
            18,
            30,
            50,
            70
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

            RemoveBabyApparel(pawn);
            SetAgeStage(pawn);
        }

        private static void RemoveBabyApparel(Pawn pawn)
        {
            if (!pawn.DevelopmentalStage.Baby() || pawn.apparel == null) return;

            foreach (Apparel apparel in pawn.apparel.WornApparel.ToList())
                pawn.apparel.Remove(apparel);
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
            if (pawn.DevelopmentalStage.Baby()) return 0;
            if (pawn.DevelopmentalStage.Child()) return 1;
            int age = pawn.ageTracker?.AgeBiologicalYears ?? 18;
            if (age >= StageAgeMin[5]) return 5;
            if (age >= StageAgeMin[4]) return 4;
            if (age >= StageAgeMin[3]) return 3;
            if (age >= StageAgeMin[2]) return 2;
            if (age >= StageAgeMin[1]) return 1;            
            return 0;
        }
    }
}
