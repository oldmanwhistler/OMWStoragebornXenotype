using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
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
