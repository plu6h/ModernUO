using System.Collections.Generic;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;

namespace Server.Guilds
{
    public enum GuildDisplayType
    {
        All,
        AwaitingAction,
        Relations
    }

    public class GuildDiplomacyGump : BaseGuildListGump<Guild>
    {
        private readonly TextDefinition m_LowerText;
        private GuildDisplayType m_Display;

        public GuildDiplomacyGump(PlayerMobile pm, Guild g)
            : this(
                pm,
                g,
                NameComparer.Instance,
                true,
                "",
                0,
                GuildDisplayType.All,
                World.Guilds.Values.SafeConvertList<BaseGuild, Guild>(),
                1063136 + (int)GuildDisplayType.All
            )
        {
        }

        public GuildDiplomacyGump(
            PlayerMobile pm, Guild g, IComparer<Guild> currentComparer, bool ascending, string filter,
            int startNumber, GuildDisplayType display
        )
            : this(
                pm,
                g,
                currentComparer,
                ascending,
                filter,
                startNumber,
                display,
                World.Guilds.Values.SafeConvertList<BaseGuild, Guild>(),
                1063136 + (int)display
            )
        {
        }

        public GuildDiplomacyGump(
            PlayerMobile pm, Guild g, IComparer<Guild> currentComparer, bool ascending, string filter,
            int startNumber, List<Guild> list, TextDefinition lowerText
        )
            : this(pm, g, currentComparer, ascending, filter, startNumber, GuildDisplayType.All, list, lowerText)
        {
        }

        public GuildDiplomacyGump(
            PlayerMobile pm, Guild g, bool ascending, string filter, int startNumber, List<Guild> list,
            TextDefinition lowerText
        )
            : this(pm, g, NameComparer.Instance, ascending, filter, startNumber, GuildDisplayType.All, list, lowerText)
        {
        }

        public GuildDiplomacyGump(
            PlayerMobile pm, Guild g, IComparer<Guild> currentComparer, bool ascending, string filter,
            int startNumber, GuildDisplayType display, List<Guild> list, TextDefinition lowerText
        )
            : base(
                pm,
                g,
                list,
                currentComparer,
                ascending,
                filter,
                startNumber,
                new[]
                {
                    new InfoField<Guild>(1062954, 280, NameComparer.Instance),  // Guild Name
                    new InfoField<Guild>(1062957, 50, AbbrevComparer.Instance), // Abbrev
                    new InfoField<Guild>(1062958, 120, new StatusComparer(g))   // Guild Title
                }
            )
        {
            m_Display = display;
            m_LowerText = lowerText;
            PopulateGump();
        }

        protected virtual bool AllowAdvancedSearch => true;

        public override bool WillFilter
        {
            get
            {
                if (m_Display == GuildDisplayType.All)
                {
                    return base.WillFilter;
                }

                return true;
            }
        }

        public override void PopulateGump()
        {
            base.PopulateGump();

            AddHtmlLocalized(431, 43, 110, 26, 1062978, 0xF); // Diplomacy
        }

        protected override TextDefinition[] GetValuesFor(Guild g, int aryLength)
        {
            var defs = new TextDefinition[aryLength];

            defs[0] = g == guild ? g.Name.Color(0x006600) : g.Name;
            defs[1] = g.Abbreviation;

            defs[2] = 3000085; // Peace

            if (guild.IsAlly(g))
            {
                if (guild.Alliance.Leader == g)
                {
                    defs[2] = 1063237; // Alliance Leader
                }
                else
                {
                    defs[2] = 1062964; // Ally
                }
            }
            else if (guild.IsWar(g))
            {
                defs[2] = 3000086; // War
            }

            return defs;
        }

        public override bool HasRelationship(Guild g)
        {
            if (g == guild)
            {
                return false;
            }

            if (guild.FindPendingWar(g) != null)
            {
                return true;
            }

            var alliance = guild.Alliance;

            if (alliance != null)
            {
                var leader = alliance.Leader;

                if (leader != null)
                {
                    if (guild == leader && alliance.IsPendingMember(g) || g == leader && alliance.IsPendingMember(guild))
                    {
                        return true;
                    }
                }
                else if (alliance.IsPendingMember(g))
                {
                    return true;
                }
            }

            return false;
        }

