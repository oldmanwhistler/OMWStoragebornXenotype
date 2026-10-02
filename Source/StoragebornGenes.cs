using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace StoragebornXenotype
{
    // These is the Storageborn Gene that all storageborn have
    public class StoragebornGene : Gene
    {
        public override void PostAdd()
        {
            base.PostAdd();
            if (pawn.genes.Xenotype?.defName != "omw_storageborn")
            {
                // This changes gene randomization and hybrid births to force the storageborn xenotype.
                // The hacky way body images are done with no heads won't work with a non-storageborn xenotype.
                Log.Message(
                    $"[Storageborn Xenotype] Body gene {def.defName} was added to a non-Storageborn xenotype ({pawn.genes?.Xenotype?.defName} -- resetting xenotype).");
                pawn.genes?.SetXenotypeDirect(DefDatabase<XenotypeDef>.GetNamed("omw_storageborn"));
                // SetXenotypeDirect doesn't add the bald and no beard genes if the pawn has already been generated, so we need to add them manually.
                pawn.genes?.AddGene(DefDatabase<GeneDef>.GetNamed("Hair_BaldOnly"), xenogene: false);
                pawn.genes?.AddGene(DefDatabase<GeneDef>.GetNamed("Beard_NoBeardOnly"), xenogene: false);
            }

            StoragebornController.ApplyTo(pawn);
            StoragebornController.RandomizeBodyGene(pawn);
        }
    }

    public class StoragebornSubGene : Gene
    {
        protected void ResetXenotype()
        {
            // This changes gene randomization and hybrid births to force the storageborn xenotype.

            Log.Message(
                $"[Storageborn Xenotype] Body gene {def.defName} was added to a non-Storageborn xenotype ({pawn.genes?.Xenotype?.defName} -- resetting the OMW_Storageborn gene).");
            // Remove the Storageborn gene since we are resetting the xenotype.
            foreach (Gene gene in pawn.genes.GenesListForReading)
            {
                if (gene.def.defName == "OMW_Storageborn")
                    pawn.genes?.RemoveGene(gene);
            }

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
            StoragebornBodyBackstoriesExtension backstoriesExtension = def.GetModExtension<StoragebornBodyBackstoriesExtension>();
            if (pawn.story != null && backstoriesExtension?.backgrounds != null)
            {
                List<BackstoryDef> backgrounds = backstoriesExtension.backgrounds.Where(backstory => backstory != null).ToList();
                if (backgrounds.Count > 0)
                    pawn.story.Childhood = backgrounds.RandomElement();
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

                Gene addedGene = pawn.genes.AddGene(geneDef, xenogene: false);
                if (addedGene != null)
                    addedGenes.Add(addedGene);
            }
        }

        public override void PostRemove()
        {
            base.PostRemove();

            foreach (Gene addedGene in addedGenes)
                pawn.genes.RemoveGene(addedGene);

            addedGenes.Clear();

            StoragebornBodyGenesExtension extension = def.GetModExtension<StoragebornBodyGenesExtension>();
            if (extension == null) return;

            foreach (GeneDef geneDef in extension.genes)
            {
                if (geneDef == null)
                    Log.Error($"[Storageborn Xenotype] Body gene {def.defName} has a missing gene in its StoragebornBodyGenesExtension.");
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
