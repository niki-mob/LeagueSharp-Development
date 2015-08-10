namespace PandaTeemo
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    using SharpDX;

    /// <summary>
    /// Original Code Credit: AiM
    /// </summary>
    internal class FileHandler
    {
        #region Fields

        /// <summary>
        /// List of the Position of the Shroom
        /// </summary>
        public static List<Vector3> Position = new List<Vector3>();

        static readonly string ShroomLocation = Config.AppDataDirectory + @"\PandaTeemo\";

        /// <summary>
        /// File Location for X
        /// </summary>
        static string xFile = ShroomLocation + Utility.Map.GetMap().Type + @"\" + "xFile" + ".txt";

        /// <summary>
        /// File Location for Y
        /// </summary>
        static string yFile = ShroomLocation + Utility.Map.GetMap().Type + @"\" + "yFile" + ".txt";

        /// <summary>
        /// File Location for Z
        /// </summary>
        static string zFile = ShroomLocation + Utility.Map.GetMap().Type + @"\" + "zFile" + ".txt";

        /// <summary>
        /// Array of X String
        /// </summary>
        static string[] xString = new string[Program.Config.SubMenu("Misc").Item("customLocationInt").GetValue<Slider>().Value];

        /// <summary>
        /// Array of Z String
        /// </summary>
        static string[] zString = new string[Program.Config.SubMenu("Misc").Item("customLocationInt").GetValue<Slider>().Value];

        /// <summary>
        /// Array of Y String
        /// </summary>
        static string[] yString = new string[Program.Config.SubMenu("Misc").Item("customLocationInt").GetValue<Slider>().Value];

        /// <summary>
        /// Array of X Int
        /// </summary>
        public static int[] XInt = new int[xString.Count()];

        /// <summary>
        /// Array of Z Int
        /// </summary>
        public static int[] ZInt = new int[zString.Count()];

        /// <summary>
        /// Array of Y Int
        /// </summary>
        public static int[] YInt = new int[yString.Count()];
        
        #endregion

        #region Methods

        /// <summary>
        /// Initialize the FileHandler
        /// </summary>
        public FileHandler()
        {
            #region Initialize
            DoChecks();
            #endregion
        }

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
                Directory.CreateDirectory(ShroomLocation + Utility.Map.MapType.Unknown);
                CreateFile();
            }

            else if (!File.Exists(xFile) || !File.Exists(zFile) || !File.Exists(yFile))
            {
                CreateFile();
            }

            else if (File.Exists(xFile) && File.Exists(zFile) && File.Exists(yFile))
            {
                ConvertToInt();
            }

            #endregion
        }

        /// <summary>
        /// Creates Files that are missing
        /// </summary>
        static void CreateFile()
        {
            #region Create File

            if (!File.Exists(xFile))
            {
                File.WriteAllText(xFile, "5020");
            }

            else if (!File.Exists(yFile))
            {
                File.WriteAllText(yFile, "8430");
            }

            else if (!File.Exists(zFile))
            {
                File.WriteAllText(zFile, "2");
            }

            DoChecks();

            #endregion
        }

        /// <summary>
        /// Gets the location of the shroom and adds it to the list
        /// </summary>
        public static void GetShroomLocation()
        {
            #region Get Location

            for (var i = 0; i < XInt.Count() && i < YInt.Count() && i < ZInt.Count(); i++)
            {
                Position.Add(new Vector3(XInt[i], ZInt[i], YInt[i]));
                if (Program.Config.SubMenu("Drawing").SubMenu("debug").Item("debugpos").GetValue<bool>())
                {
                    Game.PrintChat(Position[i].ToString());
                }
            }

            #endregion
        }

        /// <summary>
        /// Converts String to Int
        /// </summary>
        static void ConvertToInt()
        {
            #region Convert to Int

            xString = File.ReadAllLines(xFile);
            yString = File.ReadAllLines(yFile);
            zString = File.ReadAllLines(zFile);

            for (var i = 0; i < xString.Count(); i++)
            {
                XInt[i] = Convert.ToInt32(xString[i]);
            }

            for (var i = 0; i < xString.Count(); i++)
            {
                ZInt[i] = Convert.ToInt32(zString[i]);
            }

            for (var i = 0; i < xString.Count(); i++)
            {
                YInt[i] = Convert.ToInt32(yString[i]);
            }

            GetShroomLocation();

            if (Program.Config.SubMenu("Drawing").SubMenu("debug").Item("debugpos").GetValue<bool>())
            {
                Game.PrintChat("Sucessfully Initialized FileHandler");
            }

            #endregion
        }

        #endregion
    }
}