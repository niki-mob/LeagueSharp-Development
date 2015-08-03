﻿﻿using LeagueSharp;
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
        #region Initialization

        /// <summary>
        /// Teemo's Name
        /// </summary>
        public const string ChampionName = "Teemo";

        /// <summary>
        /// Array of ADC Names
        /// </summary>
        static string[] Marksman = { "Ashe", "Caitlyn", "Corki", "Draven", "Ezreal", "Jinx", "Kalista", "KogMaw", "Lucian", "MissFortune", "Quinn", "Sivir", "Teemo", "Tristana", "Twitch", "Urgot", "Varus", "Vayne" };

        /// <summary>
        /// Q
        /// </summary>
        public static Spell Q;

        /// <summary>
        /// W
        /// </summary>
        public static Spell W;

        /// <summary>
        /// E
        /// </summary>
        public static Spell E;

        /// <summary>
        /// R
        /// </summary>
        public static Spell R;

        public static ShroomTables ShroomPositions;
        public static FileHandler _FileHandler;
        
        /// <summary>
        /// Orbwalker
        /// </summary>
        public static Orbwalking.Orbwalker Orbwalker;

        /// <summary>
        /// Menu
        /// </summary>
        public static Menu Config;

        /// <summary>
        /// Player
        /// </summary>
        static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }

        /// <summary>
        /// Packet Boolean
        /// </summary>
        public static bool Packets
        {
            get { return Config.SubMenu("Misc").Item("packets").GetValue<bool>(); }
        }

        /// <summary>
        /// Called when program starts
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Hacks.PingHack = true;
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        /// <summary>
        /// Loads when Game Starts
        /// </summary>
        /// <param name="args"></param>
        static void Game_OnGameLoad(EventArgs args)
        {
            // Checks if Player is Teemo
            if (Player.CharData.BaseSkinName != ChampionName)
            {
                return;
            }

            #region Menu

            // Spells
            Q = new Spell(SpellSlot.Q, 680);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R, 230);

            Q.SetTargetted(0f, 2000f);
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
            var jungleclear = Config.AddSubMenu(new Menu("JungleClear", "JungleClear"));
            var ks = Config.AddSubMenu(new Menu("KillSteal", "KSMenu"));
            var flee = Config.AddSubMenu(new Menu("Flee Menu", "Flee"));
            var drawing = Config.AddSubMenu(new Menu("Drawing", "Drawing"));
            var interrupt = Config.AddSubMenu(new Menu("Interrupt", "Interrupt & Gapcloser"));
            var misc = Config.AddSubMenu(new Menu("Misc", "Misc"));
            var hacks = Config.AddSubMenu(new Menu("Hack Menu", "Hacks"));

            // Main Menu
            Orbwalker = new Orbwalking.Orbwalker(orbwalking);

            // Combo Menu
            combo.AddItem(new MenuItem("qcombo", "Use Q in Combo").SetValue(true));
            combo.AddItem(new MenuItem("wcombo", "Use W in Combo").SetValue(true));
            combo.AddItem(new MenuItem("rcombo", "Kite with R in Combo").SetValue(true));
            combo.AddItem(new MenuItem("checkAA", "Check for AA Range before using Q").SetValue(true));
            combo.AddItem(new MenuItem("useqADC", "Use Q only on ADC during Combo").SetValue(false));
            combo.AddItem(new MenuItem("wCombat", "Use W if enemy is in range only").SetValue(false));
            combo.AddItem(new MenuItem("rCharge", "Charges of R before using R").SetValue(new Slider(2, 1, 3)));

            // Harass Menu
            harass.AddItem(new MenuItem("qharass", "Harass with Q").SetValue(true));

            // LaneClear Menu
            laneclear.AddItem(new MenuItem("qclear", "LaneClear with Q").SetValue(true));
            laneclear.AddItem(new MenuItem("qManaManager", "Q Mana Manager").SetValue(new Slider(75, 0, 100)));
            laneclear.AddItem(new MenuItem("attackTurret", "Attack Turret").SetValue(true));
            laneclear.AddItem(new MenuItem("attackWard", "Attack Ward").SetValue(true));
            laneclear.AddItem(new MenuItem("rclear", "LaneClear with R").SetValue(true));
            laneclear.AddItem(new MenuItem("userKill", "Use R only if Killable").SetValue(false));
            laneclear.AddItem(new MenuItem("minionR", "Minion for R").SetValue(new Slider(3, 1, 4)));

            // JungleClear Menu
            jungleclear.AddItem(new MenuItem("qclear", "JungleClear with Q").SetValue(true));
            jungleclear.AddItem(new MenuItem("rclear", "JungleClear with R").SetValue(true));

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
            drawing.AddItem(new MenuItem("colorBlind", "Colorblind Mode").SetValue(false));
            drawing.AddItem(new MenuItem("drawautoR", "Draw Important Shroom Areas").SetValue(true));
            drawing.AddItem(new MenuItem("DrawVision", "Shroom Vision").SetValue(new Slider(1500, 2500, 1000)));

            var debug = drawing.AddSubMenu(new Menu("Debug", "debug"));
            debug.AddItem(new MenuItem("debugdraw", "Draw Coords").SetValue(false));
            debug.AddItem(new MenuItem("x", "Where to draw X").SetValue(new Slider(500, 0, 1920)));
            debug.AddItem(new MenuItem("y", "Where to draw Y").SetValue(new Slider(500, 0, 1080)));
            debug.AddItem(new MenuItem("debugpos", "Draw Custom Shroom Locations Coordinates").SetValue(true));

            // Flee Menu
            flee.AddItem(new MenuItem("fleetoggle", "Flee").SetValue(new KeyBind(65, KeyBindType.Press)));
            flee.AddItem(new MenuItem("w", "Use W while Flee").SetValue(true));
            flee.AddItem(new MenuItem("r", "Use R while Flee").SetValue(true));
            flee.AddItem(new MenuItem("rCharge", "Charges of R before using R").SetValue(new Slider(2, 1, 3)));

            // Misc
            misc.AddItem(new MenuItem("autoQ", "Automatic Q").SetValue(false));
            misc.AddItem(new MenuItem("autoW", "Automatic W").SetValue(false));
            misc.AddItem(new MenuItem("autoR", "Auto Place Shrooms in Important Places").SetValue(true));
            misc.AddItem(new MenuItem("autoRPanic", "Panic Key for Auto R").SetValue(new KeyBind(84, KeyBindType.Press)));
            misc.AddItem(new MenuItem("customLocation", "Use Custom Location for Auto Shroom (Requires Reload)").SetValue(true));
            misc.AddItem(new MenuItem("customLocationInt", "Set the amount of locations you have").SetValue(new Slider(1, 1, 25)));
            misc.AddItem(new MenuItem("packets", "Use Packets").SetValue(false));

            hacks.AddItem(new MenuItem("zoomHack", "Zoom Hack Enabler").SetValue(false));

            Config.AddToMainMenu();

            #endregion

            // Events
            Game.OnUpdate += Game_OnUpdate;
            Interrupter2.OnInterruptableTarget += Interrupter_OnPossibleToInterrupt;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;
            Drawing.OnDraw += Drawing_OnDraw;

            // GG PrintChat Bik™
            Game.PrintChat("<font color='#FBF5EF'>Game.PrintChat Bik</font> - <font color = '#01DF3A'>PandaTeemo v1.7.0.1 Loaded</font>");
            Notifications.AddNotification("PandaTeemo Loaded", 10000, true);
            Notifications.AddNotification("Version 1.7.0.1", 10000, true);

            // Loads ShroomPosition
            _FileHandler = new FileHandler();
            ShroomPositions = new ShroomTables();
        }

        #endregion

        #region BeforeAttack

        /// <summary>
        /// Actions before attacking
        /// </summary>
        /// <param name="args">Attack Action</param>
        static void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            double TeemoE = 0;

            #region LastHit

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit)
            {
                args.Process = false;
                foreach (var minion in MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Player.AttackRange, MinionTypes.All))
                {
                    TeemoE += Player.GetSpellDamage(minion, SpellSlot.E);
                    if (minion.Health < ObjectManager.Player.GetAutoAttackDamage(minion) + TeemoE && Orbwalking.InAutoAttackRange(minion))
                    {
                        Player.IssueOrder(GameObjectOrder.AttackUnit, minion);
                        args.Process = true;
                        return;
                    }
                    else
                    {
                        args.Process = false;
                        return;
                    }
                }
            }

            #endregion

            #region Harass

            else if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                args.Process = false;
                var minion = MinionManager.GetMinions(ObjectManager.Player.Position, Player.AttackRange, MinionTypes.All).Where(t => t.IsEnemy).OrderBy(t => t.Health).FirstOrDefault();
                TeemoE += Player.GetSpellDamage(minion, SpellSlot.E);

                if (minion.Health <= ObjectManager.Player.GetAutoAttackDamage(minion) + TeemoE)
                {
                    Player.IssueOrder(GameObjectOrder.AttackUnit, minion);
                    args.Process = true;
                    return;
                }

                else
                {
                    args.Process = false;
                    return;
                }
            }

            #endregion

            #region LaneClear

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                args.Process = false;
                foreach (var minion in MinionManager.GetMinions(Player.AttackRange, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.Health))
                {
                    #region Variables

                    TeemoE += Player.GetSpellDamage(minion, SpellSlot.E);
                    var qManaManager = Config.SubMenu("LaneClear").Item("qManaManager").GetValue<Slider>().Value;

                    #endregion

                    #region Cannot Kill Minion or In Range of Turret/Ward/Jungle Creep

                    if (Player.GetAutoAttackDamage(minion) + TeemoE < minion.Health || Q.GetDamage(minion) < minion.Health)
                    {
                        Player.IssueOrder(GameObjectOrder.AttackUnit, minion);
                        args.Process = true;
                    }

                    #endregion

                    #region Can Kill Minion

                    else if (minion.Health <= Player.GetAutoAttackDamage(minion) + TeemoE || minion.Health <= Q.GetDamage(minion))
                    {
                        args.Process = false;
                        var useQ = Config.SubMenu("LaneClear").Item("qclear").GetValue<bool>();
                        if (useQ)
                        {
                            if (Q.IsReady() && Q.IsInRange(minion) && Q.GetDamage(minion) >= minion.Health && qManaManager <= (int)Player.ManaPercent)
                            {
                                Q.CastOnUnit(minion, Packets);
                                args.Process = true;
                            }

                            else if (Player.Distance3D(minion) <= Player.AttackRange)
                            {
                                Player.IssueOrder(GameObjectOrder.AttackUnit, minion);
                                args.Process = true;
                                return;
                            }
                        }
                        else
                        {
                            Player.IssueOrder(GameObjectOrder.AttackUnit, minion);
                            args.Process = true;
                            return;
                        }
                    }

                    #endregion

                    #region No Minions

                    else
                    {
                        args.Process = true;
                        return;
                    }

                    #endregion
                }
            }

            #endregion

            #region Other Modes

            else
            {
                args.Process = true;
            }

            #endregion
        }

        #endregion

        #region Gapcloser

        /// <summary>
        /// Gapcloser
        /// </summary>
        /// <param name="gapcloser"></param>
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

        /// <summary>
        /// Action after Attack (Taken from Marksman)
        /// </summary>
        /// <param name="unit">Unit Attacked</param>
        /// <param name="target">Target Attacked</param>
        static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            var useQCombo = Config.SubMenu("Combo").Item("qcombo").GetValue<bool>();
            var useQHarass = Config.SubMenu("Harass").Item("qharass").GetValue<bool>();
            var useqADC = Config.SubMenu("Combo").Item("useqADC").GetValue<bool>();
            var checkAA = Config.SubMenu("Combo").Item("checkAA").GetValue<bool>();
            var t = target as Obj_AI_Hero;

            if (t != null && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                if (checkAA)
                {
                    if (useqADC)
                    {
                        foreach (var adc in Marksman)
                        {
                            if (useQCombo && Q.IsReady() && Q.IsInRange(t, -180) && t.CharData.BaseSkinName == adc)
                            {
                                Q.CastIfWillHit(t, 1, Packets);
                            }
                            else
                            {
                                return;
                            }
                        }
                    }

                    else if (useQCombo && Q.IsReady() && Q.IsInRange(t, -180))
                    {
                        Q.CastIfWillHit(t, 1, Packets);
                    }

                    else
                    {
                        return;
                    }
                }
                else
                {
                    if (useqADC)
                    {
                        foreach (var adc in Marksman)
                        {
                            if (useQCombo && Q.IsReady() && Q.IsInRange(t) && t.CharData.BaseSkinName == adc)
                            {
                                Q.CastIfWillHit(t, 1, Packets);
                            }
                            else
                            {
                                return;
                            }
                        }
                    }

                    else if (useQCombo && Q.IsReady() && Q.IsInRange(t))
                    {
                        Q.CastIfWillHit(t, 1, Packets);
                    }
                    
                    else
                    {
                        return;
                    }
                }
            }

            if (t != null && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                if (useQHarass && Q.IsReady() && Q.IsInRange(t))
                {
                    Q.CastIfWillHit(t, 1, Packets);
                }
            }
        }

        #endregion

        #region IsShroom

        /// <summary>
        /// Checks if there is shroom in location
        /// </summary>
        /// <param name="position">The location of check</param>
        /// <returns>If location is shroomed or not</returns>
        static bool IsShroomed(Vector3 position)
        {
            return ObjectManager.Get<Obj_AI_Base>().Where(obj => obj.Name == "Noxious Trap").Any(obj => position.Distance(obj.Position) <= 250);
        }

        #endregion

        #region Combo

        /// <summary>
        /// Combo
        /// </summary>
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

            /*if (Q.IsReady() && useQ && qtarget.IsValidTarget())
            {
                Q.Cast(qtarget, Packets);
            }*/

            if (R.IsReady() && useR && R.IsInRange(rtarget) && rCharge <= rCount && rtarget.IsValidTarget() && !IsShroomed(rtarget.Position))
            {
                R.CastIfHitchanceEquals(rtarget, HitChance.VeryHigh, Packets);
            }
        }

        #endregion

        #region KillSteal

        /// <summary>
        /// KillSteal
        /// </summary>
        static void KS()
        {
            var KSQ = Config.SubMenu("KSMenu").Item("KSQ").GetValue<bool>();
            var KSR = Config.SubMenu("KSMenu").Item("KSR").GetValue<bool>();
            var KSAA = Config.SubMenu("KSMenu").Item("KSAA").GetValue<bool>();

            #region KillSteal with AA / Q if AA not fast enough.

            if (KSAA)
            {
                var aatarget = HeroManager.Enemies.Where(t => Player.Distance3D(t) <= Player.AttackRange && Player.GetAutoAttackDamage(t) + Player.GetSpellDamage(t, SpellSlot.E) > t.Health).OrderBy(t => t.Distance3D(Player)).FirstOrDefault();

                if (aatarget != null)
                {
                    Player.IssueOrder(GameObjectOrder.AttackUnit, aatarget);
                }
            }

            #endregion

            #region KillSteal with Q

            if (KSQ)
            {
                var target = HeroManager.Enemies.Where(t => t.Distance3D(Player) < Q.Range && Q.GetDamage(t) > t.Health).OrderBy(t => t.Distance3D(Player)).FirstOrDefault();

                if (target != null && Q.IsReady())
                {
                    Q.Cast(target);
                }
            }

            #endregion

            #region KillSteal with R

            if (KSR)
            {
                var target = HeroManager.Enemies.Where(t => t.Distance3D(Player) < R.Range && R.GetDamage(t) > t.Health).OrderBy(t => t.Distance3D(Player)).FirstOrDefault();

                if (target != null && R.IsReady())
                {
                    R.CastIfHitchanceEquals(target, HitChance.VeryHigh, Packets);
                }
            }

            #endregion

            #region Old Logic

            /*
            var aatarget = TargetSelector.GetTarget(500, TargetSelector.DamageType.Physical);
            var qtarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var rtarget = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
            var KSQ = Config.SubMenu("KSMenu").Item("KSQ").GetValue<bool>();
            var KSR = Config.SubMenu("KSMenu").Item("KSR").GetValue<bool>();
            var KSAA = Config.SubMenu("KSMenu").Item("KSAA").GetValue<bool>();

            // To-Do: Rework Logic
            if (KSQ || KSAA)
            {
                if (!aatarget.IsValid && !qtarget.IsValid)
                {
                    return;
                }
                else
                {
                    double TeemoE = 0;
                    TeemoE += Player.GetSpellDamage(aatarget, SpellSlot.E);

                    if (aatarget.Health <= Player.GetAutoAttackDamage(aatarget) + TeemoE)
                    {
                        if (Q.IsReady() &&
                            Q.Delay < Player.AttackCastDelay &&
                            KSQ && qtarget.IsValidTarget() && qtarget.Health <= Q.GetDamage(qtarget) &&
                            Q.IsInRange(qtarget))
                        {
                            Q.CastOnUnit(qtarget, Packets);
                        }
                        else
                        {
                            Player.IssueOrder(GameObjectOrder.AttackUnit, aatarget);
                        }
                    }
                    else if (qtarget.Health <= Q.GetDamage(qtarget))
                    {
                        if (Q.IsReady() &&
                            Q.Delay < Player.AttackCastDelay &&
                            KSQ && qtarget.IsValidTarget() && qtarget.Health <= Q.GetDamage(qtarget) &&
                            Q.IsInRange(qtarget))
                        {
                            Q.CastOnUnit(qtarget, Packets);
                        }
                        else
                        {
                            return;
                        }
                    }
                    else
                    {
                        return;
                    }
                }
            }

            if (KSR)
            {
                if (R.IsReady() && rtarget.IsValidTarget())
                {
                    if (rtarget.Health <= R.GetDamage(rtarget) &&
                        !Q.IsReady() && R.IsInRange(rtarget) &&
                        KSQ)
                    {
                        R.CastIfHitchanceEquals(rtarget, HitChance.VeryHigh, Packets);
                    }
                    else if (rtarget.Health <= R.GetDamage(rtarget) &&
                        R.IsInRange(rtarget) &&
                        !KSQ)
                    {
                        R.CastIfHitchanceEquals(rtarget, HitChance.VeryHigh, Packets);
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
            */

            #endregion
        }

        #endregion

        #region Harass

        static void Harass()
        {
            var enemy = HeroManager.Enemies.Where(t => t.IsValidTarget() && Orbwalker.InAutoAttackRange(t) && t.IsEnemy).OrderBy(t => t.Health).FirstOrDefault();
            if (Orbwalker.InAutoAttackRange(enemy))
            {
                Player.IssueOrder(GameObjectOrder.AttackUnit, enemy);
            }
        }

        #endregion

        #region LaneClear

        /// <summary>
        /// LaneClear
        /// </summary>
        static void LaneClear()
        {
            var allMinionsR = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, R.Range, MinionTypes.Melee);
            var rangedMinionsR = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, R.Range, MinionTypes.Ranged);
            var rLocation = R.GetCircularFarmLocation(allMinionsR, R.Range);
            var r2Location = R.GetCircularFarmLocation(rangedMinionsR, R.Range);
            var useQ = Config.SubMenu("LaneClear").Item("qclear").GetValue<bool>();
            var useR = Config.SubMenu("LaneClear").Item("rclear").GetValue<bool>();
            var userKill = Config.SubMenu("LaneClear").Item("userKill").GetValue<bool>();
            var minionR = Config.SubMenu("LaneClear").Item("minionR").GetValue<Slider>().Value;

            var attackTurret = Config.SubMenu("LaneClear").Item("attackTurret").GetValue<bool>();
            var attackWard = Config.SubMenu("LaneClear").Item("attackWard").GetValue<bool>();
            var turret = ObjectManager.Get<Obj_AI_Turret>().Where(t => t.IsValidTarget() && Orbwalking.InAutoAttackRange(t) && t.IsEnemy).OrderBy(t => t.Health).FirstOrDefault();
            var inhib = ObjectManager.Get<Obj_BarracksDampener>().Where(t => t.IsValidTarget() && Orbwalking.InAutoAttackRange(t) && t.IsEnemy).OrderBy(t => t.Health).FirstOrDefault();
            var nexus = ObjectManager.Get<Obj_HQ>().Where(t => t.IsValidTarget() && Orbwalking.InAutoAttackRange(t) && t.IsEnemy).OrderBy(t => t.Health).FirstOrDefault();
            var ward = MinionManager.GetMinions(Player.AttackRange, MinionTypes.Wards, MinionTeam.NotAlly, MinionOrderTypes.Health).FirstOrDefault();

            if (Orbwalker.InAutoAttackRange(turret) && attackTurret
                || Orbwalker.InAutoAttackRange(inhib) && attackTurret
                || Orbwalker.InAutoAttackRange(nexus) && attackTurret
                || Orbwalker.InAutoAttackRange(ward) && attackWard)
            {
                #region Turret
                
                if (attackTurret)
                {
                    if (turret != null)
                    {
                        Player.IssueOrder(GameObjectOrder.AttackUnit, turret);
                    }
                    else if (inhib != null)
                    {
                        Player.IssueOrder(GameObjectOrder.AttackUnit, inhib);
                    }
                    else if (nexus != null)
                    {
                        Player.IssueOrder(GameObjectOrder.AttackUnit, nexus);
                    }
                }

                #endregion

                #region Ward

                if (attackWard)
                {
                    if (ward.IsValid && Orbwalker.InAutoAttackRange(ward) && ward.IsEnemy)
                    {
                        Player.IssueOrder(GameObjectOrder.AttackUnit, ward);
                    }
                }

                #endregion
            }

            if (minionR <= rLocation.MinionsHit && useR || minionR <= r2Location.MinionsHit && useR || minionR <= rLocation.MinionsHit + r2Location.MinionsHit && useR)
            {
                if (useR && userKill)
                {
                    foreach (var minion in allMinionsR)
                    {
                        if (minion.Health <= ObjectManager.Player.GetSpellDamage(minion, SpellSlot.R) && R.IsReady() && R.IsInRange(rLocation.Position.To3D()) && !IsShroomed(rLocation.Position.To3D()) && minionR <= rLocation.MinionsHit)
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

                else if (useR)
                {
                    if (R.IsReady() && R.IsInRange(rLocation.Position.To3D()) && !IsShroomed(rLocation.Position.To3D()) && minionR <= rLocation.MinionsHit)
                    {
                        R.Cast(rLocation.Position, Packets);
                        return;
                    }
                    else if (R.IsReady() && R.IsInRange(r2Location.Position.To3D()) && !IsShroomed(r2Location.Position.To3D()) && minionR <= r2Location.MinionsHit)
                    {
                        R.Cast(r2Location.Position, Packets);
                        return;
                    }
                }
            }
            else
            {
                JungleClear();
            }
        }

        #endregion

        #region JungleClear

        /// <summary>
        /// JungleClear
        /// </summary>
        static void JungleClear()
        {
            var useQ = Config.SubMenu("JungleClear").Item("qclear").GetValue<bool>();
            var useR = Config.SubMenu("JungleClear").Item("rclear").GetValue<bool>();
            var ammoR = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).Ammo;
            var qManaManager = Config.SubMenu("LaneClear").Item("qManaManager").GetValue<Slider>().Value;

            if (useQ && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                var junglemobQ = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
                foreach(var jungleMob in junglemobQ)
                {
                    if (Q.IsReady() && Q.IsInRange(jungleMob) && qManaManager <= (int)Player.ManaPercent)
                    {
                        Q.CastOnUnit(jungleMob, Packets);
                    }
                }
            }


            if (useR && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                var junglemobR = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
                foreach (var jungleMob in junglemobR)
                {
                    if (R.IsReady() && R.IsInRange(jungleMob) && ammoR >= 2)
                    {
                        R.Cast(jungleMob, Packets);
                    }
                }
            }

            else
            {
                return;
            }
        }

        #endregion

        #region Interrupt

        /// <summary>
        /// Interrupter
        /// </summary>
        /// <param name="sender">The Target</param>
        /// <param name="args">Action</param>
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
        
        /// <summary>
        /// AutoShroom
        /// </summary>
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
            else if (Utility.Map.GetMap().Type.ToString() == "Unknown")
            {
                foreach (var place in ShroomPositions.ButcherBridge.Where(pos => pos.Distance(ObjectManager.Player.Position) <= R.Range && !IsShroomed(pos)))
                    R.Cast(place, Packets);
            }
            else
            {
                return;
            }

        }

        #endregion

        #region Flee

        /// <summary>
        /// Flee
        /// </summary>
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

        /// <summary>
        /// Auto Q
        /// </summary>
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

        /// <summary>
        /// Auto W
        /// </summary>
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

        /// <summary>
        /// Auto Q and W
        /// </summary>
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

        /// <summary>
        /// OnUpdate
        /// </summary>
        /// <param name="args"></param>
        static void Game_OnUpdate(EventArgs args)
        {
            Hacks.ZoomHack = Config.SubMenu("Hacks").Item("zoomHack").GetValue<bool>();

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
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    LaneClear();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass();
                    break;
                case Orbwalking.OrbwalkingMode.None:
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
                    if (Config.SubMenu("KSMenu").Item("KSAA").GetValue<bool>() || Config.SubMenu("KSMenu").Item("KSQ").GetValue<bool>() || Config.SubMenu("KSMenu").Item("KSR").GetValue<bool>())
                    {
                        KS();
                    }
                    break;
            }
        }

        #endregion

        #region Drawing

        static void Drawing_OnDraw(EventArgs args)
        {
            #region Debug

            if (Config.SubMenu("Drawing").SubMenu("debug").Item("debugdraw").GetValue<bool>())
            {
                Drawing.DrawText(
                    Config.SubMenu("Drawing").SubMenu("debug").Item("x").GetValue<Slider>().Value,
                    Config.SubMenu("Drawing").SubMenu("debug").Item("y").GetValue<Slider>().Value,
                    System.Drawing.Color.Red,
                    Player.ServerPosition.ToString());
            }

            #endregion

            #region Skills

            var drawQ = Config.SubMenu("Drawing").Item("drawQ").GetValue<bool>();
            var drawR = Config.SubMenu("Drawing").Item("drawR").GetValue<bool>();
            var colorBlind = Config.SubMenu("Drawing").Item("colorBlind").GetValue<bool>();
            var player = ObjectManager.Player.Position;

            if (drawQ && colorBlind)
            {
                Render.Circle.DrawCircle(player, Q.Range, Q.IsReady() ? System.Drawing.Color.YellowGreen : System.Drawing.Color.Red);
            }

            if (drawQ && !colorBlind)
            {
                Render.Circle.DrawCircle(player, Q.Range, Q.IsReady() ? System.Drawing.Color.LightGreen : System.Drawing.Color.Red);
            }

            if (drawR && colorBlind)
            {
                Render.Circle.DrawCircle(player, R.Range, R.IsReady() ? System.Drawing.Color.YellowGreen : System.Drawing.Color.Red);
            }

            if (drawR && !colorBlind)
            {
                Render.Circle.DrawCircle(player, R.Range, R.IsReady() ? System.Drawing.Color.LightGreen : System.Drawing.Color.Red);
            }

            #endregion

            #region R Location

            var drawautoR = Config.SubMenu("Drawing").Item("drawautoR").GetValue<bool>();

            if (drawautoR && Utility.Map.GetMap().Type == Utility.Map.MapType.SummonersRift)
            {
                foreach (var place in ShroomPositions.SummonersRift.Where(pos => pos.Distance(ObjectManager.Player.Position) <= Config.SubMenu("Drawing").Item("DrawVision").GetValue<Slider>().Value))
                {
                    if (colorBlind)
                    {
                        Render.Circle.DrawCircle(place, 100, IsShroomed(place) ? System.Drawing.Color.Red : System.Drawing.Color.YellowGreen);
                    }
                    else
                    {
                        Render.Circle.DrawCircle(place, 100, IsShroomed(place) ? System.Drawing.Color.Red : System.Drawing.Color.LightGreen);
                    }
                }
            }

            else if (drawautoR && Utility.Map.GetMap().Type == Utility.Map.MapType.CrystalScar)
            {
                foreach (var place in ShroomPositions.CrystalScar.Where(pos => pos.Distance(ObjectManager.Player.Position) <= Config.SubMenu("Drawing").Item("DrawVision").GetValue<Slider>().Value))
                {
                    if (colorBlind)
                    {
                        Render.Circle.DrawCircle(place, 100, IsShroomed(place) ? System.Drawing.Color.Red : System.Drawing.Color.YellowGreen);
                    }
                    else
                    {
                        Render.Circle.DrawCircle(place, 100, IsShroomed(place) ? System.Drawing.Color.Red : System.Drawing.Color.LightGreen);
                    }
                }
            }

            else if (drawautoR && Utility.Map.GetMap().Type == Utility.Map.MapType.HowlingAbyss)
            {
                foreach (var place in ShroomPositions.HowlingAbyss.Where(pos => pos.Distance(ObjectManager.Player.Position) <= Config.SubMenu("Drawing").Item("DrawVision").GetValue<Slider>().Value))
                {
                    if (colorBlind)
                    {
                        Render.Circle.DrawCircle(place, 100, IsShroomed(place) ? System.Drawing.Color.Red : System.Drawing.Color.YellowGreen);
                    }
                    else
                    {
                        Render.Circle.DrawCircle(place, 100, IsShroomed(place) ? System.Drawing.Color.Red: System.Drawing.Color.LightGreen);
                    }
                }
            }

            else if (drawautoR && Utility.Map.GetMap().Type == Utility.Map.MapType.TwistedTreeline)
            {
                foreach (var place in ShroomPositions.TwistedTreeline.Where(pos => pos.Distance(ObjectManager.Player.Position) <= Config.SubMenu("Drawing").Item("DrawVision").GetValue<Slider>().Value))
                {
                    if (colorBlind)
                    {
                        Render.Circle.DrawCircle(place, 100, IsShroomed(place) ? System.Drawing.Color.Red : System.Drawing.Color.YellowGreen);
                    }
                    else
                    {
                        Render.Circle.DrawCircle(place, 100, IsShroomed(place) ? System.Drawing.Color.Red : System.Drawing.Color.LightGreen);
                    }
                }
            }

            else if (drawautoR && Utility.Map.GetMap().Type == Utility.Map.MapType.Unknown)
            {
                foreach (var place in ShroomPositions.ButcherBridge.Where(pos => pos.Distance(ObjectManager.Player.Position) <= Config.SubMenu("Drawing").Item("DrawVision").GetValue<Slider>().Value))
                {
                    if (colorBlind)
                    {
                        Render.Circle.DrawCircle(place, 100, IsShroomed(place) ? System.Drawing.Color.Red : System.Drawing.Color.YellowGreen);
                    }
                    else
                    {
                        Render.Circle.DrawCircle(place, 100, IsShroomed(place) ? System.Drawing.Color.Red : System.Drawing.Color.LightGreen);
                    }
                }
            }

            #endregion
        }

        #endregion

    }
}