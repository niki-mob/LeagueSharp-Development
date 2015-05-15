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
        #region Initilization

        public const string ChampionName = "Teemo";

        //Spells
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static ShroomTables ShroomPositions;
        
        // Orbwalker
        public static Orbwalking.Orbwalker Orbwalker;

        // Menu
        public static Menu Config;

        // Player
        static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }

        // Packets
        public static bool Packets
        {
            get { return Config.SubMenu("Misc").Item("packets").GetValue<bool>(); }
        }

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.BaseSkinName != ChampionName)
            {
                Notifications.AddNotification(ChampionName + "is not supported", -1, false);
                Notifications.AddNotification("To use this assembly, please use Teemo only", -1, false);
                return;
            }

            // Spells
            Q = new Spell(SpellSlot.Q, 680);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R, 230);

            Q.SetTargetted(0f, 2000f);
            R.SetSkillshot(0.1f, 75f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            // Menu
            Config = new Menu("PandaTeemo", "PandaTeemo", true);
            Config.AddToMainMenu();

            // TargetSelector
            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            // OrbWalker SubMenu
            var orbwalking = Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            var combo = Config.AddSubMenu(new Menu("Combo", "Combo"));
            var harass = Config.AddSubMenu(new Menu("Harass", "Harass"));
            var laneclear = Config.AddSubMenu(new Menu("LaneClear", "LaneClear"));
            var ks = Config.AddSubMenu(new Menu("KSMenu", "KSMenu"));
            var flee = Config.AddSubMenu(new Menu("Flee Menu", "Flee"));
            var drawing = Config.AddSubMenu(new Menu("Drawing", "Drawing"));
            var interrupt = Config.AddSubMenu(new Menu("Interrupt", "Interrupt & Gapcloser"));
            var misc = Config.AddSubMenu(new Menu("Misc", "Misc"));
            //var console = Config.AddSubMenu(new Menu("Console", "Console"));

            // Main Menu
            Orbwalker = new Orbwalking.Orbwalker(orbwalking);

            // Combo Menu
            combo.AddItem(new MenuItem("qcombo", "Use Q in Combo").SetValue(true));
            combo.AddItem(new MenuItem("wcombo", "Use W in Combo").SetValue(true));
            combo.AddItem(new MenuItem("rcombo", "Kite with R in Combo").SetValue(true));
            combo.AddItem(new MenuItem("wCombat", "Use W if enemy is in range only").SetValue(false));
            combo.AddItem(new MenuItem("rCharge", "Charges of R before using R").SetValue(new Slider(2, 1, 3)));

            // Harass Menu
            harass.AddItem(new MenuItem("qharass", "Harass with Q").SetValue(true));

            // LaneClear Menu
            laneclear.AddItem(new MenuItem("qclear", "LaneClear with Q").SetValue(true));
            laneclear.AddItem(new MenuItem("rclear", "LaneClear with R").SetValue(true));
            laneclear.AddItem(new MenuItem("minionR", "Minion for R").SetValue(new Slider(3, 1, 4)));

            // Interrupter && Gapcloser
            interrupt.AddItem(new MenuItem("intq", "Interrupt with Q").SetValue(true));
            interrupt.AddItem(new MenuItem("intChance", "Danger Level before using Q").SetValue(new StringList(new[] { "High", "Medium", "Low" })));
            interrupt.AddItem(new MenuItem("gapR", "Gapclose with R").SetValue(true));

            // KS Menu
            ks.AddItem(new MenuItem("KSQ", "KillSteal with Q").SetValue(true));
            ks.AddItem(new MenuItem("KSR", "KillSteal with R").SetValue(true));
            ks.AddItem(new MenuItem("KSAA", "KillSteal with AutoAttack").SetValue(true));

            // Drawing Menu
            drawing.AddItem(new MenuItem("drawQ", "Draw Q Range").SetValue(true));
            drawing.AddItem(new MenuItem("drawR", "Draw R Range").SetValue(true));
            drawing.AddItem(new MenuItem("drawrClear", "Draw where to place R while LaneClear").SetValue(true));
            drawing.AddItem(new MenuItem("drawrclearRange", "Draw R LaneClear Range").SetValue(new Slider(1500, 2500, 1000)));
            drawing.AddItem(new MenuItem("colorBlind", "Colorblind Mode").SetValue(false));
            drawing.AddItem(new MenuItem("drawautoR", "Draw Important Shroom Areas").SetValue(true));
            drawing.AddItem(new MenuItem("DrawVision", "Shroom Vision").SetValue(new Slider(1500, 2500, 1000)));

            // Output to Console Location
            //console.AddItem(new MenuItem("Debug", "Debug").SetValue(new KeyBind(84, KeyBindType.Press)));

            // Flee Menu
            flee.AddItem(new MenuItem("fleetoggle", "Flee").SetValue(new KeyBind(90, KeyBindType.Press)));
            flee.AddItem(new MenuItem("w", "Use W while Flee").SetValue(true));
            flee.AddItem(new MenuItem("r", "Use R while Flee").SetValue(true));
            flee.AddItem(new MenuItem("rCharge", "Charges of R before using R").SetValue(new Slider(2, 1, 3)));

            // Misc
            misc.AddItem(new MenuItem("autoQ", "Automatic Q").SetValue(false));
            misc.AddItem(new MenuItem("autoW", "Automatic W").SetValue(false));
            misc.AddItem(new MenuItem("autoR", "Auto Place Shrooms in Important Places").SetValue(true));
            misc.AddItem(new MenuItem("autoRPanic", "Panic Key for Auto R").SetValue(new KeyBind(84, KeyBindType.Press)));
            misc.AddItem(new MenuItem("packets", "Use Packets").SetValue(false));

            // Events
            ShroomPositions = new ShroomTables();
            Game.OnUpdate += Game_OnGameUpdate;
            Interrupter2.OnInterruptableTarget += Interrupter_OnPossibleToInterrupt;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Drawing.OnDraw += DrawingOnOnDraw;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;

            // Notification (Replacement for PrintChat)
            Notifications.AddNotification("PandaTeemo Loaded", 10000, true);
            Notifications.AddNotification("Version 1.3.3.7 Revised", 10000, true);
        }

        #endregion

        #region Gapcloser

        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            var gapR = Config.SubMenu("Interrupt").Item("gapR").GetValue<bool>();

            if (gapcloser.Sender.IsValidTarget(R.Range) && gapcloser.Sender.IsFacing(Player))
            {
                Notifications.AddNotification("Gapclosing" + gapcloser.Sender.Name, 10000, true);
                R.Cast(gapcloser.Sender.ServerPosition, Packets);
            }
        }

        #endregion

        #region AfterAttack

        static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            // The following code is taken from Marksman
            var useQCombo = Config.SubMenu("Combo").Item("qcombo").GetValue<bool>();
            var useQHarass = Config.SubMenu("Harass").Item("qharass").GetValue<bool>();
            var t = target as Obj_AI_Hero;

            if (t != null && (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo|| Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed))
            {
                if (useQHarass && Q.IsReady() || useQCombo && Q.IsReady())
                {
                    Q.CastOnUnit(t, Packets);
                }
            }
        }

        #endregion

        #region IsShroom

        /// <summary>
        /// Checks if there is shroom in location
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        static bool IsShroomed(Vector3 position)
        {
            return ObjectManager.Get<Obj_AI_Base>().Where(obj => obj.Name == "Noxious Trap").Any(obj => position.Distance(obj.Position) <= 250);
        }

        #endregion

        #region Combo

        static void Combo()
        {
            var target = TargetSelector.GetTarget(500, TargetSelector.DamageType.Physical);
            var qtarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var rtarget = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
            var useQ = Config.SubMenu("Combo").Item("qcombo").GetValue<bool>();
            var useW = Config.SubMenu("Combo").Item("wcombo").GetValue<bool>();
            var useR = Config.SubMenu("Combo").Item("rcombo").GetValue<bool>();
            var wCombat = Config.SubMenu("Combo").Item("wCombat").GetValue<bool>();
            var rCount = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).Ammo;
            var rCharge = Config.SubMenu("Combo").Item("rCharge").GetValue<Slider>().Value;

            if (W.IsReady() && useW && !wCombat)
            {
                W.Cast();
            }

            if (useW && wCombat)
            {
                if (target.IsValidTarget() && W.IsReady())
                {
                    W.Cast();
                }
            }

            if (!target.IsValidTarget())
            {
                return;
            }

            if (Q.IsReady() && useQ && qtarget.IsValidTarget())
            {
                Q.Cast(qtarget, Packets);
            }

            if (R.IsReady() && useR && R.IsInRange(rtarget) && rCharge <= rCount && rtarget.IsValidTarget() && !IsShroomed(rtarget.Position))
            {
                R.Cast(rtarget.Position, Packets);
            }
        }

        #endregion

        #region KillSteal

        static void KS()
        {
            var aatarget = TargetSelector.GetTarget(500, TargetSelector.DamageType.Physical);
            var qtarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var rtarget = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
            var KSQ = Config.SubMenu("KSMenu").Item("KSQ").GetValue<bool>();
            var KSR = Config.SubMenu("KSMenu").Item("KSR").GetValue<bool>();
            var KSAA = Config.SubMenu("KSMenu").Item("KSAA").GetValue<bool>();

            double TeemoE = 0;
            TeemoE += Player.GetSpellDamage(aatarget, SpellSlot.E);

            if (aatarget.Health <= TeemoE && KSAA)
            {
                Player.IssueOrder(GameObjectOrder.AttackUnit, aatarget);
            }

            if(Q.IsReady() && qtarget.IsValidTarget() && KSQ)
            {
                if(qtarget.Health <= Q.GetDamage(qtarget) && Q.IsInRange(qtarget))
                {
                    Q.CastOnUnit(qtarget, Packets);
                }
            }

            if (R.IsReady() && rtarget.IsValidTarget() && KSR)
            {
                if (rtarget.Health <= R.GetDamage(rtarget) && !Q.IsReady() && R.IsInRange(rtarget) && KSQ)
                {
                    R.Cast(rtarget.Position, Packets);
                }
                else if (rtarget.Health <= R.GetDamage(rtarget) && R.IsInRange(rtarget) && !KSQ)
                {
                    R.Cast(rtarget.Position, Packets);
                }
            }

            return;

        }

        #endregion

        #region Harass

        static void Harass()
        {
            var qtarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var useQ = Config.SubMenu("Harass").Item("qharass").GetValue<bool>();

            // Harass Logic
            if (Q.IsReady() && qtarget.IsValidTarget() && useQ && Q.IsInRange(qtarget))
            {
                Q.Cast(qtarget, Packets);
            }

            // LastHit Logic
            double TeemoE = 0;

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit)
            {
                Orbwalking.DisableNextAttack = true;
                var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, 30, MinionTypes.All);
                foreach (var minion in allMinions)
                {
                    TeemoE += Player.GetSpellDamage(minion, SpellSlot.E);
                    if (minion.Health <= ObjectManager.Player.GetAutoAttackDamage(minion) + TeemoE)
                    {
                        Orbwalking.DisableNextAttack = false;
                        Player.IssueOrder(GameObjectOrder.AttackUnit, minion);
                        return;
                    }
                }
            }
            else
            {
                Orbwalking.DisableNextAttack = false;
            }

            return;

        }

        #endregion

        #region LaneClear

        static void LaneClear()
        {
            // Removed Outdated Algorithms
            var allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range);
            var allMinionsR = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, R.Range, MinionTypes.Melee);
            var rangedMinionsR = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, R.Range, MinionTypes.Ranged);
            var rLocation = R.GetCircularFarmLocation(allMinionsR, R.Range);
            var r2Location = R.GetCircularFarmLocation(rangedMinionsR, R.Range);
            var useQ = Config.SubMenu("LaneClear").Item("qclear").GetValue<bool>();
            var useR = Config.SubMenu("LaneClear").Item("rclear").GetValue<bool>();
            var minionR = Config.SubMenu("LaneClear").Item("minionR").GetValue<Slider>().Value;

            // Fix LaneClear Bug
            if (allMinionsQ.Count > 0 && useQ || minionR <= rLocation.MinionsHit && useR || minionR <= r2Location.MinionsHit && useR || minionR <= rLocation.MinionsHit + r2Location.MinionsHit && useR)
            {
                if (useQ)
                {
                    foreach (var minion in allMinionsQ)
                    {
                        if (minion.Health < ObjectManager.Player.GetSpellDamage(minion, SpellSlot.Q) && Q.IsReady())
                        {
                            Q.CastOnUnit(minion, Packets);
                            return;
                        }
                    }
                }

                if (useR)
                {
                    foreach (var minion in allMinionsR)
                    {
                        if (minion.Health <= ObjectManager.Player.GetSpellDamage(minion, SpellSlot.R) && R.IsReady() && R.IsInRange(rLocation.Position.To3D()) && !IsShroomed(rLocation.Position.To3D()) &&  minionR <= rLocation.MinionsHit)
                        {
                            R.Cast(rLocation.Position, Packets);
                            return;
                        }
                        else if (minion.Health <= ObjectManager.Player.GetSpellDamage(minion, SpellSlot.R) && R.IsReady() && R.IsInRange(r2Location.Position.To3D()) && !IsShroomed(r2Location.Position.To3D()) && minionR <= r2Location.MinionsHit)
                        {
                            R.Cast(r2Location.Position, Packets);
                            return;
                        }
                    }
                }
            }
        }

        #endregion

        #region Interrupt

        static void Interrupter_OnPossibleToInterrupt(Obj_AI_Hero sender,
            Interrupter2.InterruptableTargetEventArgs args)
        {
            // Fixed Interrupt

            var intq = Config.SubMenu("Interrupt").Item("intq").GetValue<bool>();
            var intChance = Config.SubMenu("Interrupt").Item("intChance").GetValue<StringList>().SelectedValue;

            // High Danger Level
            if (intChance.Contains("High") && intq && Q.IsReady() && args.DangerLevel == Interrupter2.DangerLevel.High)
            {
                Notifications.AddNotification("Interrupting" + sender, 10000, true);
                Q.Cast(sender, Packets);
            }

            // Medium Danger Level
            else if (intChance.Contains("Medium") && intq && Q.IsReady() && args.DangerLevel == Interrupter2.DangerLevel.Medium)
            {
                Notifications.AddNotification("Interrupting" + sender, 10000, true);
                Q.Cast(sender, Packets);
            }

            // Low Danger Level
            else if (intChance.Contains("Low") && intq && Q.IsReady() && args.DangerLevel == Interrupter2.DangerLevel.Low)
            {
                Notifications.AddNotification("Interrupting" + sender, 10000, true);
                Q.Cast(sender, Packets);
            }

            else
            {
                return;
            }
        }

        #endregion

        #region AutoShroom
        
        static void AutoShroom()
        {
            var autoRPanic = Config.SubMenu("Misc").Item("autoRPanic").IsActive();

            // Panic Key now makes you move
            if (autoRPanic)
            {
                Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            }

            if (!R.IsReady() || Player.HasBuff("Recall") || autoRPanic)
            {
                return;
            }

            // Zhonya / Recall Auto Shroom (Taken from Marksman)
            var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
            if (target.IsValidTarget(R.Range) && (target.HasBuff("Recall") || target.HasBuff("zhonyasringshield")) && R.IsReady() && R.IsInRange(target))
            {
                R.Cast(target.Position, Packets);
            }

            // Multi Map Support Shrooming

            if (Utility.Map.GetMap().Type == Utility.Map.MapType.SummonersRift)
            {
                foreach (var place in ShroomPositions.SummonersRift.Where(pos => pos.Distance(ObjectManager.Player.Position) <= R.Range && !IsShroomed(pos)))
                    R.Cast(place, Packets);
            }
            else if (Utility.Map.GetMap().Type == Utility.Map.MapType.HowlingAbyss)
            {
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

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit)
            {
                Orbwalking.DisableNextAttack = true;
                var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, 30, MinionTypes.All);
                foreach (var minion in allMinions)
                {
                    TeemoE += Player.GetSpellDamage(minion, SpellSlot.E);
                    if (minion.Health < ObjectManager.Player.GetAutoAttackDamage(minion) + TeemoE)
                    {
                        Orbwalking.DisableNextAttack = false;
                        Player.IssueOrder(GameObjectOrder.AttackUnit, minion);
                        return;
                    }
                }
            }
            else
            {
                Orbwalking.DisableNextAttack = false;
            }

            return;
        }

        #endregion

        #region Flee

        static void Flee()
        {
            // Checks if toggle is on
            var useW = Config.SubMenu("Flee").Item("w").GetValue<bool>();
            var useR = Config.SubMenu("Flee").Item("r").GetValue<bool>();
            var rCharge = Config.SubMenu("Flee").Item("rCharge").GetValue<Slider>().Value;

            // Force move to player's mouse cursor
            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);

            // Uses W if avaliable and if toggle is on
            if (useW && W.IsReady())
            {
                W.Cast(Player);
            }

            // Uses R if avaliable and if toggle is on
            if (useR && R.IsReady() && rCharge <= ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).Ammo)
            {
                R.Cast(Player.Position, Packets);
            }
        }

        #endregion

        #region Auto Q

        static void AutoQ()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All);

            if (!Q.IsReady())
            {
                return;
            }

            if (Q.IsReady() && 1 <= allMinionsQ.Count)
            {
                foreach (var minion in allMinionsQ)
                {
                    if (minion.Health <= Q.GetDamage(minion) && Q.IsInRange(minion))
                    {
                        Q.CastOnUnit(minion, Packets);
                    }
                }
            }

            else if (Q.IsReady() && Q.IsInRange(target) && target.IsValidTarget())
            {
                Q.Cast(target, Packets);
            }

            return;
        }

        #endregion

        #region Auto W

        static void AutoW()
        {
            if (!W.IsReady())
            { 
                return; 
            }

            if (W.IsReady())
            {
                W.Cast(Player);
            }
            return;
        }

        #endregion

        #region Auto Q & W

        static void AutoQW()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All);

            if (!W.IsReady() || !Q.IsReady())
            {
                return;
            }

            if (W.IsReady())
            {
                W.Cast();
            }

            if (Q.IsReady() && 1 <= allMinionsQ.Count)
            {
                foreach (var minion in allMinionsQ)
                {
                    if (minion.Health <= Q.GetDamage(minion) && Q.IsInRange(minion))
                    {
                        Q.CastOnUnit(minion, Packets);
                    }
                }
            }
            else if (Q.IsReady() && Q.IsInRange(target) && target.IsValidTarget() && 25 <= Player.ManaPercent)
            {
                Q.Cast(target);
            }

            return;
        }

        #endregion

        #region Game_OnUpdate

        static void Game_OnGameUpdate(EventArgs args)
        {
            var autoQ = Config.Item("autoQ").GetValue<bool>();
            var autoW = Config.Item("autoW").GetValue<bool>();

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

            // Reworked Orbwalker
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Orbwalking.DisableNextAttack = false;
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Orbwalking.DisableNextAttack = false;
                    Harass();
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    Orbwalking.DisableNextAttack = false;
                    LastHit();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    Orbwalking.DisableNextAttack = false;
                    LaneClear();
                    break;
                case Orbwalking.OrbwalkingMode.None:
                    Orbwalking.DisableNextAttack = false;

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

                    //KillSteal
                    if (Config.SubMenu("KSMenu").Item("KSQ").GetValue<bool>() || Config.SubMenu("KSMenu").Item("KSR").GetValue<bool>())
                    {
                        KS();
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

        #endregion

        #region Drawing

        static void DrawingOnOnDraw(EventArgs args)
        {
            var drawQ = Config.SubMenu("Drawing").Item("drawQ").GetValue<bool>();
            var drawR = Config.SubMenu("Drawing").Item("drawR").GetValue<bool>();
            var drawautoR = Config.SubMenu("Drawing").Item("drawautoR").GetValue<bool>();
            var drawrClear = Config.SubMenu("Drawing").Item("drawrClear").GetValue<bool>();
            var colorBlind = Config.SubMenu("Drawing").Item("colorBlind").GetValue<bool>();
            var player = ObjectManager.Player.Position;

            // Reworked Drawing Colors && Added ColorBlind Mode
            if (drawQ && colorBlind)
            {
                Render.Circle.DrawCircle(player, Q.Range, Q.IsReady() ? System.Drawing.Color.YellowGreen : System.Drawing.Color.Red);
            }
            else if (drawQ)
            {
                Render.Circle.DrawCircle(player, Q.Range, Q.IsReady() ? System.Drawing.Color.LightGreen : System.Drawing.Color.Red);
            }

            if (drawR && colorBlind)
            {
                Render.Circle.DrawCircle(player, R.Range, R.IsReady() ? System.Drawing.Color.YellowGreen : System.Drawing.Color.Red);
            }
            else if (drawR)
            {
                Render.Circle.DrawCircle(player, R.Range, R.IsReady() ? System.Drawing.Color.LightGreen : System.Drawing.Color.Red);
            }

            if (drawrClear)
            {
                // LaneClear R Location Drawing
                var drawrclearRange = Config.SubMenu("Drawing").Item("drawrclearRange").GetValue<Slider>().Value;
                var rLocation = R.GetCircularFarmLocation(MinionManager.GetMinions(ObjectManager.Player.ServerPosition, drawrclearRange, MinionTypes.Melee));
                var r2Location = R.GetCircularFarmLocation(MinionManager.GetMinions(ObjectManager.Player.ServerPosition, drawrclearRange, MinionTypes.Ranged));

                if (colorBlind)
                {
                    Render.Circle.DrawCircle(rLocation.Position.To3D(), 100, System.Drawing.Color.YellowGreen);
                    Render.Circle.DrawCircle(r2Location.Position.To3D(), 100, System.Drawing.Color.YellowGreen);
                }
                else
                {
                    Render.Circle.DrawCircle(rLocation.Position.To3D(), 100, System.Drawing.Color.Green);
                    Render.Circle.DrawCircle(r2Location.Position.To3D(), 100, System.Drawing.Color.Green);
                }
            }

            // Multi Map Support Drawing

            if (drawautoR && Utility.Map.GetMap().Type == Utility.Map.MapType.SummonersRift)
            {
                foreach (var place in ShroomPositions.SummonersRift.Where(pos => pos.Distance(ObjectManager.Player.Position) <= Config.SubMenu("Drawing").Item("DrawVision").GetValue<Slider>().Value))
                {
                    if(IsShroomed(place))
                    {
                        Render.Circle.DrawCircle(place, 100, System.Drawing.Color.Red);
                    }
                    else if (!IsShroomed(place) && colorBlind)
                    {
                        Render.Circle.DrawCircle(place, 100, System.Drawing.Color.YellowGreen);
                    }
                    else if (!IsShroomed(place) && !colorBlind)
                    {
                        Render.Circle.DrawCircle(place, 100, System.Drawing.Color.Green);
                    }
                }
            }

            else if (drawautoR && Utility.Map.GetMap().Type == Utility.Map.MapType.CrystalScar)
            {
                //WIP
                foreach (var place in ShroomPositions.CrystalScar.Where(pos => pos.Distance(ObjectManager.Player.Position) <= Config.SubMenu("Drawing").Item("DrawVision").GetValue<Slider>().Value))
                {
                    if (IsShroomed(place))
                    {
                        Render.Circle.DrawCircle(place, 100, System.Drawing.Color.Red);
                    }
                    else if (!IsShroomed(place) && colorBlind)
                    {
                        Render.Circle.DrawCircle(place, 100, System.Drawing.Color.YellowGreen);
                    }
                    else if (!IsShroomed(place) && !colorBlind)
                    {
                        Render.Circle.DrawCircle(place, 100, System.Drawing.Color.Green);
                    }
                }
            }

            else if (drawautoR && Utility.Map.GetMap().Type == Utility.Map.MapType.HowlingAbyss)
            {
                foreach (var place in ShroomPositions.HowlingAbyss.Where(pos => pos.Distance(ObjectManager.Player.Position) <= Config.SubMenu("Drawing").Item("DrawVision").GetValue<Slider>().Value))
                {
                    if (IsShroomed(place))
                    {
                        Render.Circle.DrawCircle(place, 100, System.Drawing.Color.Red);
                    }
                    else if (!IsShroomed(place) && colorBlind)
                    {
                        Render.Circle.DrawCircle(place, 100, System.Drawing.Color.YellowGreen);
                    }
                    else if (!IsShroomed(place) && !colorBlind)
                    {
                        Render.Circle.DrawCircle(place, 100, System.Drawing.Color.Green);
                    }
                }
            }

            else if (drawautoR && Utility.Map.GetMap().Type == Utility.Map.MapType.TwistedTreeline)
            {
                // WIP
                foreach (var place in ShroomPositions.TwistedTreeline.Where(pos => pos.Distance(ObjectManager.Player.Position) <= Config.SubMenu("Drawing").Item("DrawVision").GetValue<Slider>().Value))
                {
                    if (IsShroomed(place))
                    {
                        Render.Circle.DrawCircle(place, 100, System.Drawing.Color.Red);
                    }
                    else if (!IsShroomed(place) && colorBlind)
                    {
                        Render.Circle.DrawCircle(place, 100, System.Drawing.Color.YellowGreen);
                    }
                    else if (!IsShroomed(place) && !colorBlind)
                    {
                        Render.Circle.DrawCircle(place, 100, System.Drawing.Color.Green);
                    }
                }
            }

        #endregion
        }
    }
}