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
        //public static bool PacketCast = false;

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


            //Menu
            Config = new Menu("PandaTeemo", "PandaTeemo", true);

            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            //OrbWalker Sub

            var orbwalking = Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            var combo = Config.AddSubMenu(new Menu("Combo", "Combo"));
            var harass = Config.AddSubMenu(new Menu("Harass", "Harass"));
            var laneclear = Config.AddSubMenu(new Menu("LaneClear", "LaneClear"));

            //Extra
            Config.SubMenu("Combo").AddItem(new MenuItem("qcombo", "Use Q in Combo").SetValue(true));
            combo.AddItem(new MenuItem("wcombo", "Use W in Combo").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("rcombo", "Use R in Combo").SetValue(true));


            harass.AddItem(new MenuItem("qharass", "Harass with Q").SetValue(true));

            laneclear.AddItem(new MenuItem("qclear", "LaneClear with Q").SetValue(false));
            laneclear.AddItem(new MenuItem("rclear", "LaneClear with R").SetValue(false));

            //Load OrbWalker
            Orbwalker = new Orbwalking.Orbwalker(orbwalking);
            Config.AddToMainMenu();
            Config.AddItem(new MenuItem("autoQ", "Automatic Q").SetValue(false));
            Config.AddItem(new MenuItem("autoW", "Automatic W").SetValue(false));

            //Events
            Game.OnGameUpdate += Game_OnGameUpdate;
            Game.PrintChat("<font color=\"#FF0000\"><b>PandaTeemo BETA v1 by KarmaPanda<b></font>");
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
            var useR = Config.SubMenu("Combo").Item("rcombo").GetValue<bool>();


            if (Q.IsReady() && useQ)
                if (target.IsValidTarget())
                    Q.Cast(target);
            if (WReady & useW == true)
                W.Cast(true);
            if (R.IsReady() && useR)
                R.Cast(ObjectManager.Player.Position);

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
            var useQ = Config.SubMenu("Harass").Item("qharass").GetValue<bool>();

            if (Q.IsReady() && useQ)
                if (target.IsValidTarget())
                    Q.Cast(target);

        }
        #endregion
        #region LaneClear
        public static void LaneClear()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, 500);
            var rangedMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range + W.Width,
    MinionTypes.Ranged);
            var rLocation = R.GetCircularFarmLocation(allMinions, R.Range);
            var r2Location = R.GetCircularFarmLocation(rangedMinions, R.Range);
            var useQ = Config.SubMenu("LaneClear").Item("qclear").GetValue<bool>();
            var useR = Config.SubMenu("LaneClear").Item("rclear").GetValue<bool>();
            var bestLocation = (rLocation.MinionsHit > r2Location.MinionsHit + 1) ? rLocation : r2Location;

            if(allMinions.Count > 0 & useQ)
            {
                if (allMinions[0].Health < ObjectManager.Player.GetSpellDamage(allMinions[0], SpellSlot.Q) && Q.IsReady())
                    Q.CastOnUnit(allMinions[0]);
            }
            if(allMinions.Count > 0 & useR)
            {
                if (allMinions[0].Health < ObjectManager.Player.GetSpellDamage(allMinions[0], SpellSlot.R) && R.IsReady())
                {
                        R.Cast(bestLocation.Position, true);
                }
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
