using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using Color = System.Drawing.Color;

namespace NasusTheLumberJack
{
    /// <summary>
    /// This program is created by KarmaPanda
    /// </summary>
    internal class Program
    {
        #region Initilization

        /// <summary>
        /// Champion Name
        /// </summary>
        public const string _Name = "Nasus";

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

        /// <summary>
        /// Orbwalker
        /// </summary>
        public static Orbwalking.Orbwalker Orbwalker;
        
        /// <summary>
        /// Menu
        /// </summary>
        public static Menu Menu;

        /// <summary>
        /// Player
        /// </summary>
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        /// <summary>
        /// Called when program starts
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        /// <summary>
        /// Called when game is loaded
        /// </summary>
        /// <param name="args"></param>
        static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.BaseSkinName != "Nasus")
            {
                return;
            }

            Q = new Spell(SpellSlot.Q, Player.AttackRange + 50);
            W = new Spell(SpellSlot.W, 600);
            E = new Spell(SpellSlot.E, 650);
            R = new Spell(SpellSlot.R, 20);

            Menu = new Menu("Nasus the Lumber Jack", "kpNasus", true);
            Menu.AddToMainMenu();

            // Target Selector
            var tsMenu = new Menu("Target Selector", "ts");
            TargetSelector.AddToMenu(tsMenu);
            Menu.AddSubMenu(tsMenu);

            // Orbwalker
            var orbwalkMenu = new Menu("Orbwalker", "Orbwalker");
            Orbwalker = new Orbwalking.Orbwalker(orbwalkMenu);
            Menu.AddSubMenu(orbwalkMenu);

            // Combo
            var comboMenu = new Menu("Combo Menu", "combo");
            comboMenu.AddItem(new MenuItem("useQCombo", "Use Q")).SetValue(true);
            comboMenu.AddItem(new MenuItem("useWCombo", "Use W")).SetValue(true);
            comboMenu.AddItem(new MenuItem("useECombo", "Use E")).SetValue(true);
            comboMenu.AddItem(new MenuItem("useRCombo", "Use R")).SetValue(true);
            comboMenu.AddItem(new MenuItem("useRHP", "HP before using R").SetValue(new Slider(35, 0, 100)));
            Menu.AddSubMenu(comboMenu);

            // LastHit
            var lastHitMenu = new Menu("LastHit Menu", "lasthit");
            lastHitMenu.AddItem(new MenuItem("useQLastHit", "Use Q To LastHit")).SetValue(true);
            lastHitMenu.AddItem(new MenuItem("manamanagerQ", "Mana Percent before using Q").SetValue(new Slider(50, 0, 100)));
            Menu.AddSubMenu(lastHitMenu);

            // Harass
            var harassMenu = new Menu("Harass Menu", "harass");
            harassMenu.AddItem(new MenuItem("useQHarass", "Use Q To Harass")).SetValue(false);
            harassMenu.AddItem(new MenuItem("useQHarass2", "Use Q To LastHit")).SetValue(true);
            harassMenu.AddItem(new MenuItem("manamanagerQ", "Mana Percent before using Q").SetValue(new Slider(50, 0, 100)));
            harassMenu.AddItem(new MenuItem("useWHarass", "Use W")).SetValue(false);
            harassMenu.AddItem(new MenuItem("useEHarass", "Use E to Harass")).SetValue(false);
            Menu.AddSubMenu(harassMenu);

            // LaneClear
            var laneClearMenu = new Menu("LaneClear Menu", "laneclear");
            laneClearMenu.AddItem(new MenuItem("laneclearQ", "Use Q only when killable")).SetValue(true);
            laneClearMenu.AddItem(new MenuItem("laneclearE", "Use E")).SetValue(true);
            laneClearMenu.AddItem(new MenuItem("eKillOnly", "Use E only if killable")).SetValue(false);
            Menu.AddSubMenu(laneClearMenu);
            
            // Misc
            var miscMenu = new Menu("Misc Menu", "misc");
            miscMenu.AddItem(new MenuItem("aaDisable", "Disable AA if Q isn't active during LastHit and Mixed")).SetValue(false);
            Menu.AddSubMenu(miscMenu);

            // Drawing
            var drawMenu = new Menu("Drawing", "Drawing");
            drawMenu.AddItem(new MenuItem("DrawE", "Draw E Range")).SetValue(true);
            Menu.AddSubMenu(drawMenu);
            
