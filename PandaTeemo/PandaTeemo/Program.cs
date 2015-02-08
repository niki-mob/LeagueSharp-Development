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

        //Spells
        public static Spell Q;
        public static Spell W;
        public static Spell R;
        
        //Timer
        public static bool QIsReady;
        public static bool WIsReady;
        public static bool RIsReady;

        //Orbwalker
        public static Orbwalking.Orbwalker Orbwalker;
        
        //Menu
        public static Menu Config;

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

            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));

            //Load OrbWalker
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));
            Config.AddToMainMenu();
            Config.AddItem(new MenuItem("autoR", "Automatic Shrooms").SetValue(false));
            Config.AddItem(new MenuItem("autoignite", "Automatic Ignite").SetValue(false));

            //Events
            Game.OnGameUpdate += Game_OnGameUpdate;
            Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;
            Game.PrintChat("PandaTeemo WIP by KarmaPanda");
        }
        static void Orbwalking_BeforeAttack(LeagueSharp.Common.Orbwalking.BeforeAttackEventArgs args)
        {
            if (((Obj_AI_Base)Orbwalker.GetTarget()).IsMinion) args.Process = false;
        }
        private static void Game_OnGameUpdate(EventArgs args)
        {
            if ((Q.IsReady() && Orbwalker.ActiveMode.ToString() == "Combo"))
            {
                var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
                if (t.IsValidTarget() && ObjectManager.Player.GetAutoAttackDamage(t) > t.Health)
                    Orbwalking.Attack = true;
                else
                    Orbwalking.Attack = false;
            }
            else
                Orbwalking.Attack = true;

            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            
            if( Q.IsReady() && target.IsValidTarget())
                Q.Cast(target, true);
        }
    }
}
