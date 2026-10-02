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
        private Dictionary<string, float> bodyWeights = new Dictionary<string, float>();
        private Vector2 scrollPosition;
        private int selectedTab;
        private float[] stageOffsets = { 25f, 50f, 75f, 100f, 150f };
        private float[] stageFactors = { 1.0f, 1.5f, 2.0f, 2.5f, 3.0f };
        private bool storagebornRefugeeQuestEnabled = true;
        private int bodySelectionMode = 0;
        private List<string> disabledCategories = new List<string>();

        public bool StoragebornRefugeeQuestEnabled => storagebornRefugeeQuestEnabled;
        private enum BodySelectionMode { PerCategory = 0, PerBody = 1, WorldTechLevel = 2 }
        private BodySelectionMode SelectionMode => (BodySelectionMode)Mathf.Clamp(bodySelectionMode, 0, 2);
        public bool MatchWorldTechLevel => SelectionMode == BodySelectionMode.WorldTechLevel && WorldTechLevelIntegration.IsAvailable;
        public bool EnableByCategory => SelectionMode == BodySelectionMode.PerCategory;
        public bool IsCategoryEnabled(string defName) => !disabledCategories.Contains(defName);

        public bool IsBodyEffectivelyEnabled(GeneDef gene)
        {
            StoragebornCategoryDef category = StoragebornController.BodyCategory(gene);
            if (category == null) return false;
            if (MatchWorldTechLevel && WorldTechLevelIntegration.TryGetWorldTechLevel(out TechLevel worldTechLevel))
                return category.worldTechLevel <= worldTechLevel && GetBodyWeight(gene.defName) > 0f;
            if (EnableByCategory && !IsCategoryEnabled(category.defName))
                return false;
            return GetBodyWeight(gene.defName) > 0f;
        }

        public string BodyStatusReasonKey(GeneDef gene)
        {
            StoragebornCategoryDef category = StoragebornController.BodyCategory(gene);
            if (category == null) return "StoragebornSettingsReasonMissingCategory";
            if (MatchWorldTechLevel && WorldTechLevelIntegration.TryGetWorldTechLevel(out TechLevel worldTechLevel))
                return category.worldTechLevel > worldTechLevel ? "StoragebornSettingsReasonWorldBlocked" : GetBodyWeight(gene.defName) > 0f ? "StoragebornSettingsReasonWorldAllowed" : "StoragebornSettingsReasonIndividualDisabled";
            if (EnableByCategory && !IsCategoryEnabled(category.defName))
                return "StoragebornSettingsReasonCategoryDisabled";
            return GetBodyWeight(gene.defName) > 0f ? "StoragebornSettingsReasonIndividualEnabled" : "StoragebornSettingsReasonIndividualDisabled";
        }

        private void SetSelectionMode(BodySelectionMode mode)
        {
            if (mode == BodySelectionMode.WorldTechLevel && !WorldTechLevelIntegration.IsAvailable)
            {
                mode = (int)BodySelectionMode.PerCategory;
            }
            if (mode == BodySelectionMode.PerCategory || mode == BodySelectionMode.WorldTechLevel)
            {
                bodyWeights.Clear();
                disabledBodyGenes.Clear();
            }
            bodySelectionMode = (int)mode;
        }

        public void SetCategoryEnabled(string defName, bool enabled)
        {
            if (enabled) disabledCategories.Remove(defName);
            else if (!disabledCategories.Contains(defName)) disabledCategories.Add(defName);
        }

        public float GetBodyWeight(string defName)
        {
            if (bodyWeights.TryGetValue(defName, out float weight))
                return Mathf.Clamp01(weight);
            return disabledBodyGenes.Contains(defName) ? 0f : 1f;
        }

        public void SetBodyWeight(string defName, float weight)
        {
            bodyWeights[defName] = Mathf.Clamp01(weight);
        }

        public bool IsBodyEnabled(string defName) => GetBodyWeight(defName) > 0f;

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
            return 420f + categoryCount * 54f + bodyCount * 68f;
        }

        private void DrawBodySettings(Listing_Standard listing)
        {
            listing.Label("StoragebornSettingsBodyGenes".Translate());
            Rect modeRow = listing.GetRect(32f);
            Widgets.Label(new Rect(modeRow.x, modeRow.y, 150f, modeRow.height), "StoragebornSettingsSelectionMode".Translate());
            if (Widgets.ButtonText(new Rect(modeRow.x + 154f, modeRow.y, modeRow.width - 154f, modeRow.height), SelectionModeLabel(SelectionMode)))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>
                {
                    new FloatMenuOption(SelectionModeLabel(BodySelectionMode.PerCategory), () => SetSelectionMode(BodySelectionMode.PerCategory)),
                    new FloatMenuOption(SelectionModeLabel(BodySelectionMode.PerBody), () => SetSelectionMode(BodySelectionMode.PerBody))
                };
                if (WorldTechLevelIntegration.IsAvailable)
                    options.Add(new FloatMenuOption(SelectionModeLabel(BodySelectionMode.WorldTechLevel), () => SetSelectionMode(BodySelectionMode.WorldTechLevel)));
                Find.WindowStack.Add(new FloatMenu(options));
            }

            bool oldGuiEnabled = GUI.enabled;
            foreach (StoragebornCategoryDef category in StoragebornController.BodyCategoryDefs())
            {
                List<GeneDef> categoryGenes = StoragebornController.BodyGeneDefs()
                    .Where(gene => StoragebornController.BodyCategory(gene) == category).ToList();
                if (SelectionMode == BodySelectionMode.WorldTechLevel &&
                    (!WorldTechLevelIntegration.TryGetWorldTechLevel(out TechLevel worldTechLevel) || category.worldTechLevel > worldTechLevel)) continue;
                listing.Gap();
                if (SelectionMode == BodySelectionMode.PerCategory)
                {
                    bool enabled = IsCategoryEnabled(category.defName);
                    bool updated = enabled;
                    CheckboxIndented(listing, category.LabelCap, ref updated);
                    if (updated != enabled) SetCategoryEnabled(category.defName, updated);
                }
                else listing.Label(category.LabelCap);
                foreach (GeneDef bodyGene in categoryGenes)
                {
                    DrawBodyGene(listing, bodyGene, SelectionMode == BodySelectionMode.PerBody);
                }
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

        private string SelectionModeLabel(BodySelectionMode mode)
        {
            switch (mode)
            {
                case BodySelectionMode.PerBody: return "StoragebornSettingsPerBody".Translate();
                case BodySelectionMode.WorldTechLevel: return "StoragebornSettingsFollowWorldTechLevel".Translate();
                default: return "StoragebornSettingsPerCategory".Translate();
            }
        }

        private void DrawBodyGene(Listing_Standard listing, GeneDef bodyGene, bool showProbability)
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

            float weight = GetBodyWeight(bodyGene.defName);
            Widgets.Label(new Rect(row.x + 88f, row.y + 5f, row.width - 100f, 24f), bodyGene.LabelCap);
            if (showProbability)
            {
                Widgets.Label(new Rect(row.xMax - 64f, row.y + 5f, 58f, 24f), weight.ToString("0.00"));
                float updatedWeight = Widgets.HorizontalSlider(new Rect(row.x + 88f, row.y + 32f, row.width - 152f, 20f), weight, 0f, 1f, false);
                if (!Mathf.Approximately(updatedWeight, weight)) SetBodyWeight(bodyGene.defName, updatedWeight);
            }
        }

        private static void CheckboxIndented(Listing_Standard listing, string label, ref bool value)
        {
            Rect row = listing.GetRect(30f);
            Widgets.CheckboxLabeled(new Rect(row.x + 16f, row.y, row.width - 16f, row.height), label, ref value);
        }

        private void ResetAllSettings()
        {
            disabledBodyGenes.Clear();
            bodyWeights.Clear();
            disabledCategories.Clear();
            bodySelectionMode = (int)BodySelectionMode.PerCategory;
            storagebornRefugeeQuestEnabled = true;
            stageOffsets = new[] { 25f, 50f, 75f, 100f, 150f };
            stageFactors = new[] { 1f, 1.5f, 2f, 2.5f, 3f };
            StoragebornController.ApplyStageStatSettings(stageOffsets, stageFactors);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref storagebornRefugeeQuestEnabled, "storagebornRefugeeQuestEnabled", true);
            Scribe_Values.Look(ref bodySelectionMode, "bodySelectionMode", 0);
            Scribe_Collections.Look(ref disabledCategories, "disabledCategories", LookMode.Value);
            Scribe_Collections.Look(ref disabledBodyGenes, "disabledBodyGenes", LookMode.Value);
            Scribe_Collections.Look(ref bodyWeights, "bodyWeights", LookMode.Value, LookMode.Value);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && disabledBodyGenes == null)
                disabledBodyGenes = new List<string>();
            if (Scribe.mode == LoadSaveMode.PostLoadInit && bodyWeights == null)
                bodyWeights = new Dictionary<string, float>();
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
