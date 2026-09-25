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
        private bool storagebornRefugeeQuestEnabled = true;
        private bool matchWorldTechLevel = true;
        private bool enableByCategory = true;
        private List<string> disabledCategories = new List<string>();

        public bool StoragebornRefugeeQuestEnabled => storagebornRefugeeQuestEnabled;
        public bool MatchWorldTechLevel => matchWorldTechLevel && WorldTechLevelIntegration.IsAvailable;
        public bool EnableByCategory => enableByCategory;
        public bool IsCategoryEnabled(string defName) => !disabledCategories.Contains(defName);

        public bool IsBodyEffectivelyEnabled(GeneDef gene)
        {
            StoragebornCategoryDef category = StoragebornController.BodyCategory(gene);
            if (category == null) return false;
            if (MatchWorldTechLevel && WorldTechLevelIntegration.TryGetWorldTechLevel(out TechLevel worldTechLevel))
                return category.worldTechLevel <= worldTechLevel;
            if (enableByCategory)
                return IsCategoryEnabled(category.defName);
            return IsBodyEnabled(gene.defName);
        }

        public string BodyStatusReasonKey(GeneDef gene)
        {
            StoragebornCategoryDef category = StoragebornController.BodyCategory(gene);
            if (category == null) return "StoragebornSettingsReasonMissingCategory";
            if (MatchWorldTechLevel && WorldTechLevelIntegration.TryGetWorldTechLevel(out TechLevel worldTechLevel))
                return category.worldTechLevel <= worldTechLevel ? "StoragebornSettingsReasonWorldAllowed" : "StoragebornSettingsReasonWorldBlocked";
            if (enableByCategory)
                return IsCategoryEnabled(category.defName) ? "StoragebornSettingsReasonCategoryEnabled" : "StoragebornSettingsReasonCategoryDisabled";
            return IsBodyEnabled(gene.defName) ? "StoragebornSettingsReasonIndividualEnabled" : "StoragebornSettingsReasonIndividualDisabled";
        }

        public void SetMatchWorldTechLevel(bool value)
        {
            matchWorldTechLevel = value && WorldTechLevelIntegration.IsAvailable;
            if (!matchWorldTechLevel)
            {
                enableByCategory = true;
                disabledCategories.Clear();
                disabledBodyGenes.Clear();
            }
        }

        public void SetEnableByCategory(bool value)
        {
            enableByCategory = value;
        }

        public void SetCategoryEnabled(string defName, bool enabled)
        {
            if (enabled) disabledCategories.Remove(defName);
            else if (!disabledCategories.Contains(defName)) disabledCategories.Add(defName);
        }

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
            float resetButtonHeight = 34f;
            Rect bodyRect = new Rect(inRect.x, inRect.y + tabHeight, inRect.width, inRect.height - tabHeight - resetButtonHeight - 4f);
            Rect bodyViewRect = new Rect(0f, 0f, bodyRect.width - 16f, selectedTab == 0 ? BodySettingsContentHeight() : 520f);
            float tabWidth = (inRect.width - 4f) / 3f;

            if (Widgets.ButtonText(new Rect(inRect.x, inRect.y, tabWidth, tabHeight), "StoragebornSettingsBodiesTab".Translate()))
                selectedTab = 0;
            if (Widgets.ButtonText(new Rect(inRect.x + tabWidth + 2f, inRect.y, tabWidth, tabHeight), "StoragebornSettingsStagesTab".Translate()))
                selectedTab = 1;
            if (Widgets.ButtonText(new Rect(inRect.x + (tabWidth + 2f) * 2f, inRect.y, tabWidth, tabHeight), "StoragebornSettingsQuestTab".Translate()))
                selectedTab = 2;

            Widgets.BeginScrollView(bodyRect, ref scrollPosition, bodyViewRect);
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(bodyViewRect);
            if (selectedTab == 0)
                DrawBodySettings(listing);
            else if (selectedTab == 1)
                DrawStageSettings(listing);
            else
                DrawQuestSettings(listing);
            listing.End();
            Widgets.EndScrollView();

            if (Widgets.ButtonText(new Rect(inRect.x, inRect.yMax - resetButtonHeight, inRect.width, resetButtonHeight), "StoragebornSettingsResetAll".Translate()))
                ResetAllSettings();
        }

        private static float BodySettingsContentHeight()
        {
            int bodyCount = System.Linq.Enumerable.Count(StoragebornController.BodyGeneDefs());
            int categoryCount = System.Linq.Enumerable.Count(StoragebornController.BodyCategoryDefs());
            // Includes the body/category toggles, per-category headings and gene rows, and footer buttons.
            return 500f + categoryCount * 50f + bodyCount * 68f;
        }

        private void DrawBodySettings(Listing_Standard listing)
        {
            listing.Label("StoragebornSettingsBodyGenes".Translate());
            bool worldTechLevelAvailable = WorldTechLevelIntegration.IsAvailable;
            if (worldTechLevelAvailable)
            {
                bool match = MatchWorldTechLevel;
                CheckboxIndented(listing, "StoragebornSettingsMatchWorldTechLevel".Translate(), ref match);
                if (match != MatchWorldTechLevel)
                    SetMatchWorldTechLevel(match);
            }

            bool oldGuiEnabled = GUI.enabled;
            GUI.enabled = !MatchWorldTechLevel;
            bool categoryMode = enableByCategory;
            CheckboxIndented(listing, "StoragebornSettingsEnableByCategory".Translate(), ref categoryMode);
            if (categoryMode != enableByCategory)
                SetEnableByCategory(categoryMode);

            listing.Label("StoragebornSettingsCategories".Translate());
            bool categoriesEnabled = !MatchWorldTechLevel && enableByCategory;
            GUI.enabled = categoriesEnabled;
            foreach (StoragebornCategoryDef category in StoragebornController.BodyCategoryDefs())
            {
                bool enabled = IsCategoryEnabled(category.defName);
                bool updated = enabled;
                CheckboxIndented(listing, category.LabelCap, ref updated);
                if (updated != enabled)
                    SetCategoryEnabled(category.defName, updated);
            }

            GUI.enabled = !MatchWorldTechLevel && !enableByCategory;
            foreach (StoragebornCategoryDef category in StoragebornController.BodyCategoryDefs())
            {
                listing.Gap();
                listing.Label(category.LabelCap);
                foreach (GeneDef bodyGene in StoragebornController.BodyGeneDefs()
                    .Where(gene => StoragebornController.BodyCategory(gene) == category))
                    DrawBodyGene(listing, bodyGene);
            }
            GUI.enabled = oldGuiEnabled;

            listing.GapLine();
            if (listing.ButtonText("StoragebornSettingsBodyStatus".Translate()))
                Find.WindowStack.Add(new StoragebornBodyStatusWindow(this));
            if (listing.ButtonText("StoragebornSettingsRerandomize".Translate()))
                StoragebornController.RerandomizeAllStorageborn();
        }

        private void DrawQuestSettings(Listing_Standard listing)
        {
            listing.Label("StoragebornSettingsQuestTitle".Translate());
            CheckboxIndented(listing, "StoragebornSettingsQuestEnabled".Translate(), ref storagebornRefugeeQuestEnabled);
            listing.Label("StoragebornSettingsQuestDescription".Translate());
        }

        private void DrawStageSettings(Listing_Standard listing)
        {
            listing.Label("StoragebornSettingsStageGenes".Translate());
            for (int i = 0; i < stageOffsets.Length; i++)
            {
                GeneDef stageGene = StoragebornController.StageSettingsGeneDefs().ElementAtOrDefault(i);
                if (stageGene == null) continue;

                listing.Label(stageGene.LabelCap);
                Rect row = listing.GetRect(60f);
                float controlsX = row.x + 16f;
                Widgets.Label(new Rect(controlsX, row.y, 246f, 30f), "StoragebornSettingsOffset".Translate());
                string offsetText = Widgets.TextField(new Rect(controlsX + 246f, row.y, 100f, 30f), stageOffsets[i].ToString());
                if (float.TryParse(offsetText, out float offset)) stageOffsets[i] = offset;
                Widgets.Label(new Rect(controlsX, row.y + 30f, 234f, 30f), "StoragebornSettingsFactor".Translate());
                string factorText = Widgets.TextField(new Rect(controlsX + 246f, row.y + 30f, 100f, 30f), stageFactors[i].ToString());
                if (float.TryParse(factorText, out float factor)) stageFactors[i] = factor;
            }

            listing.GapLine();
            if (listing.ButtonText("StoragebornSettingsSaveStages".Translate()))
                StoragebornController.ApplyStageStatSettings(stageOffsets, stageFactors);
        }

        private void DrawBodyGene(Listing_Standard listing, GeneDef bodyGene)
        {
            Rect row = listing.GetRect(68f);
            Texture2D texture = ContentFinder<Texture2D>.Get(bodyGene.iconPath, false);
            Rect iconRect = new Rect(row.x, row.y, 64f, 64f);
            if (texture != null)
                Widgets.DrawTextureFitted(iconRect, texture, 1f);

            StoragebornBodyGenesExtension extension = bodyGene.GetModExtension<StoragebornBodyGenesExtension>();
            string geneList = extension?.genes == null || extension.genes.Count == 0
                ? "StoragebornSettingsBodyNoGenes".Translate().Resolve()
                : string.Join("\n", extension.genes.Where(gene => gene != null).Select(gene => "• " + gene.LabelCap));
            TooltipHandler.TipRegion(iconRect, geneList);

            bool enabled = IsBodyEnabled(bodyGene.defName);
            bool updated = enabled;
            Widgets.CheckboxLabeled(new Rect(row.x + 88f, row.y, row.width - 88f, row.height), bodyGene.LabelCap, ref updated);
            if (updated != enabled)
                SetBodyEnabled(bodyGene.defName, updated);
        }

        private static void CheckboxIndented(Listing_Standard listing, string label, ref bool value)
        {
            Rect row = listing.GetRect(30f);
            Widgets.CheckboxLabeled(new Rect(row.x + 16f, row.y, row.width - 16f, row.height), label, ref value);
        }

        private void ResetAllSettings()
        {
            disabledBodyGenes.Clear();
            disabledCategories.Clear();
            matchWorldTechLevel = true;
            enableByCategory = true;
            storagebornRefugeeQuestEnabled = true;
            stageOffsets = new[] { 25f, 50f, 75f, 100f, 150f };
            stageFactors = new[] { 1f, 1.5f, 2f, 2.5f, 3f };
            StoragebornController.ApplyStageStatSettings(stageOffsets, stageFactors);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref storagebornRefugeeQuestEnabled, "storagebornRefugeeQuestEnabled", true);
            Scribe_Values.Look(ref matchWorldTechLevel, "matchWorldTechLevel", true);
            Scribe_Values.Look(ref enableByCategory, "enableByCategory", true);
            Scribe_Collections.Look(ref disabledCategories, "disabledCategories", LookMode.Value);
            Scribe_Collections.Look(ref disabledBodyGenes, "disabledBodyGenes", LookMode.Value);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && disabledBodyGenes == null)
                disabledBodyGenes = new List<string>();
            if (Scribe.mode == LoadSaveMode.PostLoadInit && disabledCategories == null)
                disabledCategories = new List<string>();
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (stageOffsets == null || stageOffsets.Length != 5)
                    stageOffsets = new[] { 25f, 50f, 75f, 100f, 150f };
                if (stageFactors == null || stageFactors.Length != 5)
                    stageFactors = new[] { 1f, 1.5f, 2f, 2.5f, 3f };
            }
            for (int i = 0; i < 5; i++)
            {
                Scribe_Values.Look(ref stageOffsets[i], "stageOffset" + i, stageOffsets[i]);
                Scribe_Values.Look(ref stageFactors[i], "stageFactor" + i, stageFactors[i]);
            }
        }
    }
}
