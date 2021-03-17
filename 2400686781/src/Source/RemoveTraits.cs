using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;

namespace RemoveTraits
{
    public class RemoveTraitsSettings : ModSettings
    {
        public bool notifyWhenSuccess = true;
        public Dictionary<string, bool> disableDict = new Dictionary<string, bool>();
        public bool displayBool = false;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref disableDict, "disableDict");
        }
    }
    class RemoveTraitsMod : Mod
    {
        //private bool recheckDefList = true;
        public static Vector2 scrollPosition = new Vector2();
        public static RemoveTraitsSettings settings;
        public RemoveTraitsMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<RemoveTraitsSettings>();
        }
        public override string SettingsCategory() => "Remove Traits";
        public override void DoSettingsWindowContents(Rect inRect) {

            Listing_Standard ls = new Listing_Standard();
            Rect rect2 = new Rect(0f, 0f, inRect.width/2 - 16f, inRect.height + DefDatabase<TraitDef>.AllDefs.Count() * 27);
            Widgets.BeginScrollView(inRect, ref scrollPosition, rect2, true);
            ls.Begin(rect2);

            ls.CheckboxLabeled("Notify when surgery succeed", ref settings.notifyWhenSuccess);
            ls.Label("Keep Trait List", -1, "Check the trait to prevent being removed in midgame.");
            ls.GapLine();

            foreach (TraitDef def in DefDatabase<TraitDef>.AllDefsListForReading)
            {
                foreach (TraitDegreeData i in def.degreeDatas)
                {
                    string uniqueTrait = def.defName + "(" + i.degree + ")";
                    bool index = settings.disableDict.TryGetValue(uniqueTrait, false);
                    ls.CheckboxLabeled(uniqueTrait, ref index);
                    settings.disableDict[uniqueTrait] = index;
                }
            }
            settings.Write();
            ls.End();
            Widgets.EndScrollView();
        }
    }

    public class RemoveTraits : Recipe_Surgery
    {
        public override bool AvailableOnNow(Thing thing)
        {

            if (thing is Pawn pawn)
            {
                foreach(Trait trait in pawn.story.traits.allTraits)
                {
                    //Not in disabled trait mean can be surgeryed.
                    string uniqueTrait = trait.def.defName + "(" + trait.Degree + ")";
                    if (!RemoveTraitsMod.settings.disableDict.TryGetValue(uniqueTrait, false))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            if (pawn.story.traits.allTraits.Count == 0)
            {
                Log.Message("Pawn" + pawn.Name.ToString() + " do not have any trait.");
                return;
            }

            int index = 0;
            foreach(Trait trait in pawn.story.traits.allTraits)
            {
                //Not in disabled trait mean can be surgeryed.
                string uniqueTrait = trait.def.defName + "(" + trait.Degree + ")";
                if (!RemoveTraitsMod.settings.disableDict.TryGetValue(uniqueTrait, false))
                {
                    pawn.story.traits.allTraits.RemoveAt(index);
                    // Update pawn status
                    if (!pawn.Dead && pawn.RaceProps.Humanlike)
                    {
                        pawn.needs.mood.thoughts.situational.Notify_SituationalThoughtsDirty();
                    }
                    if (pawn.skills != null)
                    {
                        pawn.skills.Notify_SkillDisablesChanged();
                    }
                    if (pawn.workSettings != null)
                    {
                        pawn.workSettings.Notify_DisabledWorkTypesChanged();
                        pawn.workSettings.Notify_UseWorkPrioritiesChanged();
                    }

                    // Notify
                    if (RemoveTraitsMod.settings.notifyWhenSuccess){
                        Messages.Message("Successfully remove " + pawn.ToString() + "'s trait. (" + uniqueTrait + ")", pawn, MessageTypeDefOf.PositiveEvent);
                    }
                    Log.Message("Removed " +uniqueTrait + " from pawn " + pawn.NameShortColored);
                    break;
                }
                index++;
            }
            return;
        }
    }
}
