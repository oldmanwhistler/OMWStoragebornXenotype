using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace StoragebornXenotype
{
    public class StoragebornGene : Gene
    {
        public override void PostAdd()
        {
            base.PostAdd();
            StoragebornController.ApplyTo(pawn);
            StoragebornController.RandomizeBodyGene(pawn);
        }
    }

    public class StoragebornBodyGene : Gene
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
                // This changes gene randomization and hybrid births to force the storageborn xenotype.
                // The hacky way body images are done with no heads won't work with a non-storageborn xenotype.
                Log.Error($"[Storageborn Xenotype] Body gene {def.defName} was added to a non-Storageborn xenotype ({pawn.genes?.Xenotype?.defName} -- resetting xenotype).");
                pawn.genes?.SetXenotypeDirect(DefDatabase<XenotypeDef>.GetNamed("omw_storageborn"));
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

    public class StoragebornSizeGene : Gene
    {
        public override bool Active
        {
            get
            {
                return base.Active && StoragebornController.HasStoragebornGene(pawn);
            }
        }
    }

}
