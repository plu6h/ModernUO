using ModernUO.Serialization;
using Server.SkillHandlers;

namespace Server.Items;

[SerializationGenerator(0, false)]
public partial class SmokeBomb : Item
{
    [Constructible]
    public SmokeBomb() : base(0x2808) => Stackable = Core.ML;

    public override double DefaultWeight => 1.0;

    public override void OnDoubleClick(Mobile from)
    {
        if (!IsChildOf(from.Backpack))
        {
            // The item must be in your backpack to use it.
            from.SendLocalizedMessage(1060640);
        }
        else if (from.Skills.Ninjitsu.Value < 50.0)
        {
            // You need at least ~1_SKILL_REQUIREMENT~ ~2_SKILL_NAME~ skill to use that ability.
            from.SendLocalizedMessage(1063013, "50\tNinjitsu");
        }
        else if (Core.TickCount - from.NextSkillTime < 0)
        {
            // You must wait a few seconds before you can use that item.
            from.SendLocalizedMessage(1070772);
        }
        else if (from.Mana < 10)
        {
            // You don't have enough mana to do that.
            from.SendLocalizedMessage(1049456);
        }
        else
        {
            Hiding.CombatOverride = true;

            if (from.UseSkill(SkillName.Hiding))
            {
                from.Mana -= 10;

                from.FixedParticles(0x3709, 1, 30, 9904, 1108, 6, EffectLayer.RightFoot);
                from.PlaySound(0x22F);

                Consume();
            }

            Hiding.CombatOverride = false;
        }
    }
}
