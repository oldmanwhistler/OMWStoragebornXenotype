using RimWorld;
using UnityEngine;
using Verse;

namespace StoragebornXenotype
{
    public class StoragebornBodyStatusWindow : Window
    {
        private readonly StoragebornSettings settings;
        private Vector2 scrollPosition;

        public StoragebornBodyStatusWindow(StoragebornSettings settings)
        {
            this.settings = settings;
            doCloseX = true;
            doCloseButton = true;
            draggable = true;
            absorbInputAroundWindow = true;
            resizeable = false;
            closeOnClickedOutside = true;
        }

        public override Vector2 InitialSize => new Vector2(620f, 650f);

        public override void DoWindowContents(Rect inRect)
        {
            Rect viewRect = new Rect(0f, 0f, inRect.width - 16f, System.Linq.Enumerable.Count(StoragebornController.BodyGeneDefs()) * 58f + 30f);
            Widgets.BeginScrollView(inRect, ref scrollPosition, viewRect);
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(viewRect);
            foreach (GeneDef gene in StoragebornController.BodyGeneDefs())
            {
                bool enabled = settings.IsBodyEffectivelyEnabled(gene);
                string statusKey = enabled ? "StoragebornSettingsStatusEnabled" : "StoragebornSettingsStatusDisabled";
                listing.Label(statusKey.Translate(gene.LabelCap));
                listing.Label(GetReason(gene));
                listing.Gap(4f);
            }
            listing.End();
            Widgets.EndScrollView();
        }

        private string GetReason(GeneDef gene)
        {
            StoragebornCategoryDef category = StoragebornController.BodyCategory(gene);
            string categoryLabel = category?.LabelCap ?? "StoragebornSettingsUnknownCategory".Translate();
            string key = settings.BodyStatusReasonKey(gene);
            if (key == "StoragebornSettingsReasonWorldAllowed" || key == "StoragebornSettingsReasonWorldBlocked")
            {
                WorldTechLevelIntegration.TryGetWorldTechLevel(out TechLevel worldTechLevel);
                return key.Translate(categoryLabel, worldTechLevel.ToString()).Resolve();
            }
            if (key == "StoragebornSettingsReasonCategoryEnabled" || key == "StoragebornSettingsReasonCategoryDisabled")
                return key.Translate(categoryLabel).Resolve();
            return key.Translate().Resolve();
        }
    }
}
