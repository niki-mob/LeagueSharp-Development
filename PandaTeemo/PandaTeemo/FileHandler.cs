using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace PandaTeemo
{
    /// <summary>
    /// Some part of the code is taken from AiM
    /// </summary>
    internal class FileHandler
    {
        private static string ShroomLocation = LeagueSharp.Common.Config.AppDataDirectory + @"\PandaTeemo\";

        static string xFile = ShroomLocation + Utility.Map.GetMap().Type + @"\" + "xFile" + ".txt";
        static string yFile = ShroomLocation + Utility.Map.GetMap().Type + @"\" + "yFile" + ".txt";
        static string zFile = ShroomLocation + Utility.Map.GetMap().Type + @"\" + "zFile" + ".txt";

        public static string[] xString = File.ReadAllLines(xFile);
        public static string[] zString = File.ReadAllLines(zFile);
        public static string[] yString = File.ReadAllLines(yFile);

        public static int[] xInt = new int[xString.Count()];
        public static int[] zInt = new int[zString.Count()];
        public static int[] yInt = new int[yString.Count()];

        /// <summary>
        /// Main
        /// </summary>
        public FileHandler()
        {
            DoChecks();
        }

        /// <summary>
        /// Position of the Shroom
        /// </summary>
        public static List<Vector3> Position = new List<Vector3>();

        /// <summary>
        /// Checks for missing files
        /// </summary>
        public static void DoChecks()
        {
            #region Check Missing Files

            if (!Directory.Exists(ShroomLocation))
            {
                Directory.CreateDirectory(ShroomLocation);
                Directory.CreateDirectory(ShroomLocation + Utility.Map.MapType.CrystalScar);
                Directory.CreateDirectory(ShroomLocation + Utility.Map.MapType.HowlingAbyss);
                Directory.CreateDirectory(ShroomLocation + Utility.Map.MapType.SummonersRift);
                Directory.CreateDirectory(ShroomLocation + Utility.Map.MapType.TwistedTreeline);
            }

            else if (!File.Exists(xFile))
            {
                var newfile = File.Create(xFile);
                newfile.Close();
                var content = "0\n";
                var seperator = new[] { "\n" };
                var lines = content.Split(seperator, StringSplitOptions.None);
                File.WriteAllLines(xFile, lines);
            }

            else if (!File.Exists(yFile))
            {
                var newfile = File.Create(yFile);
                newfile.Close();
                var content = "0\n";
                var seperator = new[] { "\n" };
                var lines = content.Split(seperator, StringSplitOptions.None);
                File.WriteAllLines(yFile, lines);
            }

            else if (!File.Exists(zFile))
            {
                var newfile = File.Create(zFile);
                newfile.Close();
                var content = "0\n";
                var seperator = new[] { "\n" };
                var lines = content.Split(seperator, StringSplitOptions.None);
                File.WriteAllLines(zFile, lines);
            }

            #endregion

            else
            {
                for (var i = 0; i < xString.Count(); i++)
                {
                    //Notifications.AddNotification("Converted xString", 10000, true);
                    xInt[i] = Convert.ToInt32(xString[i]);
                }

                for (var i = 0; i < xString.Count(); i++)
                {
                    //Notifications.AddNotification("Converted zString", 10000, true);
                    zInt[i] = Convert.ToInt32(zString[i]);
                }

                for (var i = 0; i < xString.Count(); i++)
                {
                    //Notifications.AddNotification("Converted yString", 10000, true);
                    yInt[i] = Convert.ToInt32(yString[i]);
                }

                GetShroomLocation();
                Notifications.AddNotification("FileHandler Initialized", 10000, true);
            }
        }
        
        /// <summary>
        /// Gets the location of the shroom
        /// </summary>
        public static void GetShroomLocation()
        {
            for (var i = 0; i < xInt.Count(); i++)
            {
                Position.Add(new Vector3(xInt[i], zInt[i], yInt[i]));
            }
        }
    }
}
