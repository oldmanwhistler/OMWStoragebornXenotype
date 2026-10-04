using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace StoragebornXenotype
{
    [StaticConstructorOnStartup]
    public static class StoragebornController
    {
        private const string StoragebornGeneDefName = "OMW_Storageborn";
        private const string ChildhoodBackstoryDefName = "OMW_StoragebornChildhood";
        
        private static readonly string[] ArmorDefNames =
        {
            "BS_ToughSkin",
            "BS_ToughSkin",
            "BS_NaturalArmor",
            "BS_NaturalArmor",
            "BS_NaturalArmor_Great",
            "BS_NaturalArmor_Great"
        };

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

        private static readonly string[] StageSettingsGeneDefNames =
        {
            "OMW_StorageStage_0",
            "OMW_StorageStage_1",
            "OMW_StorageStage_2",
            "OMW_StorageStage_3",
            "OMW_StorageStage_4"
        };

        private static readonly List<string> BodyCategoryDefNames = new List<string>();
        private static readonly List<string> BodyGeneDefNames = new List<string>();

        static StoragebornController()
        {
            BodyCategoryDefNames.AddRange(DefDatabase<StoragebornCategoryDef>.AllDefsListForReading
                .Select(def => def.defName));
            BodyGeneDefNames.AddRange(DefDatabase<GeneDef>.AllDefsListForReading
                .Where(def => BodyCategory(def) != null)
                .Select(def => def.defName));
        }

        public static IEnumerable<StoragebornCategoryDef> BodyCategoryDefs()
        {
            foreach (string defName in BodyCategoryDefNames)
            {
                StoragebornCategoryDef def = DefDatabase<StoragebornCategoryDef>.GetNamedSilentFail(defName);
                if (def != null)
                    yield return def;
            }
        }

        public static StoragebornCategoryDef BodyCategory(GeneDef gene)
        {
            return gene?.GetModExtension<StoragebornBodyCategoryExtension>()?.OMW_StoragebornCategory!;
        }

        public static IEnumerable<GeneDef> BodyGeneDefs()
        {
            foreach (string defName in BodyGeneDefNames)
            {
                GeneDef def = DefDatabase<GeneDef>.GetNamedSilentFail(defName);
                if (def != null)
                    yield return def;
            }
        }


        public static IEnumerable<GeneDef> StageSettingsGeneDefs()
        {
            foreach (string defName in StageSettingsGeneDefNames)
            {
                GeneDef def = DefDatabase<GeneDef>.GetNamedSilentFail(defName);
                if (def != null)
                    yield return def;
            }
        }

        public static void ApplyStageStatSettings(float[] offsets, float[] factors)
        {
            StatDef carryingCapacity = StatDefOf.CarryingCapacity;
            StatDef vefMassCarryCapacity = DefDatabase<StatDef>.GetNamedSilentFail("VEF_MassCarryCapacity");

            for (int i = 0; i < StageSettingsGeneDefNames.Length; i++)
            {
                GeneDef gene = DefDatabase<GeneDef>.GetNamedSilentFail(StageSettingsGeneDefNames[i]);
                if (gene == null) continue;

                gene.statOffsets = SetStatModifier(gene.statOffsets, carryingCapacity, offsets[i]);
                gene.statFactors = SetStatModifier(gene.statFactors, carryingCapacity, factors[i]);
                if (vefMassCarryCapacity != null)
                {
                    gene.statOffsets = SetStatModifier(gene.statOffsets, vefMassCarryCapacity, offsets[i]);
                    gene.statFactors = SetStatModifier(gene.statFactors, vefMassCarryCapacity, factors[i]);
                }
            }
        }

        private static List<StatModifier> SetStatModifier(List<StatModifier> modifiers, StatDef stat, float value)
        {
            modifiers ??= new List<StatModifier>();
            modifiers.RemoveAll(modifier => modifier.stat == stat);
            modifiers.Add(new StatModifier { stat = stat, value = value });
            return modifiers;
        }

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
                if (childhood != null) pawn.story.Childhood = childhood;
            }

            RemoveBabyApparel(pawn);
            SetStage(pawn);
        }

        private static void RemoveBabyApparel(Pawn pawn)
        {
            if (!pawn.DevelopmentalStage.Baby() || pawn.apparel == null) return;

            foreach (Apparel apparel in pawn.apparel.WornApparel.ToList())
                pawn.apparel.Remove(apparel);
        }

        public static void RandomizeBodyGene(Pawn pawn, bool force = false)
        {
            if (pawn?.genes == null || !HasStoragebornGene(pawn)) return;

            StoragebornSettings settings = StoragebornXenotypeMod.Instance.Settings;
            List<GeneDef> enabled = BodyGeneDefs()
                .Where(def => settings.IsBodyEffectivelyEnabled(def))
                .Where(def => settings == null || settings.GetBodyWeight(def.defName) > 0f)
                .ToList();
            List<Gene> existing = pawn.genes.GenesListForReading
                .Where(g => BodyGeneDefNames.Contains(g.def.defName))
                .ToList();
            Gene activeGene = existing.FirstOrDefault(g => enabled.Contains(g.def));

            if (!force && activeGene != null)
            {
                foreach (Gene duplicate in existing.Where(g => g != activeGene).ToList())
                    pawn.genes.RemoveGene(duplicate);
                return;
            }

            GeneDef selectedGeneDef = DefDatabase<GeneDef>.GetNamedSilentFail("OMW_StorageBodyWardrobe");
            if (selectedGeneDef == null)
            {
                Log.Error(
                    "[Storageborn Xenotype] Fallback body gene OMW_StorageBodyWardrobe could not be found; existing body genes were left unchanged.");
                return;
            }

            bool selectedSelected = false;
            float totalWeight = enabled.Sum(def => settings?.GetBodyWeight(def.defName) ?? 1f);
            if (totalWeight > 0f)
            {
                float roll = Rand.Value * totalWeight;
                foreach (GeneDef candidate in enabled)
                {
                    roll -= settings?.GetBodyWeight(candidate.defName) ?? 1f;
                    if (roll < 0f)
                    {
                        selectedGeneDef = candidate;
                        selectedSelected = true;
                        break;
                    }
                }
                if (!selectedSelected) selectedGeneDef = enabled[enabled.Count - 1];
            }

            if (selectedSelected == false)
            {
                Log.Error("[Storageborn Xenotype] No eligible Storageborn body subtype had a positive relative weight; defaulting to OMW_StorageBodyWardrobe.");
            }

            foreach (Gene gene in existing)
                pawn.genes.RemoveGene(gene);

            pawn.genes.AddGene(selectedGeneDef, xenogene: false);
        }

        public static void RerandomizeAllStorageborn()
        {
            HashSet<Pawn> pawns = new HashSet<Pawn>();

            foreach (Map map in Find.Maps)
                foreach (Pawn pawn in map.mapPawns.AllPawns)
                    pawns.Add(pawn);

            if (Find.WorldObjects != null)
                foreach (Caravan caravan in Find.WorldObjects.Caravans)
                    foreach (Pawn pawn in caravan.PawnsListForReading)
                        pawns.Add(pawn);

            if (Find.WorldPawns != null)
                foreach (Pawn pawn in Find.WorldPawns.AllPawnsAliveOrDead)
                    pawns.Add(pawn);

            foreach (Pawn pawn in pawns)
                if (HasStoragebornGene(pawn))
                    RandomizeBodyGene(pawn, force: true);
        }

        public static void SetStage(Pawn pawn)
        {
            if (pawn?.genes == null || !HasStoragebornGene(pawn)) return;
            int targetIndex = GetStageIndex(pawn);
            SetAgeStage(pawn, targetIndex);
            SetArmorStage(pawn, targetIndex);
        }

        public static void SetAgeStage(Pawn pawn, int targetIndex)
        {            
            
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

                pawn.genes.AddGene(targetDef, xenogene: false);
            }
        }

        public static void SetArmorStage(Pawn pawn, int targetIndex)
        {
            GeneDef targetDef = DefDatabase<GeneDef>.GetNamedSilentFail(ArmorDefNames[targetIndex]);
            if (targetDef == null) return;

            List<Gene> existingArmor = pawn.genes.GenesListForReading
                .Where(g => ArmorDefNames.Contains(g.def.defName))
                .ToList();


            bool alreadyCorrect = existingArmor.Count == 1 && existingArmor[0].def == targetDef;
            if (!alreadyCorrect)
            {
                foreach (Gene gene in existingArmor)
                    pawn.genes.RemoveGene(gene);

                pawn.genes.AddGene(targetDef, xenogene: false);
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