        public override void DrawEndingEntry(int itemNumber)
        {
            // AddHtmlLocalized( 66, 153 + itemNumber * 28, 280, 26, 1063136 + (int)m_Display, 0xF, false, false ); // Showing All Guilds/Awaiting Action/ w/Relation Ship
            // AddHtmlText( 66, 153 + itemNumber * 28, 280, 26, m_LowerText, false, false );

            if (m_LowerText?.Number > 0)
            {
                AddHtmlLocalized(66, 153 + itemNumber * 28, 280, 26, m_LowerText.Number, 0xF);
            }
            else if (m_LowerText?.String != null)
            {
                AddHtml(66, 153 + itemNumber * 28, 280, 26, m_LowerText.String.Color(0x99));
            }

            if (AllowAdvancedSearch)
            {
                AddBackground(350, 148 + itemNumber * 28, 200, 26, 0x2486);
                AddButton(355, 153 + itemNumber * 28, 0x845, 0x846, 8);
                AddHtmlLocalized(380, 151 + itemNumber * 28, 160, 26, 1063083, 0x0); // Advanced Search
            }
        }

        protected override bool IsFiltered(Guild g, string filter)
        {
            if (g == null)
            {
                return true;
            }

            switch (m_Display)
            {
                case GuildDisplayType.Relations:
                    {
                        // As per OSI, only the guild leader wars show up under the sorting by relation
                        return !(guild.FindActiveWar(g) != null || guild.IsAlly(g));
                    }
                case GuildDisplayType.AwaitingAction:
                    {
                        return !HasRelationship(g);
                    }
            }

            return !(g.Name.InsensitiveContains(filter) || g.Abbreviation.InsensitiveContains(filter));
        }

        public override Gump GetResentGump(
            PlayerMobile pm, Guild g, IComparer<Guild> comparer, bool ascending,
            string filter, int startNumber
        ) =>
            new GuildDiplomacyGump(pm, g, comparer, ascending, filter, startNumber, m_Display);

        public override Gump GetObjectInfoGump(PlayerMobile pm, Guild g, Guild o)
        {
            if (guild == o)
            {
                return new GuildInfoGump(pm, g);
            }

            return new OtherGuildInfo(pm, g, o);
        }

        public override void OnResponse(NetState sender, in RelayInfo info)
        {
            base.OnResponse(sender, info);

            if (sender.Mobile is not PlayerMobile pm || !IsMember(pm, guild))
            {
                return;
            }

            if (AllowAdvancedSearch && info.ButtonID == 8)
            {
                pm.SendGump(new GuildAdvancedSearchGump(pm, guild, m_Display, AdvancedSearch_Callback));
            }
        }

        public void AdvancedSearch_Callback(GuildDisplayType display)
        {
            m_Display = display;
            ResendGump();
        }

        private class NameComparer : IComparer<Guild>
        {
            public static readonly IComparer<Guild> Instance = new NameComparer();

            public int Compare(Guild x, Guild y)
            {
                if (x == null && y == null)
                {
                    return 0;
                }

                if (x == null)
                {
                    return -1;
                }

                if (y == null)
                {
                    return 1;
                }

                return x.Name.InsensitiveCompare(y.Name);
            }
        }

        private class StatusComparer : IComparer<Guild>
        {
            private readonly Guild m_Guild;

            public StatusComparer(Guild g) => m_Guild = g;

            public int Compare(Guild x, Guild y)
            {
                if (x == null && y == null)
                {
                    return 0;
                }

                if (x == null)
                {
                    return -1;
                }

                if (y == null)
                {
                    return 1;
                }

                var aStatus = GuildCompareStatus.Peace;
                var bStatus = GuildCompareStatus.Peace;

                if (m_Guild.IsAlly(x))
                {
                    aStatus = GuildCompareStatus.Ally;
                }
                else if (m_Guild.IsWar(x))
                {
                    aStatus = GuildCompareStatus.War;
                }

                if (m_Guild.IsAlly(y))
                {
                    bStatus = GuildCompareStatus.Ally;
                }
                else if (m_Guild.IsWar(y))
                {
                    bStatus = GuildCompareStatus.War;
                }

                return ((int)aStatus).CompareTo((int)bStatus);
            }

            private enum GuildCompareStatus
            {
                Peace,
                Ally,
                War
            }
        }

        private class AbbrevComparer : IComparer<Guild>
        {
            public static readonly IComparer<Guild> Instance = new AbbrevComparer();

            public int Compare(Guild x, Guild y)
            {
                if (x == null && y == null)
                {
                    return 0;
                }

                if (x == null)
                {
                    return -1;
                }

                if (y == null)
                {
                    return 1;
                }

                return x.Abbreviation.InsensitiveCompare(y.Abbreviation);
            }
        }
    }
}
