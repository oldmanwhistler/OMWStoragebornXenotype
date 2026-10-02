using System.Linq;
using RimWorld;
using Verse;

namespace StoragebornXenotype
{
    public class StoragebornIdeologyWarningComponent : GameComponent
    {
        private bool warningShown;

        public StoragebornIdeologyWarningComponent(Game game)
        {
        }

        public override void StartedNewGame()
        {
            base.StartedNewGame();
            if (warningShown || !ModsConfig.IdeologyActive || !PlayerIdeologyUsesHeadgear())
                return;

            warningShown = true;
            Letter letter = LetterMaker.MakeLetter(
                "StoragebornIdeologyHeadgearLetterLabel".Translate(),
                "StoragebornIdeologyHeadgearWarning".Translate(),
                LetterDefOf.NegativeEvent);
            Find.LetterStack.ReceiveLetter(letter);
        }

        private static bool PlayerIdeologyUsesHeadgear()
        {
            if (Faction.OfPlayerSilentFail?.ideos == null)
                return false;
            Ideo ideo = Faction.OfPlayerSilentFail.ideos.PrimaryIdeo;
            if (ideo?.PreceptsListForReading == null)
                return false;

            foreach (Precept_Apparel precept in ideo.PreceptsListForReading.OfType<Precept_Apparel>())
            {
                if (precept.apparelDef == null)
                    continue;
                ApparelProperties apparel = precept.apparelDef.apparel;
                if (apparel == null || apparel.bodyPartGroups == null)
                    continue;

                foreach (BodyPartGroupDef group in apparel.bodyPartGroups)
                {
                    if (group == BodyPartGroupDefOf.UpperHead || group == BodyPartGroupDefOf.FullHead)
                        return true;
                }
            }
            return false;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref warningShown, "storagebornIdeologyHeadgearWarningShown", false);
        }
    }
}
