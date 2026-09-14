using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace StoragebornXenotype
{
    public class StoragebornSettings : ModSettings
    {
        private List<string> disabledBodyGenes = new List<string>();
        private Vector2 scrollPosition;

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
            Rect viewRect = new Rect(0f, 0f, inRect.width - 16f, 1400f);
            Widgets.BeginScrollView(inRect, ref scrollPosition, viewRect);

            Listing_Standard listing = new Listing_Standard();
            listing.Begin(viewRect);
            listing.Label("StoragebornSettingsBodyGenes".Translate());
            DrawBodyGroup(listing, "StoragebornSettingsFantasy", new[]
            {
                "OMW_StorageBodyMimic",
                "OMW_StorageBodyLuggage",
                "OMW_StorageBodyCube",
                "OMW_StorageBodyTreant",
                "OMW_StorageBodyGolem"
            });
            DrawBodyGroup(listing, "StoragebornSettingsModern", new[]
            {
                "OMW_StorageBodyKallax",
                "OMW_StorageBodyCardboard",
                "OMW_StorageBodySuitcase",
                "OMW_StorageBodyWardrobe"
            });
            DrawBodyGroup(listing, "StoragebornSettingsFuturistic", new[]
            {
                "OMW_StorageBodyMaid",
                "OMW_StorageBodyOrb",
                "OMW_StorageBodyMechLoader"
            });

            listing.GapLine();
            if (listing.ButtonText("StoragebornSettingsRerandomize".Translate()))
                StoragebornController.RerandomizeAllStorageborn();

            listing.End();
            Widgets.EndScrollView();
        }

        private void DrawBodyGroup(Listing_Standard listing, string labelKey, IEnumerable<string> defNames)
        {
            listing.Gap();
            listing.Label(labelKey.Translate());
            foreach (string defName in defNames)
            {
                GeneDef bodyGene = DefDatabase<GeneDef>.GetNamedSilentFail(defName);
                if (bodyGene == null) continue;

                Rect row = listing.GetRect(68f);
                Texture2D texture = StoragebornController.BodyPreviewTexture(bodyGene.defName);
                if (texture != null)
                    Widgets.DrawTextureFitted(new Rect(row.x, row.y, 64f, 64f), texture, 1f);

                bool enabled = IsBodyEnabled(bodyGene.defName);
                bool updated = enabled;
                Widgets.CheckboxLabeled(new Rect(row.x + 72f, row.y, row.width - 72f, row.height), bodyGene.LabelCap, ref updated);
                if (updated != enabled)
                    SetBodyEnabled(bodyGene.defName, updated);
            }
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