            // Notification
            Notifications.AddNotification("Nasus The Lumber Jack", 10000, true);
            Notifications.AddNotification("Version 1.0.1.0", 10000, true);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;
        }

        #endregion

        #region Methods

        #region Combo

        /// <summary>
        /// Combo Mode
        /// </summary>
        static void Combo()
        {
            var wtarget = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical);
            var etarget = TargetSelector.GetTarget(E.Range + E.Width, TargetSelector.DamageType.Magical);

            if (wtarget == null || !wtarget.IsValid || etarget == null || !etarget.IsValid)
            {
                return;
            }

            else
            {
                var useWCombo = Menu.SubMenu("combo").Item("useWCombo").GetValue<bool>();
                var useECombo = Menu.SubMenu("combo").Item("useECombo").GetValue<bool>();
                var useRCombo = Menu.SubMenu("combo").Item("useRCombo").GetValue<bool>();
                var useRHP = Menu.SubMenu("combo").Item("useRHP").GetValue<Slider>().Value;

                // W
                if (useWCombo && W.IsReady() && W.IsInRange(wtarget))
                {
                    W.Cast(wtarget);
                }
                else
                {
                    return;
                }

                // E
                if (useECombo && E.IsReady() && etarget.IsValidTarget() && E.IsInRange(etarget))
                {
                    var prediction = E.GetPrediction(etarget).Hitchance;
                    if (prediction >= HitChance.VeryHigh)
                    {
                        E.Cast(etarget);
                    }
                    else
                    {
                        return;
                    }
                }

                // R
                if (useRCombo && R.IsReady() && Player.CountEnemiesInRange(E.Range) >= 1 && Player.HealthPercent <= useRHP)
                {
                    R.CastOnUnit(Player);
                }
                else
                {
                    return;
                }
            }
        }

        #endregion

        #region LaneClear

        /// <summary>
        /// LaneClear Mode
        /// </summary>
        static void LaneClear()
        {
            var qMinion = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.Health);
            var laneclearQ = Menu.SubMenu("laneclear").Item("laneclearQ").GetValue<bool>();
            var laneclearE = Menu.SubMenu("laneclear").Item("laneclearE").GetValue<bool>();
            var eKillOnly = Menu.SubMenu("laneclear").Item("eKillOnly").GetValue<bool>();
            var eMinion = MinionManager.GetMinions(E.Range + E.Width, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.Health);
            var eLocation = E.GetCircularFarmLocation(eMinion, E.Range);

            if (laneclearQ)
            {
                foreach (var minion in qMinion)
                {
                    if (minion.Health <= Q.GetDamage(minion) && laneclearQ)
                    {
                        Q.Cast();
                        Player.IssueOrder(GameObjectOrder.AttackUnit, minion);
                    }
                }
            }

            if (laneclearE && eKillOnly)
            {
                foreach(var minion in eMinion)
                {
                    if (minion.Health <= E.GetDamage(minion) && E.IsInRange(minion))
                    {
                        E.Cast(eLocation.Position);
                    }
                }
            }

            else if (laneclearE)
            {
                foreach (var minion in eMinion)
                {
                    if (E.IsInRange(minion))
                    {
                        E.Cast(eLocation.Position);
                    }
                }
            }

            else
            {
                return;
            }
        }

        #endregion

        #region Harass

        /// <summary>
        /// Harass Mode
        /// </summary>
        static void Harass()
        {
            var useQHarass = Menu.SubMenu("harass").Item("useQHarass").GetValue<bool>();
            var useWHarass = Menu.SubMenu("harass").Item("useWHarass").GetValue<bool>();
            var useEHarass = Menu.SubMenu("harass").Item("useEHarass").GetValue<bool>();

            if (useQHarass)
            {
                var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);

                if (target.IsValidTarget() && Q.IsInRange(target) && Q.IsReady())
                {
                    Q.Cast();
                    Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                }
            }

            if (useWHarass)
            {
                var wTarget = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical);

                if (wTarget.IsValidTarget(W.Range, true) && W.IsInRange(wTarget) && W.IsReady())
                {
                    W.Cast(wTarget);
                }
            }

            if (useEHarass)
            {
                var eTarget = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);

                if (eTarget.IsValidTarget(E.Range, true) && E.IsInRange(eTarget) && E.IsReady())
                {
                    E.CastIfHitchanceEquals(eTarget, HitChance.VeryHigh);
                }
            }
        }

        #endregion

        #endregion

        #region OnUpdate

        /// <summary>
        /// Called when game update
        /// </summary>
        /// <param name="args"></param>
        static void Game_OnUpdate(EventArgs args)
        {
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
            }
        }

        #endregion

        #region BeforeAttack

        static void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                #region Combo

                case Orbwalking.OrbwalkingMode.Combo:
                    var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
                    var useQCombo = Menu.SubMenu("combo").Item("useQCombo").GetValue<bool>();

                    if (target.IsValidTarget() && Q.IsInRange(target) && Q.IsReady() && useQCombo)
                    {
                        Q.Cast();
                        Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                    }
                    else
                    {
                        return;
                    }
                    break;

                #endregion

                #region LastHit

                case Orbwalking.OrbwalkingMode.LastHit:
                    var useQLastHit = Menu.SubMenu("lasthit").Item("useQLastHit").GetValue<bool>();
                    var aaDisable = Menu.SubMenu("misc").Item("aaDisable").GetValue<bool>();
                    var manamanagerQ = Menu.SubMenu("lasthit").Item("manamanagerQ").GetValue<Slider>().Value;
                    var minionQ = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.Health);

                    if (useQLastHit && aaDisable)
                    {
                        args.Process = false;
                        foreach (var minion in minionQ)
                        {
                            if (manamanagerQ <= Player.ManaPercent && minion.Health <= Q.GetDamage(minion) + Player.GetAutoAttackDamage(minion) && Q.IsReady())
                            {
                                Q.Cast();
                                Player.IssueOrder(GameObjectOrder.AttackUnit, minion);
                                args.Process = true;
                            }
                        }
                    }
                    else if (useQLastHit)
                    {
                        foreach (var minion in minionQ)
                        {
                            if (manamanagerQ <= Player.ManaPercent && minion.Health <= Q.GetDamage(minion) + Player.GetAutoAttackDamage(minion) && Q.IsReady())
                            {
                                Q.Cast();
                                Player.IssueOrder(GameObjectOrder.AttackUnit, minion);
                            }
                        }
                    }
                    else
                    {
                        return;
                    }
                    break;

                #endregion

                #region Mixed

                case Orbwalking.OrbwalkingMode.Mixed:
                    var useQHarass2 = Menu.SubMenu("harass").Item("useQHarass2").GetValue<bool>();
                    aaDisable = Menu.SubMenu("misc").Item("aaDisable").GetValue<bool>();

                    if (useQHarass2 && aaDisable)
                    {
                        args.Process = false;
                        minionQ = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.Health);
                        manamanagerQ = Menu.SubMenu("harass").Item("manamanagerQ").GetValue<Slider>().Value;

                        foreach (var minion in minionQ)
                        {
                            if (manamanagerQ <= Player.ManaPercent && minion.Health <= Q.GetDamage(minion) + Player.GetAutoAttackDamage(minion) && Q.IsReady())
                            {
                                Q.Cast();
                                Player.IssueOrder(GameObjectOrder.AttackUnit, minion);
                                args.Process = true;
                            }
                        }
                    }

                    else if (useQHarass2)
                    {
                        minionQ = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.Health);
                        manamanagerQ = Menu.SubMenu("harass").Item("manamanagerQ").GetValue<Slider>().Value;

                        foreach (var minion in minionQ)
                        {
                            if (manamanagerQ <= Player.ManaPercent && minion.Health <= Q.GetDamage(minion) + Player.GetAutoAttackDamage(minion) && Q.IsReady())
                            {
                                Q.Cast();
                                Player.IssueOrder(GameObjectOrder.AttackUnit, minion);
                            }
                        }
                    }

                    else
                    {
                        return;
                    }

                    break;

                #endregion
            }
        }

        #endregion

        #region Drawing

        /// <summary>
        /// Calls when game draws
        /// </summary>
        /// <param name="args"></param>
        static void Drawing_OnDraw(EventArgs args)
        {
            var DrawE = Menu.SubMenu("Drawing").Item("DrawE").GetValue<bool>();

            if (DrawE)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, E.IsReady() ? System.Drawing.Color.YellowGreen : System.Drawing.Color.Red);
            }
        }

        #endregion

    }
}
