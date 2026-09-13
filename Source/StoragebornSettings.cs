using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace StoragebornXenotype
{
    public class StoragebornSettings : ModSettings
    {
        private List<string> disabledBodyGenes = new List<string>();

        public bool IsBodyEnabled(string defName)
        {
            return !disabledBodyGenes.Contains(defName);
        }

        public void SetBodyEnabled(string defName, bool enabled)
        {
            if (enabled)
                disabledBodyGenes.Remove(defName);
            else if (!disabledBodyGenes.Contains(defName))
                disabledBodyGenes.Add(defName);
        }

        public void DoWindowContents(UnityEngine.Rect inRect)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);
            listing.Label("StoragebornSettingsBodyGenes".Translate());

            foreach (GeneDef bodyGene in StoragebornController.BodyGeneDefs())
            {
                bool enabled = IsBodyEnabled(bodyGene.defName);
                bool updated = enabled;
                listing.CheckboxLabeled(bodyGene.LabelCap, ref updated);
                if (updated != enabled)
                    SetBodyEnabled(bodyGene.defName, updated);
            }

            listing.GapLine();
            if (listing.ButtonText("StoragebornSettingsRerandomize".Translate()))
                StoragebornController.RerandomizeAllStorageborn();

            listing.End();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref disabledBodyGenes, "disabledBodyGenes", LookMode.Value);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && disabledBodyGenes == null)
                disabledBodyGenes = new List<string>();
        }
    }
}
