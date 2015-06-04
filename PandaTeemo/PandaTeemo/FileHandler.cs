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

        private static string[] xString = File.ReadAllLines(xFile);
        private static string[] zString = File.ReadAllLines(zFile);
        private static string[] yString = File.ReadAllLines(yFile);

        /// <summary>
        /// Array of X Int
        /// </summary>
        public static int[] xInt = new int[xString.Count()];

        /// <summary>
        /// Array of Z Int
        /// </summary>
        public static int[] zInt = new int[zString.Count()];

        /// <summary>
        /// Array of Y Int
        /// </summary>
        public static int[] yInt = new int[yString.Count()];

        /// <summary>
        /// Initilize the FileHandler
        /// </summary>
        public FileHandler()
        {
            DoChecks();
        }

        /// <summary>
        /// List of the Position of the Shroom
        /// </summary>
        public static List<Vector3> Position = new List<Vector3>();

        /// <summary>
        /// Checks for missing files, Converts the values to int, then adds them into a Vector3 List
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
                    xInt[i] = Convert.ToInt32(xString[i]);
                }

                for (var i = 0; i < xString.Count(); i++)
                {
                    zInt[i] = Convert.ToInt32(zString[i]);
                }

                for (var i = 0; i < xString.Count(); i++)
                {
                    yInt[i] = Convert.ToInt32(yString[i]);
                }

                GetShroomLocation();
                Notifications.AddNotification("FileHandler Initialized", 10000, true);
            }
        }
        
        /// <summary>
        /// Gets the location of the shroom and adds it to the list
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
