using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse.Grammar;
using UnityEngine;
using Verse;

namespace StoragebornXenotype
{
    // These is the Storageborn Gene that all storageborn have
    public class StoragebornGene : Gene
    {
        private bool TraceExtensions()
        {
            return StoragebornXenotypeMod.Instance?.Settings?.TraceGeneExtensionsEnabled ?? false;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos() ?? Enumerable.Empty<Gizmo>())
                yield return gizmo;

            if (!Prefs.DevMode) yield break;

            yield return new Command_Action
            {
                defaultLabel = "StoragebornDevRerandomizeBody".Translate(),
                defaultDesc = "StoragebornDevRerandomizeBodyDesc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Icons/Abilities/OMW_StoragebornRandomizer"),
                action = () => StoragebornController.RandomizeBodyGene(pawn, force: true)
            };
        }

        public override void PostAdd()
        {
            base.PostAdd();
            if (pawn.genes.Xenotype?.defName != "omw_storageborn")
            {
                // This changes gene randomization and hybrid births to force the storageborn xenotype.
                // The hacky way body images are done with no heads won't work with a non-storageborn xenotype.
                Log.Message(
                    $"[Storageborn Xenotype] Body gene {def.defName} was added to a non-Storageborn xenotype ({pawn.genes?.Xenotype?.defName} -- resetting xenotype).");
                List<Gene> genesToAddBack = new List<Gene>();
                foreach (Gene gene in pawn.genes.GenesListForReading)
                {
                    if (gene.def.defName.Contains("OMW_StorageBody"))
                        if (TraceExtensions())
                            Log.Message($"[Storageborn Xenotype] Removing bodygene {gene.def.defName} from pawn {pawn.LabelShort}.");
                        pawn.genes?.RemoveGene(gene);
                }
                pawn.genes?.SetXenotype(DefDatabase<XenotypeDef>.GetNamed("omw_storageborn"));
                foreach (Gene gene in genesToAddBack)
                {
                    if (TraceExtensions())
                        Log.Message($"[Storageborn Xenotype] Re-adding bodygene {gene.def.defName} to pawn {pawn.LabelShort}.");
                    pawn.genes?.AddGene(gene.def, xenogene: false);
                }
            }

            StoragebornController.ApplyTo(pawn);
            StoragebornController.RandomizeBodyGene(pawn);
        }
    }

    public class StoragebornSubGene : Gene
    {
        protected bool TraceExtensions()
        {
            return StoragebornXenotypeMod.Instance?.Settings?.TraceGeneExtensionsEnabled ?? false;
        }
        
        protected void ResetXenotype()
        {
            // This changes gene randomization and hybrid births to force the storageborn xenotype.

            Log.Message(
                $"[Storageborn Xenotype] Body gene {def.defName} was added to a non-Storageborn xenotype ({pawn.genes?.Xenotype?.defName} -- resetting the OMW_Storageborn gene).");
            // Remove the Storageborn gene since we are resetting the xenotype.
            foreach (Gene gene in pawn.genes.GenesListForReading)
            {
                if (gene.def.defName == "OMW_Storageborn")
                    if (TraceExtensions())
                        Log.Message($"[Storageborn Xenotype] Removing gene {gene.def.defName} from pawn {pawn.LabelShort}.");
                    pawn.genes?.RemoveGene(gene);
            }

            if (TraceExtensions())
                Log.Message($"[Storageborn Xenotype] Adding OMW_Storageborn gene which should reset the xenotype to OMW_Storageborn for pawn {pawn.LabelShort}.");
            // this will reset the xenotype to storageborn and cause the body type to be randomized again.                
            pawn.genes.AddGene(DefDatabase<GeneDef>.GetNamed("OMW_Storageborn"), xenogene: false);
        }
    }

    // This is a specific body gene to set the body type.
    public class StoragebornBodyGene : StoragebornSubGene
    {
        private readonly List<Gene> addedGenes = new List<Gene>();

        public override void PostAdd()
        {
            base.PostAdd();
            if (pawn == null || pawn.genes == null)
            {
                Log.Error($"[Storageborn Xenotype] Body gene {def.defName} was added to a null pawn or a pawn with no genes.");
                return;
            }

            if (pawn.genes.Xenotype?.defName != "omw_storageborn")
            {
                // The hacky way body images are done with no heads won't work with a non-storageborn xenotype.
                ResetXenotype();
                return;
            }            
            StoragebornBodyNameRulesExtension nameRulesExtension = def.GetModExtension<StoragebornBodyNameRulesExtension>();
            if (nameRulesExtension?.nameRules != null && nameRulesExtension.nameRules.Count > 0 && pawn.Name is NameTriple currentName)
            {
                GrammarRequest request = new GrammarRequest();
                foreach (RulePackDef rules in nameRulesExtension.nameRules)
                {
                    if (rules == null)
                        continue;
                    if (pawn.gender == Gender.Male && rules.defName.EndsWith("_Female", System.StringComparison.Ordinal))
                        continue;
                    if (pawn.gender == Gender.Female && rules.defName.EndsWith("_Male", System.StringComparison.Ordinal))
                        continue;
                    request.Includes.Add(rules);
                }

                if (request.Includes.Count > 0)
                {
                    string generated = NameGenerator.GenerateName(request, rootKeyword: "name");
                    string[] parts = generated.Split(new[] { ' ' }, 2, System.StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 0)
                    {
                        if (TraceExtensions())
                            Log.Message($"[Storageborn Xenotype] Body gene {def.defName} generated new name {generated} for pawn {pawn.LabelShort}.");
                        pawn.Name = new NameTriple(parts[0], currentName.Nick,
                            parts.Length > 1 ? parts[1] : string.Empty);
                    }
                }
            }

            StoragebornBodyBackstoriesExtension backstoriesExtension = def.GetModExtension<StoragebornBodyBackstoriesExtension>();
            if (pawn.story != null && backstoriesExtension?.backgrounds != null)
            {
                List<BackstoryDef> backgrounds = backstoriesExtension.backgrounds.Where(backstory => backstory != null).ToList();
                if (backgrounds.Count > 0)
                {
                    BackstoryDef selectedBackground = backgrounds.RandomElement();
                    if (TraceExtensions())
                        Log.Message($"[Storageborn Xenotype] Body gene {def.defName} selected background {selectedBackground.defName} for pawn {pawn.LabelShort}.");
                    pawn.story.Childhood = selectedBackground;
                }
            }

            StoragebornBodyGenesExtension extension = def.GetModExtension<StoragebornBodyGenesExtension>();
            if (extension == null) return;

            foreach (GeneDef geneDef in extension.genes)
            {
                if (geneDef == null)
                {
                    Log.Error($"[Storageborn Xenotype] Body gene {def.defName} has a missing gene in its StoragebornBodyGenesExtension.");
                    continue;
                }

                if (TraceExtensions())
                    Log.Message($"[Storageborn Xenotype] Applying gene extension: body gene {def.defName} adds {geneDef.defName} to pawn {pawn.LabelShort}.");

                Gene addedGene = pawn.genes.AddGene(geneDef, xenogene: false);
                if (addedGene != null)
                {
                    addedGenes.Add(addedGene);
                    if (TraceExtensions())
                        Log.Message($"[Storageborn Xenotype] Applied gene extension: {geneDef.defName} added to pawn {pawn.LabelShort}.");
                }
                else if (TraceExtensions())
                {
                    Log.Message($"[Storageborn Xenotype] Gene extension produced no gene instance: {geneDef.defName} for pawn {pawn.LabelShort}.");
                }
            }
        }

        public override void PostRemove()
        {
            base.PostRemove();

            foreach (Gene addedGene in addedGenes)
            {
                if (TraceExtensions())
                    Log.Message($"[Storageborn Xenotype] Removing gene {addedGene.def.defName} from pawn {pawn.LabelShort} due to removal of body gene {def.defName}.");
                pawn.genes.RemoveGene(addedGene);
            }

            addedGenes.Clear();

            StoragebornBodyGenesExtension extension = def.GetModExtension<StoragebornBodyGenesExtension>();
            if (extension == null) return;

            foreach (GeneDef geneDef in extension.genes)
            {
                if (geneDef == null)
                {
                    Log.Error($"[Storageborn Xenotype] Body gene {def.defName} has a missing gene in its StoragebornBodyGenesExtension.");
                }
            }
        }
    }

    public class StoragebornSizeGene : StoragebornSubGene
    {
        public override bool Active
        {
            get
            {
                return base.Active && StoragebornController.HasStoragebornGene(pawn);
            }
        }

        public override void PostAdd()
        {
            base.PostAdd();
            if (pawn == null || pawn.genes == null)
            {
                Log.Error(
                    $"[Storageborn Xenotype] Storage capacity gene {def.defName} was added to a null pawn or a pawn with no genes.");
                return;
            }

            if (pawn.genes.Xenotype?.defName != "omw_storageborn")
            {
                // The hacky way body images are done with no heads won't work with a non-storageborn xenotype.
                ResetXenotype();                
            }
        }
    }

}
