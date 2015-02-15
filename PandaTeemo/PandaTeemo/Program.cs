﻿using System;
using System.Drawing;
using LeagueSharp;
using LeagueSharp.Common;

namespace PandaTeemo
{
    internal class Program
    {
        public const string ChampionName = "Teemo";

        //Spells
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static Spell Ignite;

        //Orbwalker
        public static Orbwalking.Orbwalker Orbwalker;

        //Menu
        public static Menu Config;

        //Player
        private static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }

        public static bool Packets
        {
            get { return Config.SubMenu("Misc").Item("packets").GetValue<bool>(); }
        }

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.BaseSkinName != ChampionName)
            {
                return;
            }

            //Spells
            Q = new Spell(SpellSlot.Q, 580);
            W = new Spell(SpellSlot.W);
            R = new Spell(SpellSlot.R, 230);

            R.SetSkillshot(0.1f, 75f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            //Menu
            Config = new Menu("PandaTeemo", "PandaTeemo", true);

            //TargetSelector
            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            //OrbWalker SubMenu
            var orbwalking = Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            var combo = Config.AddSubMenu(new Menu("Combo", "Combo"));
            var harass = Config.AddSubMenu(new Menu("Harass", "Harass"));
            var laneclear = Config.AddSubMenu(new Menu("LaneClear", "LaneClear"));
            var misc = Config.AddSubMenu(new Menu("Misc", "Misc"));

            //Combo Menu
            Config.SubMenu("Combo").AddItem(new MenuItem("qcombo", "Use Q in Combo").SetValue(true));
            combo.AddItem(new MenuItem("wcombo", "Use W in Combo").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("rcombo", "Kite with R in Combo").SetValue(false));

            //Harass Menu
            harass.AddItem(new MenuItem("qharass", "Harass with Q").SetValue(true));

            //LaneClear Menu
            laneclear.AddItem(new MenuItem("qclear", "LaneClear with Q").SetValue(false));
            laneclear.AddItem(new MenuItem("rclear", "LaneClear with R").SetValue(false));

            //Main Menu
            Orbwalker = new Orbwalking.Orbwalker(orbwalking);
            Config.AddToMainMenu();
            Config.AddItem(new MenuItem("autoQ", "Automatic Q").SetValue(false));
            Config.AddItem(new MenuItem("autoW", "Automatic W").SetValue(false));

            //Interrupter
            var interrupt = Config.AddSubMenu(new Menu("Interrupt", "Interrupt"));
            interrupt.AddItem(new MenuItem("intq", "Interrupt with Q").SetValue(true));

            //Misc
            Config.SubMenu("Misc").AddItem(new MenuItem("packets", "Use Packets").SetValue(false));

            //KS Menu
            var KS = Config.AddSubMenu(new Menu("KSMenu", "Kill Steal Menu"));
            KS.AddItem(new MenuItem("KSQ", "KillSteal with Q").SetValue(true));

            //Drawing Menu
            var drawing = Config.AddSubMenu(new Menu("Drawing", "Drawing"));
            drawing.AddItem(new MenuItem("drawQ", "Draw Q Range").SetValue(true));
            drawing.AddItem(new MenuItem("drawR", "Draw R Range").SetValue(true));

            //Events
            Game.OnGameUpdate += Game_OnGameUpdate;
            Interrupter2.OnInterruptableTarget += Interrupter_OnPossibleToInterrupt;
            Drawing.OnDraw += DrawingOnOnDraw;
            Game.PrintChat("<font color=\"#FF0000\"><b>PandaTeemo RELEASE by KarmaPanda</b></font>");
        }

        #region Combo

        public static void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (!target.IsValidTarget())
            {
                return;
            }

            var useQ = Config.SubMenu("Combo").Item("qcombo").GetValue<bool>();
            var useW = Config.SubMenu("Combo").Item("wcombo").GetValue<bool>();
            var useR = Config.SubMenu("Combo").Item("rcombo").GetValue<bool>();


            if (Q.IsReady() && useQ)
            {
                Q.Cast(target, Packets);
            }

            if (W.IsReady() && useW)
            {
                W.Cast(true);
            }

            if (R.IsReady() && useR)
            {
                R.Cast(target.Position, Packets);
            }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                if (target.IsValidTarget())
                {
                    Orbwalking.Attack = true;
                }
                else
                {
                    Orbwalking.Attack = false;
                }
            }
            else
            {
                Orbwalking.Attack = true;
            }
        }

        #endregion

        #region KillSteal
        public static void KSQ()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (target == null) return;

            if(Q.IsReady())
            {
                if(target.Health < Q.GetDamage(target))
                {
                    Q.Cast(target, Packets);
                }
            }
        }
        #endregion

        #region Harass

        public static void Harass()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var useQ = Config.SubMenu("Harass").Item("qharass").GetValue<bool>();

            if (!Q.IsReady() || !useQ)
            {
                return;
            }

            if (target.IsValidTarget())
            {
                Q.Cast(target, Packets);
            }
        }

        #endregion

        #region LaneClear

        public static void LaneClear()
        {
            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, 500);
            var rangedMinions = MinionManager.GetMinions(
                ObjectManager.Player.ServerPosition, Q.Range + W.Width, MinionTypes.Ranged);
            var rLocation = R.GetCircularFarmLocation(allMinions, R.Range);
            var r2Location = R.GetCircularFarmLocation(rangedMinions, R.Range);
            var useQ = Config.SubMenu("LaneClear").Item("qclear").GetValue<bool>();
            var useR = Config.SubMenu("LaneClear").Item("rclear").GetValue<bool>();
            var bestLocation = (rLocation.MinionsHit > r2Location.MinionsHit + 1) ? rLocation : r2Location;

            if (allMinions.Count > 0 & useQ)
            {
                if (allMinions[0].Health < ObjectManager.Player.GetSpellDamage(allMinions[0], SpellSlot.Q) &&
                    Q.IsReady())
                {
                    Q.CastOnUnit(allMinions[0], Packets);
                }
            }

            if (!(allMinions.Count > 0 & useR))
            {
                return;
            }

            if (allMinions[0].Health < ObjectManager.Player.GetSpellDamage(allMinions[0], SpellSlot.R) && R.IsReady())
            {
                R.Cast(bestLocation.Position, true);
            }
        }

        #endregion

        private static void Interrupter_OnPossibleToInterrupt(Obj_AI_Hero sender,
            Interrupter2.InterruptableTargetEventArgs args)
        {
            var intq = Config.SubMenu("Interrupt").Item("intq").GetValue<bool>();

            if (intq & Q.IsReady() || args.DangerLevel != Interrupter2.DangerLevel.High)
            {
                Q.Cast(sender, Packets);
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var autoQ = Config.Item("autoQ").GetValue<bool>();
            var autoW = Config.Item("autoW").GetValue<bool>();

            //Auto Q and W
            if (W.IsReady() && autoW)
            {
                W.Cast(true);
            }

            if (Q.IsReady() && autoQ)
            {
                Q.Cast(target, true, Packets);
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
            //KillSteal
            if (Config.SubMenu("KSMenu").Item("KSQ").GetValue<bool>())
            {
                KSQ();
            }
        }

        private static void DrawingOnOnDraw(EventArgs args)
        {
            var drawQ = Config.SubMenu("Drawing").Item("drawQ").GetValue<bool>();
            var drawR = Config.SubMenu("Drawing").Item("drawR").GetValue<bool>();

            var player = ObjectManager.Player.Position;

            if (drawQ)
            {
                Render.Circle.DrawCircle(player, Q.Range, Q.IsReady() ? Color.Gold : Color.Green);
            }
            if (drawR)
            {
                Render.Circle.DrawCircle(player, R.Range, R.IsReady() ? Color.Gold : Color.Green);
            }
        }
    }
}