using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace RemoveTraits
{
    public class RemoveTraits : Recipe_Surgery
    {
        public override bool AvailableOnNow(Thing thing)
        {
            if (thing is Pawn pawn)
            {
                if (pawn.story.traits.allTraits.Count != 0)
                {
                    return true;
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

            int index = new IntRange(0, pawn.story.traits.allTraits.Count - 1).RandomInRange;
            Log.Message("Try to remove " + pawn.story.traits.allTraits.ElementAt(index).def.defName + " from pawn " + pawn.NameShortColored);
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
            Messages.Message("Successfully remove " + pawn.ToString() + "'s trait.", pawn, MessageTypeDefOf.PositiveEvent);
            return;
        }
    }
}
