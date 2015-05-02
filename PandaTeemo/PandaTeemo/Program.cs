﻿using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

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
        public static ShroomTables ShroomPositions;
        
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

            // Spells
            Q = new Spell(SpellSlot.Q, 580);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R, 230);

            R.SetSkillshot(0.1f, 75f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            // Menu
            Config = new Menu("PandaTeemo", "PandaTeemo", true);

            // TargetSelector
            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            // OrbWalker SubMenu
            var orbwalking = Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            var combo = Config.AddSubMenu(new Menu("Combo", "Combo"));
            var harass = Config.AddSubMenu(new Menu("Harass", "Harass"));
            var laneclear = Config.AddSubMenu(new Menu("LaneClear", "LaneClear"));
            var misc = Config.AddSubMenu(new Menu("Misc", "Misc"));

            // Combo Menu
            Config.SubMenu("Combo").AddItem(new MenuItem("qcombo", "Use Q in Combo").SetValue(true));
            combo.AddItem(new MenuItem("wcombo", "Use W in Combo").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("rcombo", "Kite with R in Combo").SetValue(true));

            // Harass Menu
            harass.AddItem(new MenuItem("qharass", "Harass with Q").SetValue(true));

            // LaneClear Menu
            laneclear.AddItem(new MenuItem("qclear", "LaneClear with Q").SetValue(true));
            laneclear.AddItem(new MenuItem("rclear", "LaneClear with R").SetValue(true));

            // Main Menu
            Orbwalker = new Orbwalking.Orbwalker(orbwalking);
            Config.AddToMainMenu();

            // Interrupter
            var interrupt = Config.AddSubMenu(new Menu("Interrupt", "Interrupt"));
            interrupt.AddItem(new MenuItem("intq", "Interrupt with Q").SetValue(true));

            // Misc
            misc.AddItem(new MenuItem("autoQ", "Automatic Q").SetValue(false));
            misc.AddItem(new MenuItem("autoW", "Automatic W").SetValue(false));
            misc.AddItem(new MenuItem("autoR", "Auto Place Shrooms in Important Places").SetValue(true));
            misc.AddItem(new MenuItem("packets", "Use Packets").SetValue(false));

            // KS Menu
            var ks = Config.AddSubMenu(new Menu("KSMenu", "KSMenu"));
            ks.AddItem(new MenuItem("KSQ", "KillSteal with Q").SetValue(true));

            // Drawing Menu
            var drawing = Config.AddSubMenu(new Menu("Drawing", "Drawing"));
            drawing.AddItem(new MenuItem("drawQ", "Draw Q Range").SetValue(false));
            drawing.AddItem(new MenuItem("drawR", "Draw R Range").SetValue(false));
            drawing.AddItem(new MenuItem("drawautoR", "Draw Important Shroom Areas").SetValue(true));
            drawing.AddItem(new MenuItem("DrawVision", "Shroom Vision").SetValue(new Slider(1500, 2500, 1000)));

            //Output to Console Location
            //var console = Config.AddSubMenu(new Menu("Console", "Console"));
            //console.AddItem(new MenuItem("Debug", "Debug").SetValue(new KeyBind(84, KeyBindType.Press)));
            
            // Flee Menu
            var flee = Config.AddSubMenu(new Menu("Flee Menu", "Flee"));
            flee.AddItem(new MenuItem("fleetoggle", "Flee").SetValue(new KeyBind(90, KeyBindType.Press)));
            flee.AddItem(new MenuItem("w", "Use W while Flee").SetValue(true));
            flee.AddItem(new MenuItem("r", "Use R while Flee").SetValue(true));

            // Events
            ShroomPositions = new ShroomTables();
            Game.OnUpdate += Game_OnGameUpdate;
            Interrupter2.OnInterruptableTarget += Interrupter_OnPossibleToInterrupt;
            Drawing.OnDraw += DrawingOnOnDraw;
        }

        #region IsShroom

        /// <summary>
        /// Checks if there is shroom in location
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        private static bool IsShroomed(Vector3 position)
        {
            return ObjectManager.Get<Obj_AI_Base>().Where(obj => obj.Name == "Noxious Trap").Any(obj => position.Distance(obj.Position) <= 250);
        }

        #endregion

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
                W.Cast();
            }

            if (R.IsReady() && useR && R.IsInRange(target))
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

            if (Q.IsReady() && target.IsValidTarget() && useQ && Q.IsInRange(target))
            {
                Q.Cast(target, Packets);
            }
        }

        #endregion

        #region LaneClear

        public static void LaneClear()
        {
            var allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range);

            var allMinionsR = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, R.Range);
            var rangedMinionsR = MinionManager.GetMinions(
                ObjectManager.Player.ServerPosition, R.Range, MinionTypes.Ranged);

            var rLocation = R.GetCircularFarmLocation(allMinionsR, R.Range);
            var r2Location = R.GetCircularFarmLocation(rangedMinionsR, R.Range);

            var useQ = Config.SubMenu("LaneClear").Item("qclear").GetValue<bool>();
            var useR = Config.SubMenu("LaneClear").Item("rclear").GetValue<bool>();

            var bestLocation = (rLocation.MinionsHit > r2Location.MinionsHit + 1) ? rLocation : r2Location;

            if (allMinionsQ.Count > 0 & useQ)
            {
                if (allMinionsQ[0].Health < ObjectManager.Player.GetSpellDamage(allMinionsQ[0], SpellSlot.Q) &&
                    Q.IsReady())
                {
                    Q.CastOnUnit(allMinionsQ[0], Packets);
                }
            }

            if (!(allMinionsR.Count > 0 & useR))
            {
                return;
            }

            if (allMinionsR[0].Health < ObjectManager.Player.GetSpellDamage(allMinionsR[0], SpellSlot.R) && R.IsReady() && R.IsInRange(r2Location.Position.To3D()))
            {
                R.Cast(bestLocation.Position, true);
            }

            else if (allMinionsR[0].Health < ObjectManager.Player.GetSpellDamage(allMinionsR[0], SpellSlot.R) &&
                     R.IsReady() && R.IsInRange(rLocation.Position.To3D()))
            {
                R.Cast(bestLocation.Position, true);
            }

        }

        #endregion

        #region Interrupt

        private static void Interrupter_OnPossibleToInterrupt(Obj_AI_Hero sender,
            Interrupter2.InterruptableTargetEventArgs args)
        {
            var intq = Config.SubMenu("Interrupt").Item("intq").GetValue<bool>();

            if (intq & Q.IsReady() || args.DangerLevel != Interrupter2.DangerLevel.High)
            {
                Q.Cast(sender, Packets);
            }
        }

        #endregion

        #region AutoShroom
        
        private static void AutoShroom()
        {
            if (!R.IsReady())
                return;
            
            // Multi Map Support Shrooming

            if (Utility.Map.GetMap().Type == Utility.Map.MapType.SummonersRift)
            {
                foreach (var place in ShroomPositions.SummonersRift.Where(pos => pos.Distance(ObjectManager.Player.Position) <= R.Range && !IsShroomed(pos)))
                    R.Cast(place, Packets);
            }

            else if (Utility.Map.GetMap().Type == Utility.Map.MapType.HowlingAbyss)
            {
                // WIP
                foreach (var place in ShroomPositions.HowlingAbyss.Where(pos => pos.Distance(ObjectManager.Player.Position) <= R.Range && !IsShroomed(pos)))
                    R.Cast(place, Packets);
            }

            else if (Utility.Map.GetMap().Type == Utility.Map.MapType.CrystalScar)
            {
                // WIP
                foreach (var place in ShroomPositions.CrystalScar.Where(pos => pos.Distance(ObjectManager.Player.Position) <= R.Range && !IsShroomed(pos)))
                    R.Cast(place, Packets);

            }

            else if (Utility.Map.GetMap().Type == Utility.Map.MapType.TwistedTreeline)
            {
                // WIP
                foreach (var place in ShroomPositions.TwistedTreeline.Where(pos => pos.Distance(ObjectManager.Player.Position) <= R.Range && !IsShroomed(pos)))
                    R.Cast(place, Packets);
            }

            else
            {
                return;
            }

        }

        #endregion

        #region LastHit

        static void LastHit()
        {
            double TeemoE = 0;

            var t = TargetSelector.GetTarget((float)TeemoE, TargetSelector.DamageType.Physical);

            TeemoE += Player.GetSpellDamage(t, SpellSlot.E);

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit)
            {
                var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, 30, MinionTypes.All);

                foreach (var minion in allMinions)
                {
                    if (minion.Health < ObjectManager.Player.GetAutoAttackDamage(minion) + TeemoE)
                    {
                        Orbwalking.DisableNextAttack = false;
                        Orbwalking.CanAttack();
                    }
                }
            }
            else
            {
                return;
            }
        }

        #endregion

        #region Flee

        static void Flee()
        {
            // Checks if toggle is on
            var useW = Config.SubMenu("Flee").Item("w").GetValue<bool>();
            var useR = Config.SubMenu("Flee").Item("r").GetValue<bool>();

            // Force move to player's mouse cursor
            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);

            // Uses R if avaliable and if toggle is on
            if (useR && R.IsReady())
            {
                R.Cast(Player.Position, Packets);
            }

            // Uses W if avaliable and if toggle is on
            if (useW && W.IsReady())
            {
                W.Cast(Player);
            }
        }

        #endregion

        #region Auto Q

        static void AutoQ()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All);

            if (Q.IsReady() && allMinionsQ.Count >= 1)
            {
                foreach (var minion in allMinionsQ)
                {
                    if (minion.Health <= Q.GetDamage(minion) && Q.IsInRange(minion))
                    {
                        Q.CastOnUnit(minion, Packets);
                    }
                }
            }

            else if (Q.IsReady() && Q.IsInRange(target) && target.IsValid)
            {
                Q.Cast(target, Packets);
            }

            else
            {
                switch (Orbwalker.ActiveMode)
                {
                    case Orbwalking.OrbwalkingMode.Combo:
                        Combo();
                        break;
                    case Orbwalking.OrbwalkingMode.Mixed:
                        Harass();
                        break;
                    case Orbwalking.OrbwalkingMode.LastHit:
                        Orbwalking.DisableNextAttack = true;
                        LastHit();
                        break;
                    case Orbwalking.OrbwalkingMode.LaneClear:
                        LaneClear();
                        break;
                    case Orbwalking.OrbwalkingMode.None:
                        break;
                }
            }
        }

        #endregion

        #region Auto W

        static void AutoW()
        {
            if (W.IsReady())
            {
                W.Cast(Player);
            }

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass();
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    Orbwalking.DisableNextAttack = true;
                    LastHit();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    LaneClear();
                    break;
                case Orbwalking.OrbwalkingMode.None:
                    break;
            }
        }

        #endregion

        #region Auto Q & W

        static void AutoQW()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All);

            if (W.IsReady())
            {
                W.Cast();
            }

            if (Q.IsReady())
            {
                foreach (var minion in allMinionsQ)
                {
                    if (minion.Health <= Q.GetDamage(minion) && Q.IsInRange(minion))
                    {
                        Q.CastOnUnit(minion, Packets);
                    }
                }
            }



            else if (Q.IsReady() && Q.IsInRange(target))
            {
                Q.Cast(target);
            }


            else
            {
                switch (Orbwalker.ActiveMode)
                {
                    case Orbwalking.OrbwalkingMode.Combo:
                        Combo();
                        break;
                    case Orbwalking.OrbwalkingMode.Mixed:
                        Harass();
                        break;
                    case Orbwalking.OrbwalkingMode.LastHit:
                        Orbwalking.DisableNextAttack = true;
                        LastHit();
                        break;
                    case Orbwalking.OrbwalkingMode.LaneClear:
                        LaneClear();
                        break;
                    case Orbwalking.OrbwalkingMode.None:
                        break;
                }
            }
        }

        #endregion

        static void Game_OnGameUpdate(EventArgs args)
        {
            var autoQ = Config.Item("autoQ").GetValue<bool>();
            var autoW = Config.Item("autoW").GetValue<bool>();



            // Reworked Orbwalker
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    // Reworked Auto Q and W
                    if (autoQ && autoW)
                    {
                        AutoQW();
                    }
                    else if (autoQ)
                    {
                        AutoQ();
                    }
                    else if (autoW)
                    {
                        AutoW();
                    }
                    else
                    {
                        Combo();
                    }
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    // Reworked Auto Q and W
                    if (autoQ && autoW)
                    {
                        AutoQW();
                    }
                    else if (autoQ)
                    {
                        AutoQ();
                    }
                    else if (autoW)
                    {
                        AutoW();
                    }
                    else
                    {
                        Harass();
                    }
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    // Reworked Auto Q and W
                    if (autoQ && autoW)
                    {
                        AutoQW();
                    }
                    else if (autoQ)
                    {
                        AutoQ();
                    }
                    else if (autoW)
                    {
                        AutoW();
                    }
                    else
                    {
                        Orbwalking.DisableNextAttack = true;
                        LastHit();
                    }
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    // Reworked Auto Q and W
                    if (autoQ && autoW)
                    {
                        AutoQW();
                    }
                    else if (autoQ)
                    {
                        AutoQ();
                    }
                    else if (autoW)
                    {
                        AutoW();
                    }
                    else
                    {
                        LaneClear();
                    }
                    break;
                case Orbwalking.OrbwalkingMode.None:
                    //KillSteal
                    if (Config.SubMenu("KSMenu").Item("KSQ").GetValue<bool>())
                    {
                        KSQ();
                    }

                    //Flee Menu
                    if (Config.SubMenu("Flee").Item("fleetoggle").IsActive())
                    {
                        Flee();
                    }

                    //Auto Shroom
                    if (Config.SubMenu("Misc").Item("autoR").GetValue<bool>())
                    {
                        AutoShroom();
                    }

                    // Reworked Auto Q and W
                    if (autoQ && autoW)
                    {
                        AutoQ();
                        AutoW();
                    }
                    else if (autoQ)
                    {
                        AutoQ();
                    }
                    else if (autoW)
                    {
                        AutoW();
                    }
                    break;
            }
            // Debug
            //if(Config.SubMenu("Console").Item("Debug").IsActive())
            //{
            //    Console.WriteLine(Player.Position.X + "is the X position");
            //    Console.WriteLine(Player.Position.Y + "is the Y Position");
            //    Console.WriteLine(Player.Position.Z + "is the Z Position");
            //}
        }

        static void DrawingOnOnDraw(EventArgs args)
        {
            var drawQ = Config.SubMenu("Drawing").Item("drawQ").GetValue<bool>();
            var drawR = Config.SubMenu("Drawing").Item("drawR").GetValue<bool>();
            var drawautoR = Config.SubMenu("Drawing").Item("drawautoR").GetValue<bool>();

            var player = ObjectManager.Player.Position;

            // Reworked Drawing Colors

            if (drawQ)
            {
                Render.Circle.DrawCircle(player, Q.Range, Q.IsReady() ? System.Drawing.Color.LightGreen : System.Drawing.Color.Red);
            }
            if (drawR)
            {
                Render.Circle.DrawCircle(player, R.Range, R.IsReady() ? System.Drawing.Color.LightGreen : System.Drawing.Color.Red);
            }

            // Multi Map Support Drawing

            if (drawautoR && Utility.Map.GetMap().Type == Utility.Map.MapType.SummonersRift)
                foreach (var place in ShroomPositions.SummonersRift.Where(pos => pos.Distance(ObjectManager.Player.Position) <= Config.SubMenu("Drawing").Item("DrawVision").GetValue<Slider>().Value))
                {
                    Render.Circle.DrawCircle(place, 100, System.Drawing.Color.Red);
                }

            else if (drawautoR && Utility.Map.GetMap().Type == Utility.Map.MapType.CrystalScar)
            {
                //WIP
                foreach (var place in ShroomPositions.CrystalScar.Where(pos => pos.Distance(ObjectManager.Player.Position) <= Config.SubMenu("Drawing").Item("DrawVision").GetValue<Slider>().Value))
                {
                    Render.Circle.DrawCircle(place, 100, System.Drawing.Color.Red);
                }
            }

            else if (drawautoR && Utility.Map.GetMap().Type == Utility.Map.MapType.HowlingAbyss)
            {
                foreach (var place in ShroomPositions.HowlingAbyss.Where(pos => pos.Distance(ObjectManager.Player.Position) <= Config.SubMenu("Drawing").Item("DrawVision").GetValue<Slider>().Value))
                {
                    Render.Circle.DrawCircle(place, 100, System.Drawing.Color.Red);
                }
            }

            else if (drawautoR && Utility.Map.GetMap().Type == Utility.Map.MapType.TwistedTreeline)
            {
                // WIP
                foreach (var place in ShroomPositions.TwistedTreeline.Where(pos => pos.Distance(ObjectManager.Player.Position) <= Config.SubMenu("Drawing").Item("DrawVision").GetValue<Slider>().Value))
                {
                    Render.Circle.DrawCircle(place, 100, System.Drawing.Color.Red);
                }
            }

        }
    }
}