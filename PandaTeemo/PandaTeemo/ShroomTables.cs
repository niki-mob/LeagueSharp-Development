﻿using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace PandaTeemo
{
    //The following code is taken from UC2's Teemo. All the shroom locations are modified in the recent update for better positioning.
    //To add a shroom location, press T and look at the console for the X Y Z positions and copy the template below to add your own location.

    // REWORKED BY KARMAPANDA

    /// <summary>
    /// Shroom Locations
    /// </summary>
    internal class ShroomTables
    {
        public List<Vector3> SummonersRift = new List<Vector3>();
        public List<Vector3> HowlingAbyss = new List<Vector3>();
        public List<Vector3> CrystalScar = new List<Vector3>();
        public List<Vector3> TwistedTreeline = new List<Vector3>();

        public ShroomTables()
        {
            CreateTables();
            var list = (from pos in SummonersRift
                        let x = pos.X
                        let y = pos.Y
                        let z = pos.Z
                        select new Vector3(x, z, y)).ToList();
            SummonersRift = list;

            list = (from pos in HowlingAbyss
                        let x = pos.X
                        let y = pos.Y
                        let z = pos.Z
                        select new Vector3(x, z, y)).ToList();
            HowlingAbyss = list;

            list = (from pos in CrystalScar
                        let x = pos.X
                        let y = pos.Y
                        let z = pos.Z
                        select new Vector3(x, z, y)).ToList();
            CrystalScar = list;

            list = (from pos in TwistedTreeline
                        let x = pos.X
                        let y = pos.Y
                        let z = pos.Z
                        select new Vector3(x, z, y)).ToList();
            TwistedTreeline = list;
        }
        private void CreateTables()
        {
            /// Summoner's Rift

            //Top Lane Blue Side including Baron
            SummonersRift.Add(new Vector3(2790f, 50.16358f, 7278f));
            SummonersRift.Add(new Vector3(3700.708f, -11.22648f, 9294.094f));
            SummonersRift.Add(new Vector3(2314f, 53.165f, 9722f));
            SummonersRift.Add(new Vector3(3090f, -68.03732f, 10810f));
            SummonersRift.Add(new Vector3(4722f, -71.2406f, 10010f));
            SummonersRift.Add(new Vector3(5208f, -71.2406f, 9114f));
            SummonersRift.Add(new Vector3(4724f, 52.53909f, 7590f));
            SummonersRift.Add(new Vector3(4564f, 51.83786f, 6060f));
            SummonersRift.Add(new Vector3(2760f, 52.96445f, 5178f));
            SummonersRift.Add(new Vector3(4440f, 56.8484f, 11840f));

            //Top Lane Tri Bush
            SummonersRift.Add(new Vector3(2420f, 52.8381f, 13482f));
            SummonersRift.Add(new Vector3(1630f, 52.8381f, 13008f));
            SummonersRift.Add(new Vector3(1172f, 52.8381f, 12302f));

            //Top Lane Red Side
            SummonersRift.Add(new Vector3(5666f, 52.8381f, 12722f));
            SummonersRift.Add(new Vector3(8004f, 56.4768f, 11782f));
            SummonersRift.Add(new Vector3(9194f, 53.35013f, 11368f));
            SummonersRift.Add(new Vector3(8280f, 50.06194f, 10254f));
            SummonersRift.Add(new Vector3(6728f, 53.82967f, 11450f));
            SummonersRift.Add(new Vector3(6242f, 54.09851f, 10270f));

            //Mid Lane
            SummonersRift.Add(new Vector3(6484f, -71.2406f, 8380f));
            SummonersRift.Add(new Vector3(8380f, -71.2406f, 6502f));
            SummonersRift.Add(new Vector3(9099.75f, 52.95337f, 7376.637f));
            SummonersRift.Add(new Vector3(7376f, 52.8726f, 8802f));
            SummonersRift.Add(new Vector3(7602f, 52.56985f, 5928f));

            // Dragon
            SummonersRift.Add(new Vector3(9372f, -71.2406f, 5674f));
            SummonersRift.Add(new Vector3(10148f, -71.2406f, 4801.525f));
            
            //Bot Lane Red Side
            SummonersRift.Add(new Vector3(9772f, 9.031885f, 6458f));
            SummonersRift.Add(new Vector3(9938f, 51.62378f, 7900f));
            SummonersRift.Add(new Vector3(11465f, 51.72557f, 7157.772f));
            SummonersRift.Add(new Vector3(12481f, 51.7294f, 5232.559f));
            SummonersRift.Add(new Vector3(11266f, -7.897567f, 5542f));
            SummonersRift.Add(new Vector3(11290f, 64.39886f, 8694f));
            SummonersRift.Add(new Vector3(12676f, 51.6851f, 7310.818f));
            SummonersRift.Add(new Vector3(12022f, 9154f, 51.25105f));


            //Bot Lane Blue Side (Bushes only)
            SummonersRift.Add(new Vector3(6544f, 48.257f, 4732f));
            SummonersRift.Add(new Vector3(5576f, 51.42581f, 3512f));
            SummonersRift.Add(new Vector3(6888f, 51.94016f, 3082f));
            SummonersRift.Add(new Vector3(8070f, 51.5508f, 3472f));
            SummonersRift.Add(new Vector3(8594f, 51.73177f, 4668f));
            SummonersRift.Add(new Vector3(10388f, 49.81641f, 3046f));
            SummonersRift.Add(new Vector3(9160f, 59.97022f, 2122f));

            /// Howling Abyss

            

            // Template
            // MAP NAME.Add(new Vector3("X"f, "Z"f, "Y"f));
        }
    }
}
