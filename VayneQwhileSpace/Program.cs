using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace VayneQwhileSpace
{
    internal class Program
    {
        public const string ChampionName = "Vayne";

        //Spells
        public static Spell Q;

        //Menu
        public static Menu Config;

        private static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.BaseSkinName != ChampionName)
            {
                Game.PrintChat("Champion is not supported");
                return;
            }

            Q = new Spell(SpellSlot.Q, 300);

            Config = new Menu("Vayne", "Vayne", true);
            Config.AddToMainMenu();
            var toggle = Config.AddSubMenu(new Menu("Toggle", "Toggle"));

            toggle.AddItem(new MenuItem("useQ", "Use Q").SetValue(new KeyBind(32 ,KeyBindType.Press)));

            //Event
            Game.OnUpdate += Game_OnUpdate;
            Game.PrintChat("Vayne Loaded");
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if(Config.Item("useQ").IsActive())
            {
                if (Q.IsReady())
                {
                    Q.Cast(Game.CursorPos);
                }
                else
                {
                    Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                }
            }
        }
    }
}
