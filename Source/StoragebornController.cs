using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace StoragebornXenotype
{
    public static class StoragebornController
    {
        private const string StoragebornGeneDefName = "OMW_Storageborn";
        private const string ChildhoodBackstoryDefName = "OMW_StoragebornChildhood";
        private const string AdulthoodBackstoryDefName = "OMW_StoragebornWanderer";
        
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

        private static readonly string[] BodyGeneDefNames =
        {
            "OMW_StorageBodyKallax",
            "OMW_StorageBodyLuggage",
            "OMW_StorageBodyMimic",
            "OMW_StorageBodyCardboard",
            "OMW_StorageBodyMaid",
            "OMW_StorageBodyCube",
            "OMW_StorageBodyTreant",
            "OMW_StorageBodyGolem",
            "OMW_StorageBodySuitcase",
            "OMW_StorageBodyWardrobe",
            "OMW_StorageBodyOrb",
            "OMW_StorageBodyMechLoader",
            "OMW_StorageBodyCube2",
            "OMW_StorageBodyRobot",
            "OMW_StorageBodyRobot2"
        };

        public static IEnumerable<GeneDef> BodyGeneDefs()
        {
            foreach (string defName in BodyGeneDefNames)
            {
                GeneDef def = DefDatabase<GeneDef>.GetNamedSilentFail(defName);
                if (def != null)
                    yield return def;
            }
        }

        public static UnityEngine.Texture2D BodyPreviewTexture(string defName)
        {
            switch (defName)
            {
                case "OMW_StorageBodyKallax":
                    return ContentFinder<Texture2D>.Get("Storageborn/Bodies/Kallax/Storageborn_south", false);
                case "OMW_StorageBodyLuggage":
                    return ContentFinder<Texture2D>.Get("Storageborn/Bodies/Luggage/Storageborn_south", false);
                case "OMW_StorageBodyMimic":
                    return ContentFinder<Texture2D>.Get("Storageborn/Bodies/Mimic/Storageborn_south", false);
                case "OMW_StorageBodyCardboard":
                    return ContentFinder<Texture2D>.Get("Storageborn/Bodies/Cardboard/Storageborn_south", false);
                case "OMW_StorageBodyMaid":
                    return ContentFinder<Texture2D>.Get("Storageborn/Bodies/Maid/Storageborn_south", false);
                case "OMW_StorageBodyCube":
                    return ContentFinder<Texture2D>.Get("Storageborn/Bodies/Cube/Storageborn_south", false);
                case "OMW_StorageBodyTreant":
                    return ContentFinder<Texture2D>.Get("Storageborn/Bodies/Treant/Storageborn_south", false);
                case "OMW_StorageBodyGolem":
                    return ContentFinder<Texture2D>.Get("Storageborn/Bodies/Golem/Storageborn_south", false);
                case "OMW_StorageBodySuitcase":
                    return ContentFinder<Texture2D>.Get("Storageborn/Bodies/Suitcase/Storageborn_south", false);
                case "OMW_StorageBodyWardrobe":
                    return ContentFinder<Texture2D>.Get("Storageborn/Bodies/Wardrobe/Storageborn_south", false);
                case "OMW_StorageBodyOrb":
                    return ContentFinder<Texture2D>.Get("Storageborn/Bodies/Orb/Storageborn_south", false);
                case "OMW_StorageBodyMechLoader":
                    return ContentFinder<Texture2D>.Get("Storageborn/Bodies/MechLoader/Storageborn_south", false);
                case "OMW_StorageBodyCube2":
                    return ContentFinder<Texture2D>.Get("Storageborn/Bodies/Cube2/Storageborn_south", false);
                case "OMW_StorageBodyRobot":
                    return ContentFinder<Texture2D>.Get("Storageborn/Bodies/Robot/Storageborn_south", false);
                case "OMW_StorageBodyRobot2":
                    return ContentFinder<Texture2D>.Get("Storageborn/Bodies/Robot2/Storageborn_south", false);
                default:
                    return null!;
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
                BackstoryDef adulthood = DefDatabase<BackstoryDef>.GetNamedSilentFail(AdulthoodBackstoryDefName);
                if (childhood != null) pawn.story.Childhood = childhood;
                if (adulthood != null) pawn.story.Adulthood = adulthood;
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

        public static void RandomizeBodyGene(Pawn pawn)
        {
            if (pawn?.genes == null || !HasStoragebornGene(pawn)) return;

            List<GeneDef> enabled = BodyGeneDefs()
                .Where(def => StoragebornXenotypeMod.Instance?.Settings?.IsBodyEnabled(def.defName) ?? true)
                .ToList();
            if (enabled.Count == 0) return;

            GeneDef selected = enabled.RandomElement();
            List<Gene> existing = pawn.genes.GenesListForReading
                .Where(g => BodyGeneDefNames.Contains(g.def.defName))
                .ToList();

            foreach (Gene gene in existing)
                pawn.genes.RemoveGene(gene);

            pawn.genes.AddGene(selected, xenogene: false);
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
                    RandomizeBodyGene(pawn);
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
