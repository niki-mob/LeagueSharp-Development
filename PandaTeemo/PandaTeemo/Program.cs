﻿namespace PandaTeemo
{
    using System;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    using SharpDX;

    using Color = System.Drawing.Color;

    /// <summary>
    /// Made by KarmaPanda
    /// </summary>
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
        static readonly string[] Marksman = { "Ashe", "Caitlyn", "Corki", "Draven", "Ezreal", "Jinx", "Kalista", "KogMaw", "Lucian", "MissFortune", "Quinn", "Sivir", "Teemo", "Tristana", "Twitch", "Urgot", "Varus", "Vayne" };

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
        public static FileHandler FileHandler;
        
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

        public static double TeemoE (Obj_AI_Base minion)
        {
            { return Player.GetSpellDamage(minion, SpellSlot.E); }
        }

        /// <summary>
        /// Teemo's R Range
        /// </summary>
        public static float RRange
        {
            get { return 300 * R.Level; }
        }

        /// <summary>
        /// Called when program starts
        /// </summary>
        static void Main()
        {
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

            // Updated data for Teemo's Q and R
            Q = new Spell(SpellSlot.Q, 680);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R, 300);

            Q.SetTargetted(0.5f, 1500f);
            R.SetSkillshot(0.5f, 120f, 1000f, false, SkillshotType.SkillshotCircle);

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
            var interrupt = Config.AddSubMenu(new Menu("Interrupt / Gapcloser", "Interrupt"));
            var misc = Config.AddSubMenu(new Menu("Misc", "Misc"));
            var hacks = Config.AddSubMenu(new Menu("Hack Menu", "Hacks"));

            // Main Menu
            Orbwalker = new Orbwalking.Orbwalker(orbwalking);

            // Combo Menu
            combo.AddItem(new MenuItem("qcombo", "Use Q in Combo").SetValue(true));
            combo.AddItem(new MenuItem("wcombo", "Use W in Combo").SetValue(true));
            combo.AddItem(new MenuItem("rcombo", "Kite with R in Combo").SetValue(true));
            combo.AddItem(new MenuItem("useqADC", "Use Q only on ADC during Combo").SetValue(false));
            combo.AddItem(new MenuItem("wCombat", "Use W if enemy is in range only").SetValue(false));
            combo.AddItem(new MenuItem("rCharge", "Charges of R before using R").SetValue(new Slider(2, 1, 3)));
            combo.AddItem(new MenuItem("checkCamo", "Prevents combo being activated while stealth in brush").SetValue(false));

            // Harass Menu
            harass.AddItem(new MenuItem("qharass", "Harass with Q").SetValue(true));

            // LaneClear Menu
            laneclear.AddItem(new MenuItem("qclear", "LaneClear with Q").SetValue(true));
            laneclear.AddItem(new MenuItem("qManaManager", "Q Mana Manager").SetValue(new Slider(75)));
            laneclear.AddItem(new MenuItem("attackTurret", "Attack Turret").SetValue(true));
            laneclear.AddItem(new MenuItem("attackWard", "Attack Ward").SetValue(true));
            laneclear.AddItem(new MenuItem("rclear", "LaneClear with R").SetValue(true));
            laneclear.AddItem(new MenuItem("userKill", "Use R only if Killable").SetValue(true));
            laneclear.AddItem(new MenuItem("minionR", "Minion for R").SetValue(new Slider(3, 1, 4)));

            // JungleClear Menu
            jungleclear.AddItem(new MenuItem("qclear", "JungleClear with Q").SetValue(true));
            jungleclear.AddItem(new MenuItem("rclear", "JungleClear with R").SetValue(true));

            // Interrupter && Gapcloser
            interrupt.AddItem(new MenuItem("intq", "Interrupt with Q").SetValue(true));
            interrupt.AddItem(new MenuItem("intChance", "Danger Level before using Q").SetValue(new StringList(new[] { "High", "Medium", "Low" })));
            interrupt.AddItem(new MenuItem("gapR", "Gapclose with R").SetValue(true));

            // KillSteal Menu
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
            misc.AddItem(new MenuItem("customLocationInt", "Set the amount of locations you have (Requires Reload)").SetValue(new Slider(1, 1, 25)));
            misc.AddItem(new MenuItem("packets", "Use Packets").SetValue(false));
            misc.AddItem(new MenuItem("checkAA", "Subtract Range for Q (checkAA)").SetValue(true));
            misc.AddItem(new MenuItem("checkaaRange", "How many to subtract from Q Range (checkAA)").SetValue(new Slider(100, 0, 180)));

            hacks.AddItem(new MenuItem("zoomHack", "Zoom Hack Enabler (DISABLED)").SetValue(false));

            Config.AddToMainMenu();

            #endregion

            // Events
            Game.OnUpdate += Game_OnUpdate;
            Interrupter2.OnInterruptableTarget += Interrupter_OnPossibleToInterrupt;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Orbwalking.AfterAttack += OrbwalkingAfterAttack;
            Orbwalking.BeforeAttack += OrbwalkingBeforeAttack;
            Drawing.OnDraw += Drawing_OnDraw;

            // GG PrintChat Bik™
            Game.PrintChat("<font color='#FBF5EF'>Game.PrintChat Bik</font> - <font color = '#01DF3A'>PandaTeemo v1.7.5.3 Loaded</font>");

            // Loads ShroomPosition
            FileHandler = new FileHandler();
            ShroomPositions = new ShroomTables();
        }

        #endregion

        #region BeforeAttack

        /// <summary>
        /// Actions before attacking
        /// </summary>
        /// <param name="args">Attack Action</param>
        static void OrbwalkingBeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            #region LastHit

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit)
            {
                foreach (var minion in MinionManager.GetMinions(ObjectManager.Player.Position, Player.AttackRange))
                {
                    if (minion.Health <= ObjectManager.Player.GetAutoAttackDamage(minion) + TeemoE(minion))
                    {
                        Player.IssueOrder(GameObjectOrder.AttackUnit, minion);
                    }
                    else
                    {
                        return;
                    }
                }
            }

            #endregion

            #region Harass

            else if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                args.Process = false;
                var enemy = HeroManager.Enemies.OrderBy(t => t.Health).FirstOrDefault();
                var minion = MinionManager.GetMinions(ObjectManager.Player.Position, Player.AttackRange).Where(t => t.IsEnemy && Orbwalker.InAutoAttackRange(t)).OrderBy(t => t.Health).FirstOrDefault();

                if (minion != null && minion.Health <= ObjectManager.Player.GetAutoAttackDamage(minion) + TeemoE(minion) && !Orbwalker.InAutoAttackRange(enemy))
                {
                    Player.IssueOrder(GameObjectOrder.AttackUnit, minion);
                }

                if (minion != null && minion.Health <= ObjectManager.Player.GetAutoAttackDamage(minion) + TeemoE(minion) && Orbwalker.InAutoAttackRange(enemy))
                {
                    Player.IssueOrder(GameObjectOrder.AttackUnit, minion);
                }

                if (enemy != null && minion.Health >= ObjectManager.Player.GetAutoAttackDamage(minion) + TeemoE(minion) && Orbwalker.InAutoAttackRange(enemy))
                {
                    Player.IssueOrder(GameObjectOrder.AttackUnit, enemy);
                }

                return;
            }

            #endregion

            #region LaneClear

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                var attackTurret = Config.SubMenu("LaneClear").Item("attackTurret").GetValue<bool>();
                var attackWard = Config.SubMenu("LaneClear").Item("attackWard").GetValue<bool>();
                var useQ = Config.SubMenu("LaneClear").Item("qclear").GetValue<bool>();
                var turret = ObjectManager.Get<Obj_AI_Turret>().Where(t => Orbwalking.InAutoAttackRange(t) && t.IsEnemy).OrderBy(t => t.Health).FirstOrDefault();
                var inhib = ObjectManager.Get<Obj_BarracksDampener>().Where(t => Orbwalking.InAutoAttackRange(t) && t.IsEnemy).OrderBy(t => t.Health).FirstOrDefault();
                var nexus = ObjectManager.Get<Obj_HQ>().Where(t => Orbwalking.InAutoAttackRange(t) && t.IsEnemy).OrderBy(t => t.Health).FirstOrDefault();
                var ward = MinionManager.GetMinions(Player.AttackRange, MinionTypes.Wards).FirstOrDefault();
                var mob = MinionManager.GetMinions(Player.AttackRange, MinionTypes.All, MinionTeam.NotAlly);

                foreach (var m in mob)
                {
                    if (!Orbwalker.InAutoAttackRange(m))
                    {
                        #region Turret

                        if (attackTurret)
                        {
                            if (turret != null)
                            {
                                Player.IssueOrder(GameObjectOrder.AttackUnit, turret);
                                Utility.DelayAction.Add(1500, () => Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos));
                            }
                            if (inhib != null)
                            {
                                Player.IssueOrder(GameObjectOrder.AttackUnit, inhib);
                                Utility.DelayAction.Add(1500, () => Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos));
                            }
                            if (nexus != null)
                            {
                                Player.IssueOrder(GameObjectOrder.AttackUnit, nexus);
                                Utility.DelayAction.Add(1500, () => Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos));
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

                    else if (Orbwalker.InAutoAttackRange(m))
                    {
                        if (Orbwalker.InAutoAttackRange(turret) && attackTurret
                            || Orbwalker.InAutoAttackRange(inhib) && attackTurret
                            || Orbwalker.InAutoAttackRange(nexus) && attackTurret
                            || Orbwalker.InAutoAttackRange(ward) && attackWard)
                        {
                            #region Turret

                            if (attackTurret)
                            {
                                if (m.Health < Player.GetAutoAttackDamage(m) + TeemoE(m))
                                {
                                    Player.IssueOrder(GameObjectOrder.AttackUnit, m);
                                }

                                if (turret != null && m.Health > Player.GetAutoAttackDamage(m) + TeemoE(m))
                                {
                                    Player.IssueOrder(GameObjectOrder.AttackUnit, turret);
                                }
                                if (inhib != null && m.Health > Player.GetAutoAttackDamage(m) + TeemoE(m))
                                {
                                    Player.IssueOrder(GameObjectOrder.AttackUnit, inhib);
                                }
                                if (nexus != null && m.Health > Player.GetAutoAttackDamage(m) + TeemoE(m))
                                {
                                    Player.IssueOrder(GameObjectOrder.AttackUnit, nexus);
                                }
                            }

                            #endregion

                            #region Ward

                            if (attackWard)
                            {
                                if (m.Health < Player.GetAutoAttackDamage(m) + TeemoE(m))
                                {
                                    Player.IssueOrder(GameObjectOrder.AttackUnit, m);
                                }

                                if (ward.IsValid && Orbwalker.InAutoAttackRange(ward) && ward.IsEnemy
                                    && m.Health > Player.GetAutoAttackDamage(m) + TeemoE(m))
                                {
                                    Player.IssueOrder(GameObjectOrder.AttackUnit, ward);
                                }
                            }

                            #endregion
                        }

                        else
                        {
                            #region Variables

                            var qManaManager = Config.SubMenu("LaneClear").Item("qManaManager").GetValue<Slider>().Value;

                            #endregion

                            #region Cannot Kill Minion

                            if (useQ && m.Health > Q.GetDamage(m))
                            {
                                Q.CastOnUnit(m, Packets);
                            }

                            if (Player.GetAutoAttackDamage(m) + TeemoE(m) < m.Health)
                            {
                                Player.IssueOrder(GameObjectOrder.AttackUnit, m);
                            }

                            #endregion

                            #region Can Kill Minion

                            else if (m.Health <= Player.GetAutoAttackDamage(m) + TeemoE(m) || m.Health <= Q.GetDamage(m))
                            {
                                if (useQ)
                                {
                                    if (Q.IsReady() && Q.IsInRange(m) && Q.GetDamage(m) >= m.Health
                                        && qManaManager >= (int)Player.ManaPercent)
                                    {
                                        Q.CastOnUnit(m, Packets);
                                    }

                                    else if (Player.Distance3D(m) <= Player.AttackRange)
                                    {
                                        Player.IssueOrder(GameObjectOrder.AttackUnit, m);
                                    }
                                }
                                else
                                {
                                    Player.IssueOrder(GameObjectOrder.AttackUnit, m);
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

            if (gapR && gapcloser.Sender.IsValidTarget() && gapcloser.Sender.IsFacing(Player) && gapcloser.Sender.IsTargetable)
            {
                R.Cast(gapcloser.Sender.Position, Packets);
            }
        }

        #endregion

        #region AfterAttack

        /// <summary>
        /// Action after Attack
        /// </summary>
        /// <param name="unit">Unit Attacked</param>
        /// <param name="target">Target Attacked</param>
        static void OrbwalkingAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            var useQCombo = Config.SubMenu("Combo").Item("qcombo").GetValue<bool>();
            var useQHarass = Config.SubMenu("Harass").Item("qharass").GetValue<bool>();
            var targetAdc = Config.SubMenu("Combo").Item("useqADC").GetValue<bool>();
            var checkAA = Config.SubMenu("Misc").Item("checkAA").GetValue<bool>();
            var checkaaRange = Config.SubMenu("Misc").Item("checkaaRange").GetValue<Slider>().Value;
            var t = target as Obj_AI_Hero;

            if (t != null && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                if (checkAA)
                {
                    if (targetAdc)
                    {
                        foreach (var adc in Marksman)
                        {
                            if (t.CharData.BaseSkinName == adc && useQCombo && Q.IsReady() && Q.IsInRange(t, -checkaaRange))
                            {
                                Q.Cast(t, Packets);
                            }
                            else
                            {
                                return;
                            }
                        }
                    }
                    else
                    {
                        if (useQCombo && Q.IsReady() && Q.IsInRange(t, -checkaaRange))
                        {
                            Q.Cast(t, Packets);
                        }

                        else
                        {
                            return;
                        }
                    }
                }
                else
                {
                    if (targetAdc)
                    {
                        foreach (var adc in Marksman)
                        {
                            if (t.CharData.BaseSkinName == adc && useQCombo && Q.IsReady() && Q.IsInRange(t))
                            {
                                Q.Cast(t, Packets);
                            }
                            else
                            {
                                return;
                            }
                        }
                    }
                    else
                    {
                        if (useQCombo && Q.IsReady() && Q.IsInRange(t))
                        {
                            Q.Cast(t, Packets);
                        }

                        else
                        {
                            return;
                        }
                    }   
                }
            }

            if (t != null && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                if (checkAA)
                {
                    if (useQHarass && Q.IsReady() && Q.IsInRange(t, -100f))
                    {
                        Q.Cast(t, Packets);
                    }
                }
                else
                {
                    if (useQHarass && Q.IsReady() && Q.IsInRange(t))
                    {
                        Q.Cast(t, Packets);
                    }
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
            var checkCamo = Config.SubMenu("Combo").Item("checkCamo").GetValue<bool>();

            if (checkCamo && Player.HasBuff("CamouflageStealth"))
            {
                return;
            }

            var enemies = HeroManager.Enemies.FirstOrDefault(t => t.IsValidTarget() && Orbwalker.InAutoAttackRange(t));
            var rtarget = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
            var useW = Config.SubMenu("Combo").Item("wcombo").GetValue<bool>();
            var useR = Config.SubMenu("Combo").Item("rcombo").GetValue<bool>();
            var wCombat = Config.SubMenu("Combo").Item("wCombat").GetValue<bool>();
            var rCount = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).Ammo;
            var rCharge = Config.SubMenu("Combo").Item("rCharge").GetValue<Slider>().Value;

            if (W.IsReady() && useW && !wCombat)
            {
                W.Cast();
            }

            if (enemies == null)
            {
                return;
            }

            if (useW && wCombat)
            {
                if (W.IsReady())
                {
                    W.Cast();
                }
            }

            if (R.IsReady() && useR && R.IsInRange(rtarget) && rCharge <= rCount && rtarget.IsValidTarget() && !IsShroomed(rtarget.Position))
            {
                R.CastIfHitchanceEquals(rtarget, HitChance.VeryHigh, Packets);
            }

            else if (R.IsReady() && useR && rCharge <= rCount && IsShroomed(rtarget.Position))
            {
                var shroom = ObjectManager.Get<Obj_AI_Base>().FirstOrDefault(t => t.Name == "Noxious Trap");

                if (shroom != null)
                {
                    var shroomPosition = shroom.Position;

                    if (R.IsInRange(rtarget, Player.CharData.SelectionRadius * R.Level + 2) && IsShroomed(shroomPosition))
                    {
                        R.Cast(shroom);
                    }
                }
            }
        }

        #endregion

        #region KillSteal

        /// <summary>
        /// KillSteal
        /// </summary>
        static void KillSteal()
        {
            var ksq = Config.SubMenu("KSMenu").Item("KSQ").GetValue<bool>();
            var ksr = Config.SubMenu("KSMenu").Item("KSR").GetValue<bool>();
            var ksaa = Config.SubMenu("KSMenu").Item("KSAA").GetValue<bool>();

            #region KillSteal with AA

            if (ksaa)
            {
                var aatarget = HeroManager.Enemies.Where(t => 
                    t.IsValidTarget() 
                    && Orbwalker.InAutoAttackRange(t) 
                    && Player.GetAutoAttackDamage(t) + TeemoE(t) >= t.Health).OrderBy(t => t.Health).FirstOrDefault();

                if (aatarget != null)
                {
                    Player.IssueOrder(GameObjectOrder.AttackUnit, aatarget);
                }
                else
                {
                    return;
                }
            }

            #endregion

            #region KillSteal with Q

            if (ksq)
            {
                var target = HeroManager.Enemies.Where(t => t.IsValidTarget()
                    && Q.IsInRange(t) 
                    && Q.GetDamage(t) >= t.Health).OrderBy(t => t.Health).FirstOrDefault();

                if (target != null && Q.IsReady())
                {
                    Q.Cast(target);
                }
                else
                {
                    return;
                }
            }

            #endregion

            #region KillSteal with R

            if (ksr)
            {
                var target = HeroManager.Enemies.Where(t => t.IsValidTarget() 
                    && R.IsInRange(t) 
                    && R.GetDamage(t) >= t.Health).OrderBy(t => t.Health).FirstOrDefault();

                if (target != null && R.IsReady())
                {
                    R.CastIfHitchanceEquals(target, HitChance.VeryHigh, Packets);
                }
            }

            #endregion
        }

        #endregion

        #region LaneClear

        /// <summary>
        /// LaneClear
        /// </summary>
        static void LaneClear()
        {
            #region LaneClear R

            var allMinionsR = MinionManager.GetMinions(ObjectManager.Player.Position, R.Range, MinionTypes.Melee);
            var rangedMinionsR = MinionManager.GetMinions(ObjectManager.Player.Position, R.Range, MinionTypes.Ranged);
            var rLocation = R.GetCircularFarmLocation(allMinionsR, R.Range);
            var r2Location = R.GetCircularFarmLocation(rangedMinionsR, R.Range);
            var useR = Config.SubMenu("LaneClear").Item("rclear").GetValue<bool>();
            var userKill = Config.SubMenu("LaneClear").Item("userKill").GetValue<bool>();
            var minionR = Config.SubMenu("LaneClear").Item("minionR").GetValue<Slider>().Value;

            if (minionR <= rLocation.MinionsHit && useR
                || minionR <= r2Location.MinionsHit && useR
                || minionR <= rLocation.MinionsHit + r2Location.MinionsHit && useR)
            {
                if (userKill)
                {
                    foreach (var minion in allMinionsR)
                    {
                        if (minion.Health <= ObjectManager.Player.GetSpellDamage(minion, SpellSlot.R) 
                            && R.IsReady() 
                            && R.IsInRange(rLocation.Position.To3D()) 
                            && !IsShroomed(rLocation.Position.To3D()) 
                            && minionR <= rLocation.MinionsHit)
                        {
                            R.Cast(rLocation.Position, Packets);
                            return;
                        }
                        if (minion.Health <= ObjectManager.Player.GetSpellDamage(minion, SpellSlot.R) 
                            && R.IsReady() 
                            && R.IsInRange(r2Location.Position.To3D()) 
                            && !IsShroomed(r2Location.Position.To3D()) 
                            && minionR <= r2Location.MinionsHit)
                        {
                            R.Cast(r2Location.Position, Packets);
                            return;
                        }
                    }
                }

                else
                {
                    if (R.IsReady() 
                        && R.IsInRange(rLocation.Position.To3D()) 
                        && !IsShroomed(rLocation.Position.To3D()) 
                        && minionR <= rLocation.MinionsHit)
                    {
                        R.Cast(rLocation.Position, Packets);
                    }
                    else if (R.IsReady() 
                        && R.IsInRange(r2Location.Position.To3D()) 
                        && !IsShroomed(r2Location.Position.To3D()) 
                        && minionR <= r2Location.MinionsHit)
                    {
                        R.Cast(r2Location.Position, Packets);
                    }
                }
            }

            #endregion
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

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                var jungleMobQ = ObjectManager.Get<Obj_AI_Base>().Where(t => Q.IsInRange(t) && t.Team == GameObjectTeam.Neutral && t.IsValidTarget()).OrderBy(t => t.MaxHealth).FirstOrDefault();
                var jungleMobR = ObjectManager.Get<Obj_AI_Base>().Where(t => R.IsInRange(t) && t.Team == GameObjectTeam.Neutral && t.IsValidTarget()).OrderBy(t => t.MaxHealth).FirstOrDefault();

                if (useQ && jungleMobQ != null)
                {
                    if (Q.IsReady() && qManaManager <= (int)Player.ManaPercent)
                    {
                        Q.CastOnUnit(jungleMobQ, Packets);
                    }
                }

                if (useR && jungleMobR != null)
                {
                    if (R.IsReady() && ammoR >= 1)
                    {
                        R.Cast(jungleMobR.Position, Packets);
                    }
                }
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
            var intq = Config.SubMenu("Interrupt").Item("intq").GetValue<bool>();
            var intChance = Config.SubMenu("Interrupt").Item("intChance").GetValue<StringList>().SelectedValue;

            // High Danger Level
            if (intChance == "High" && intq && Q.IsReady() && args.DangerLevel == Interrupter2.DangerLevel.High)
            {
                if (sender != null)
                {
                    Q.Cast(sender, Packets);
                }
            }

            // Medium Danger Level
            else if (intChance == "Medium" && intq && Q.IsReady() && args.DangerLevel == Interrupter2.DangerLevel.Medium)
            {
                if (sender != null)
                {
                    Q.Cast(sender, Packets);
                }
            }

            // Low Danger Level
            else if (intChance == "Low" && intq && Q.IsReady() && args.DangerLevel == Interrupter2.DangerLevel.Low)
            {
                if (sender != null)
                {
                    Q.Cast(sender, Packets);
                }
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

            var target = HeroManager.Enemies.FirstOrDefault(t => R.IsInRange(t) && t.IsValidTarget());

            if (target != null)
            {
                if (target.HasBuff("zhonyasringshield") && R.IsReady() && R.IsInRange(target))
                    R.Cast(target.Position, Packets);
            }

            else
            {
                if (Utility.Map.GetMap().Type == Utility.Map.MapType.SummonersRift)
                {
                    foreach (var place in ShroomPositions.SummonersRift.Where(pos => pos.Distance(ObjectManager.Player.Position) <= R.Range && !IsShroomed(pos)))
                    {
                        R.Cast(place, Packets);
                    }         
                }
                else if (Utility.Map.GetMap().Type == Utility.Map.MapType.HowlingAbyss)
                {
                    foreach (var place in ShroomPositions.HowlingAbyss.Where(pos => pos.Distance(ObjectManager.Player.Position) <= R.Range && !IsShroomed(pos)))
                    {
                        R.Cast(place, Packets);
                    }
                }
                else if (Utility.Map.GetMap().Type == Utility.Map.MapType.CrystalScar)
                {
                    // WIP
                    foreach (var place in ShroomPositions.CrystalScar.Where(pos => pos.Distance(ObjectManager.Player.Position) <= R.Range && !IsShroomed(pos)))
                    {
                        R.Cast(place, Packets);
                    }
                }
                else if (Utility.Map.GetMap().Type == Utility.Map.MapType.TwistedTreeline)
                {
                    // WIP
                    foreach (var place in ShroomPositions.TwistedTreeline.Where(pos => pos.Distance(ObjectManager.Player.Position) <= R.Range && !IsShroomed(pos)))
                    {
                        R.Cast(place, Packets);
                    }        
                }
                else if (Utility.Map.GetMap().Type.ToString() == "Unknown")
                {
                    foreach (var place in ShroomPositions.ButcherBridge.Where(pos => pos.Distance(ObjectManager.Player.Position) <= R.Range && !IsShroomed(pos)))
                    {
                        R.Cast(place, Packets);
                    }
                }
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
            var allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.Position, Q.Range);

            if (target == null)
            {
                return;
            }

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
        }

        #endregion

        #region Auto Q & W

        /// <summary>
        /// Auto Q and W
        /// </summary>
        static void AutoQw()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.Position, Q.Range);

            if (!W.IsReady() || !Q.IsReady())
            {
                return;
            }

            if (W.IsReady())
            {
                W.Cast();
            }

            if (target == null)
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
            else if (Q.IsReady() && Q.IsInRange(target) && target.IsValidTarget() && 25 <= Player.ManaPercent)
            {
                Q.Cast(target);
            }
        }

        #endregion

        #region Game_OnUpdate

        /// <summary>
        /// OnUpdate
        /// </summary>
        /// <param name="args"></param>
        static void Game_OnUpdate(EventArgs args)
        {
            //Hacks.ZoomHack = Config.SubMenu("Hacks").Item("zoomHack").GetValue<bool>();
            R.Range = RRange;

            var autoQ = Config.Item("autoQ").GetValue<bool>();
            var autoW = Config.Item("autoW").GetValue<bool>();

            // Reworked Auto Q and W
            if (autoQ && autoW)
            {
                AutoQw();
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
                    JungleClear();
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
                    if (Config.SubMenu("KSMenu").Item("KSAA").GetValue<bool>() 
                        || Config.SubMenu("KSMenu").Item("KSQ").GetValue<bool>() 
                        || Config.SubMenu("KSMenu").Item("KSR").GetValue<bool>())
                    {
                        KillSteal();
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
                    Color.Red,
                    Player.Position.ToString());
            }

            #endregion

            #region Skills

            var drawQ = Config.SubMenu("Drawing").Item("drawQ").GetValue<bool>();
            var drawR = Config.SubMenu("Drawing").Item("drawR").GetValue<bool>();
            var colorBlind = Config.SubMenu("Drawing").Item("colorBlind").GetValue<bool>();
            var player = ObjectManager.Player.Position;

            if (drawQ && colorBlind)
            {
                Render.Circle.DrawCircle(player, Q.Range, Q.IsReady() ? Color.YellowGreen : Color.Red);
            }

            if (drawQ && !colorBlind)
            {
                Render.Circle.DrawCircle(player, Q.Range, Q.IsReady() ? Color.LightGreen : Color.Red);
            }

            if (drawR && colorBlind)
            {
                Render.Circle.DrawCircle(player, R.Range, R.IsReady() ? Color.YellowGreen : Color.Red);
            }

            if (drawR && !colorBlind)
            {
                Render.Circle.DrawCircle(player, R.Range, R.IsReady() ? Color.LightGreen : Color.Red);
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
                        Render.Circle.DrawCircle(place, 100, IsShroomed(place) ? Color.Red : Color.YellowGreen);
                    }
                    else
                    {
                        Render.Circle.DrawCircle(place, 100, IsShroomed(place) ? Color.Red : Color.LightGreen);
                    }
                }
            }

            else if (drawautoR && Utility.Map.GetMap().Type == Utility.Map.MapType.CrystalScar)
            {
                foreach (var place in ShroomPositions.CrystalScar.Where(pos => pos.Distance(ObjectManager.Player.Position) <= Config.SubMenu("Drawing").Item("DrawVision").GetValue<Slider>().Value))
                {
                    if (colorBlind)
                    {
                        Render.Circle.DrawCircle(place, 100, IsShroomed(place) ? Color.Red : Color.YellowGreen);
                    }
                    else
                    {
                        Render.Circle.DrawCircle(place, 100, IsShroomed(place) ? Color.Red : Color.LightGreen);
                    }
                }
            }

            else if (drawautoR && Utility.Map.GetMap().Type == Utility.Map.MapType.HowlingAbyss)
            {
                foreach (var place in ShroomPositions.HowlingAbyss.Where(pos => pos.Distance(ObjectManager.Player.Position) <= Config.SubMenu("Drawing").Item("DrawVision").GetValue<Slider>().Value))
                {
                    if (colorBlind)
                    {
                        Render.Circle.DrawCircle(place, 100, IsShroomed(place) ? Color.Red : Color.YellowGreen);
                    }
                    else
                    {
                        Render.Circle.DrawCircle(place, 100, IsShroomed(place) ? Color.Red: Color.LightGreen);
                    }
                }
            }

            else if (drawautoR && Utility.Map.GetMap().Type == Utility.Map.MapType.TwistedTreeline)
            {
                foreach (var place in ShroomPositions.TwistedTreeline.Where(pos => pos.Distance(ObjectManager.Player.Position) <= Config.SubMenu("Drawing").Item("DrawVision").GetValue<Slider>().Value))
                {
                    if (colorBlind)
                    {
                        Render.Circle.DrawCircle(place, 100, IsShroomed(place) ? Color.Red : Color.YellowGreen);
                    }
                    else
                    {
                        Render.Circle.DrawCircle(place, 100, IsShroomed(place) ? Color.Red : Color.LightGreen);
                    }
                }
            }

            else if (drawautoR && Utility.Map.GetMap().Type == Utility.Map.MapType.Unknown)
            {
                foreach (var place in ShroomPositions.ButcherBridge.Where(pos => pos.Distance(ObjectManager.Player.Position) <= Config.SubMenu("Drawing").Item("DrawVision").GetValue<Slider>().Value))
                {
                    if (colorBlind)
                    {
                        Render.Circle.DrawCircle(place, 100, IsShroomed(place) ? Color.Red : Color.YellowGreen);
                    }
                    else
                    {
                        Render.Circle.DrawCircle(place, 100, IsShroomed(place) ? Color.Red : Color.LightGreen);
                    }
                }
            }

            #endregion
        }

        #endregion

    }
}