using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using System.Drawing;

namespace PandaTeemo
{
    class Program
    {
        public const string ChampionName = "Teemo";

        //PacketCast
        public static bool PacketCast = false;

        //Spells
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static Spell Ignite;

        public static bool QReady;
        public static bool WReady;
        public static bool RReady;

        //Orbwalker
        public static Orbwalking.Orbwalker Orbwalker;
        
        //Menu
        public static Menu Config;
        private static Menu menu1;

        //Player
        private static Obj_AI_Hero Player;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;
            if (Player.BaseSkinName != ChampionName) return;

            menu1 = new Menu("PandaTeemo", ChampionName, true);

            //Spells
            Q = new Spell(SpellSlot.Q, 580);
            W = new Spell(SpellSlot.W);
            R = new Spell(SpellSlot.R, 230);

            //Summoners
            Ignite = new Spell(Player.GetSpellSlot("summonerdot"), 600);

            //Menu
            Config = new Menu("PandaTeemo", "PandaTeemo", true);

            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            //OrbWalker Sub

            var orbwalking = Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            //var lasthit = Config.AddSubMenu(new Menu("LastHit", "LastHit"));
            var combo = Config.AddSubMenu(new Menu("Combo", "Combo"));
            var harass = Config.AddSubMenu(new Menu("Harass", "Harass"));
            var laneclear = Config.AddSubMenu(new Menu("LaneClear", "LaneClear"));
            var misc = Config.AddSubMenu(new Menu("Misc", "Misc"));

            //Extra
            Config.SubMenu("Combo").AddItem(new MenuItem("qcombo", "Use Q in Combo").SetValue(true));
            combo.AddItem(new MenuItem("wcombo", "Use W in Combo").SetValue(true));

            //lasthit.AddItem(new MenuItem("farmq", "LastHit with Q").SetValue(true));

            //harass.AddItem(new MenuItem("Harass with Q", "Harass with Q").SetValue(true));

            laneclear.AddItem(new MenuItem("qClear", "LaneClear with Q").SetValue(false));

            //misc.AddItem(new MenuItem("packetCast", "Use Packets for Spells").SetValue(false));

            //Load OrbWalker
            Orbwalker = new Orbwalking.Orbwalker(orbwalking);
            Config.AddToMainMenu();
            Config.AddItem(new MenuItem("autoQ", "Automatic Q").SetValue(false));
            Config.AddItem(new MenuItem("autoW", "Automatic W").SetValue(false));
            Config.AddItem(new MenuItem("autoignite", "Automatic Ignite").SetValue(false));

            //Events
            Game.OnGameUpdate += Game_OnGameUpdate;
            Game.PrintChat("PandaTeemo WIP by KarmaPanda");
        }
        #region Combo
        public static void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (target == null || !target.IsValid)
            {
                return;
            }

            var useQ = Config.SubMenu("Combo").Item("qcombo").GetValue<bool>();
            var useW = Config.SubMenu("Combo").Item("wcombo").GetValue<bool>();

            if (Q.IsReady() && useQ)
                if (target.IsValidTarget())
                    Q.Cast(target);
            if (WReady & useW == true)
                W.Cast(true);

            if (Orbwalker.ActiveMode.ToString() == "Combo")
            {
                if (target.IsValidTarget())
                    Orbwalking.Attack = true;
                else
                    Orbwalking.Attack = false;
            }
            else
                Orbwalking.Attack = true;

        }
        #endregion
        #region Harass
        public static void Harass()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

        }
        #endregion
        #region LaneClear
        public static void LaneClear()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, 500);
            var useQ = Config.SubMenu("LaneClear").Item("qclear").GetValue<bool>();

            if(allMinions.Count > 0 & useQ)
            {
                if (allMinions[0].Health < ObjectManager.Player.GetSpellDamage(allMinions[0], SpellSlot.Q))
                    Q.CastOnUnit(allMinions[0]);
            }

        }
        #endregion
        private static void Game_OnGameUpdate(EventArgs args)
        {
            QReady = (Player.Spellbook.CanUseSpell(Q.Slot) == SpellState.Ready || Player.Spellbook.CanUseSpell(Q.Slot) == SpellState.Surpressed);
            WReady = (Player.Spellbook.CanUseSpell(W.Slot) == SpellState.Ready || Player.Spellbook.CanUseSpell(W.Slot) == SpellState.Surpressed);
            RReady = (Player.Spellbook.CanUseSpell(R.Slot) == SpellState.Ready || Player.Spellbook.CanUseSpell(R.Slot) == SpellState.Surpressed);

            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var AutoQ = Config.Item("autoQ").GetValue<bool>();
            var AutoW = Config.Item("autoW").GetValue<bool>();

            //Auto Q and W
            if (W.IsReady() && AutoW)
            {
                W.Cast(true);
            }
            else if (Q.IsReady() && AutoQ)
            {
                Q.Cast(target, true);
            }

            //Orbwalker
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                Combo();
            }
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                Harass();
            }
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                LaneClear();
            }
        }
    }
}
