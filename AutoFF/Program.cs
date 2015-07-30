using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace AutoFF
{
    class Program
    {
        public static Menu Config;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            Config = new Menu("Auto Surrender", "menu", true);

            Config.AddItem(new MenuItem("toggle", "Auto Surrender at Time Set").SetValue(true));
            Config.AddItem(new MenuItem("time", "Set Time for Surrender").SetValue(new Slider(20, 15, 120)));
            Config.AddToMainMenu();

            Notifications.AddNotification("Auto FF Initialized", 10000, true);

            Game.OnUpdate += Game_OnUpdate;
        }

        static void Game_OnUpdate(EventArgs args)
        {
            var time = Config.Item("time").GetValue<Slider>().Value;
            var surrender = false;
            var surrendertime = 0;

            if (surrender)
            {
                surrendertime += 1;
                if (surrendertime >= 3000)
                {
                    surrender = false;
                }
            }

            if (Game.ClockTime >= time * 60 && Config.Item("toggle").GetValue<bool>() && !surrender)
            {
                Game.Say("/ff");
                surrender = true;
            }
        }
    }
}
