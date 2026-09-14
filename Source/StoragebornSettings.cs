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
        private int selectedTab;
        private float[] stageOffsets = { 25f, 50f, 75f, 100f, 150f };
        private float[] stageFactors = { 1.0f, 1.5f, 2.0f, 2.5f, 3.0f };

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
            float tabHeight = 35f;
            Rect bodyRect = new Rect(inRect.x, inRect.y + tabHeight, inRect.width, inRect.height - tabHeight);
            Rect bodyViewRect = new Rect(0f, 0f, bodyRect.width - 16f, selectedTab == 0 ? 1400f : 520f);

            if (Widgets.ButtonText(new Rect(inRect.x, inRect.y, inRect.width / 2f - 2f, tabHeight), "StoragebornSettingsBodiesTab".Translate()))
                selectedTab = 0;
            if (Widgets.ButtonText(new Rect(inRect.x + inRect.width / 2f + 2f, inRect.y, inRect.width / 2f - 2f, tabHeight), "StoragebornSettingsStagesTab".Translate()))
                selectedTab = 1;

            Widgets.BeginScrollView(bodyRect, ref scrollPosition, bodyViewRect);
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(bodyViewRect);
            if (selectedTab == 0)
                DrawBodySettings(listing);
            else
                DrawStageSettings(listing);
            listing.End();
            Widgets.EndScrollView();
        }

        private void DrawBodySettings(Listing_Standard listing)
        {
            listing.Label("StoragebornSettingsBodyGenes".Translate());
            DrawBodyGroup(listing, "StoragebornSettingsFantasy", new[]
            {
                "OMW_StorageBodyMimic", "OMW_StorageBodyLuggage", "OMW_StorageBodyCube", "OMW_StorageBodyTreant", "OMW_StorageBodyGolem"
            });
            DrawBodyGroup(listing, "StoragebornSettingsModern", new[]
            {
                "OMW_StorageBodyKallax", "OMW_StorageBodyCardboard", "OMW_StorageBodySuitcase", "OMW_StorageBodyWardrobe"
            });
            DrawBodyGroup(listing, "StoragebornSettingsFuturistic", new[]
            {
                "OMW_StorageBodyMaid", "OMW_StorageBodyOrb", "OMW_StorageBodyMechLoader"
            });

            listing.GapLine();
            if (listing.ButtonText("StoragebornSettingsRerandomize".Translate()))
                StoragebornController.RerandomizeAllStorageborn();
        }

        private void DrawStageSettings(Listing_Standard listing)
        {
            listing.Label("StoragebornSettingsStageGenes".Translate());
            for (int i = 0; i < stageOffsets.Length; i++)
            {
                GeneDef stageGene = StoragebornController.StageSettingsGeneDefs().ElementAtOrDefault(i);
                if (stageGene == null) continue;

                listing.Label(stageGene.LabelCap);
                Rect row = listing.GetRect(30f);
                Widgets.Label(new Rect(row.x, row.y, 90f, row.height), "StoragebornSettingsOffset".Translate());
                string offsetText = Widgets.TextField(new Rect(row.x + 90f, row.y, 100f, row.height), stageOffsets[i].ToString());
                if (float.TryParse(offsetText, out float offset)) stageOffsets[i] = offset;
                Widgets.Label(new Rect(row.x + 205f, row.y, 90f, row.height), "StoragebornSettingsFactor".Translate());
                string factorText = Widgets.TextField(new Rect(row.x + 295f, row.y, 100f, row.height), stageFactors[i].ToString());
                if (float.TryParse(factorText, out float factor)) stageFactors[i] = factor;
            }

            listing.GapLine();
            if (listing.ButtonText("StoragebornSettingsSaveStages".Translate()))
                StoragebornController.ApplyStageStatSettings(stageOffsets, stageFactors);
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
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (stageOffsets == null || stageOffsets.Length != 5)
                    stageOffsets = new[] { 25f, 50f, 75f, 100f, 150f };
                if (stageFactors == null || stageFactors.Length != 5)
                    stageFactors = new[] { 0.7f, 1f, 1.2f, 1.5f, 2f };
            }
            for (int i = 0; i < 5; i++)
            {
                Scribe_Values.Look(ref stageOffsets[i], "stageOffset" + i, stageOffsets[i]);
                Scribe_Values.Look(ref stageFactors[i], "stageFactor" + i, stageFactors[i]);
            }
        }
    }
}
