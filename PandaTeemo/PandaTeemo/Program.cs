using LeagueSharp;
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
        public const string ChampionName = "Teemo";

        //Spells
        public static Spell Q;
        public static Spell W;
        public static Spell R;
        public static ShroomTables ShroomPositions;
        
        //Orbwalker
        public static Orbwalking.Orbwalker Orbwalker;

        //Menu
        public static Menu Config;

        //Player
        private static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }

        public static bool Packets
        {
            get { return Config.SubMenu("Misc").Item("packets").GetValue<bool>(); }
        }

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.BaseSkinName != ChampionName)
            {
                return;
            }

            //Spells
            Q = new Spell(SpellSlot.Q, 580);
            W = new Spell(SpellSlot.W);
            R = new Spell(SpellSlot.R, 230);

            R.SetSkillshot(0.1f, 75f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            //Menu
            Config = new Menu("PandaTeemo", "PandaTeemo", true);

            //TargetSelector
            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            //OrbWalker SubMenu
            var orbwalking = Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            var combo = Config.AddSubMenu(new Menu("Combo", "Combo"));
            var harass = Config.AddSubMenu(new Menu("Harass", "Harass"));
            var laneclear = Config.AddSubMenu(new Menu("LaneClear", "LaneClear"));
            var misc = Config.AddSubMenu(new Menu("Misc", "Misc"));

            //Combo Menu
            Config.SubMenu("Combo").AddItem(new MenuItem("qcombo", "Use Q in Combo").SetValue(true));
            combo.AddItem(new MenuItem("wcombo", "Use W in Combo").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("rcombo", "Kite with R in Combo").SetValue(false));

            //Harass Menu
            harass.AddItem(new MenuItem("qharass", "Harass with Q").SetValue(true));

            //LaneClear Menu
            laneclear.AddItem(new MenuItem("qclear", "LaneClear with Q").SetValue(false));
            laneclear.AddItem(new MenuItem("rclear", "LaneClear with R").SetValue(true));

            //Main Menu
            Orbwalker = new Orbwalking.Orbwalker(orbwalking);
            Config.AddToMainMenu();
            Config.AddItem(new MenuItem("autoQ", "Automatic Q").SetValue(false));
            Config.AddItem(new MenuItem("autoW", "Automatic W").SetValue(false));

            //Interrupter
            var interrupt = Config.AddSubMenu(new Menu("Interrupt", "Interrupt"));
            interrupt.AddItem(new MenuItem("intq", "Interrupt with Q").SetValue(true));

            //Misc
            misc.AddItem(new MenuItem("packets", "Use Packets").SetValue(false));
            misc.AddItem(new MenuItem("autoR", "Auto Place Shrooms in Important Places").SetValue(true));

            //KS Menu
            var ks = Config.AddSubMenu(new Menu("KSMenu", "KSMenu"));
            ks.AddItem(new MenuItem("KSQ", "KillSteal with Q").SetValue(true));

            //Drawing Menu
            var drawing = Config.AddSubMenu(new Menu("Drawing", "Drawing"));
            drawing.AddItem(new MenuItem("drawQ", "Draw Q Range").SetValue(true));
            drawing.AddItem(new MenuItem("drawR", "Draw R Range").SetValue(true));
            drawing.AddItem(new MenuItem("drawautoR", "Draw Important Shroom Areas").SetValue(true));
            drawing.AddItem(new MenuItem("DrawVision", "Shroom Vision").SetValue(new Slider(1500, 2500, 1000)));

            //Events
            ShroomPositions = new ShroomTables();
            Game.OnGameUpdate += Game_OnGameUpdate;
            Interrupter2.OnInterruptableTarget += Interrupter_OnPossibleToInterrupt;
            Drawing.OnDraw += DrawingOnOnDraw;
            Game.PrintChat("<font color=\"#FF0000\"><b>PandaTeemo RELEASE by KarmaPanda</b></font>");
        }

        #region IsShroom

        private static bool IsShroomed(Vector3 position)
        {
            return ObjectManager.Get<Obj_AI_Base>().Where(obj => obj.Name == "Noxious Trap").Any(obj => position.Distance(obj.Position) <= 250);
        }

        #endregion

        #region Combo

        public static void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (!target.IsValidTarget())
            {
                return;
            }

            var useQ = Config.SubMenu("Combo").Item("qcombo").GetValue<bool>();
            var useW = Config.SubMenu("Combo").Item("wcombo").GetValue<bool>();
            var useR = Config.SubMenu("Combo").Item("rcombo").GetValue<bool>();


            if (Q.IsReady() && useQ)
            {
                Q.Cast(target, Packets);
            }

            if (W.IsReady() && useW)
            {
                W.Cast(true);
            }

            if (R.IsReady() && useR && R.IsInRange(target))
            {
                R.Cast(target.Position, Packets);
            }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                if (target.IsValidTarget())
                {
                    Orbwalking.Attack = true;
                }
                else
                {
                    Orbwalking.Attack = false;
                }
            }
            else
            {
                Orbwalking.Attack = true;
            }
        }

        #endregion

        #region KillSteal
        public static void KSQ()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (target == null) return;

            if(Q.IsReady())
            {
                if(target.Health < Q.GetDamage(target))
                {
                    Q.Cast(target, Packets);
                }
            }
        }
        #endregion

        #region Harass

        public static void Harass()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var useQ = Config.SubMenu("Harass").Item("qharass").GetValue<bool>();

            if (!Q.IsReady() || !useQ)
            {
                return;
            }

            if (target.IsValidTarget())
            {
                Q.Cast(target, Packets);
            }
        }

        #endregion

        #region LaneClear

        public static void LaneClear()
        {
            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range);
            var rangedMinions = MinionManager.GetMinions(
                ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.Ranged);
            var rLocation = R.GetCircularFarmLocation(allMinions, R.Range);
            var r2Location = R.GetCircularFarmLocation(rangedMinions, R.Range);
            var useQ = Config.SubMenu("LaneClear").Item("qclear").GetValue<bool>();
            var useR = Config.SubMenu("LaneClear").Item("rclear").GetValue<bool>();
            var bestLocation = (rLocation.MinionsHit > r2Location.MinionsHit + 1) ? rLocation : r2Location;

            if (allMinions.Count > 0 & useQ)
            {
                if (allMinions[0].Health < ObjectManager.Player.GetSpellDamage(allMinions[0], SpellSlot.Q) &&
                    Q.IsReady())
                {
                    Q.CastOnUnit(allMinions[0], Packets);
                }
            }

            if (!(allMinions.Count > 0 & useR))
            {
                return;
            }

            if (allMinions[0].Health < ObjectManager.Player.GetSpellDamage(allMinions[0], SpellSlot.R) && R.IsReady() && R.IsInRange(r2Location.Position.To3D()))
            {
                R.Cast(bestLocation.Position, true);
            }
            else if (allMinions[0].Health < ObjectManager.Player.GetSpellDamage(allMinions[0], SpellSlot.R) &&
                     R.IsReady() && R.IsInRange(rLocation.Position.To3D()))
            {
                R.Cast(bestLocation.Position, true);
            }
        }

        #endregion

        private static void Interrupter_OnPossibleToInterrupt(Obj_AI_Hero sender,
            Interrupter2.InterruptableTargetEventArgs args)
        {
            var intq = Config.SubMenu("Interrupt").Item("intq").GetValue<bool>();

            if (intq & Q.IsReady() || args.DangerLevel != Interrupter2.DangerLevel.High)
            {
                Q.Cast(sender, Packets);
            }
        }

        #region AutoShroom

        private static void AutoShroom()
        {
            if (!R.IsReady())
                return;
            if (Config.SubMenu("Misc").Item("autoR").GetValue<bool>())
                foreach (var place in ShroomPositions.HighPriority.Where(pos => pos.Distance(ObjectManager.Player.Position) <= R.Range && !IsShroomed(pos)))
                    R.Cast(place, Packets);
        }

        #endregion
        private static void Game_OnGameUpdate(EventArgs args)
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var autoQ = Config.Item("autoQ").GetValue<bool>();
            var autoW = Config.Item("autoW").GetValue<bool>();

            //Auto Q and W
            if (W.IsReady() && autoW)
            {
                W.Cast(true);
            }

            if (Q.IsReady() && autoQ)
            {
                Q.Cast(target, true, Packets);
            }
            
            //Auto Shroom
            AutoShroom();

            //Orbwalker
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                Combo();
            }
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                Harass();
            }
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                LaneClear();
            }
            //KillSteal
            if (Config.SubMenu("KSMenu").Item("KSQ").GetValue<bool>())
            {
                KSQ();
            }
        }

        private static void DrawingOnOnDraw(EventArgs args)
        {
            var drawQ = Config.SubMenu("Drawing").Item("drawQ").GetValue<bool>();
            var drawR = Config.SubMenu("Drawing").Item("drawR").GetValue<bool>();
            var drawautoR = Config.SubMenu("Drawing").Item("drawautoR").GetValue<bool>();

            var player = ObjectManager.Player.Position;

            if (drawQ)
            {
                Render.Circle.DrawCircle(player, Q.Range, Q.IsReady() ? System.Drawing.Color.Gold : System.Drawing.Color.Green);
            }
            if (drawR)
            {
                Render.Circle.DrawCircle(player, R.Range, R.IsReady() ? System.Drawing.Color.Gold : System.Drawing.Color.Green);
            }
            if (drawautoR)
                foreach (var place in ShroomPositions.HighPriority.Where(pos => pos.Distance(ObjectManager.Player.Position) <= Config.SubMenu("Drawing").Item("DrawVision").GetValue<Slider>().Value))
                {
                    Render.Circle.DrawCircle(place, 100, System.Drawing.Color.Red);
                }
        }
        internal class ShroomTables
        {
            public List<Vector3> HighPriority = new List<Vector3>();

            public ShroomTables()
            {
                CreateTables();
                var list = (from pos in HighPriority let x = pos.X let y = pos.Y let z = pos.Z select new Vector3(x, z, y)).ToList();

                HighPriority = list;
            }
            private void CreateTables()
            {
                HighPriority.Add(new Vector3(921.46795654297f, 39.889404296875f, 12422.21484375f));
                HighPriority.Add(new Vector3(1499.1662597656f, 34.766235351563f, 12988.01953125f));
                HighPriority.Add(new Vector3(2298.3325195313f, 30.003173828125f, 13440.301757813f));
                HighPriority.Add(new Vector3(2713.0063476563f, -63.90966796875f, 10630.198242188f));
                HighPriority.Add(new Vector3(2515.1171875f, -64.839721679688f, 11122.674804688f));
                HighPriority.Add(new Vector3(2975.3854980469f, -62.576782226563f, 10700.487304688f));
                HighPriority.Add(new Vector3(3244.3505859375f, -62.547485351563f, 10755.734375f));
                HighPriority.Add(new Vector3(3994.9416503906f, 48.449096679688f, 11596.635742188f));
                HighPriority.Add(new Vector3(4139.32421875f, -61.858642578125f, 9903.69921875f));
                HighPriority.Add(new Vector3(4348.1538085938f, -61.60302734375f, 9768.2900390625f));
                HighPriority.Add(new Vector3(4761.8310546875f, -63.09326171875f, 9862.34765625f));
                HighPriority.Add(new Vector3(4171.7451171875f, -63.068359375f, 10351.319335938f));
                HighPriority.Add(new Vector3(3281.0983886719f, -55.713745117188f, 9349.4912109375f));
                HighPriority.Add(new Vector3(3153.3203125f, 36.545166015625f, 8962.8525390625f));
                HighPriority.Add(new Vector3(1891.9038085938f, 54.14990234375f, 9499.126953125f));
                HighPriority.Add(new Vector3(2850.5981445313f, 55.041748046875f, 7639.6494140625f));
                HighPriority.Add(new Vector3(2502.4973144531f, 55.001098632813f, 7341.9370117188f));
                HighPriority.Add(new Vector3(2602.5441894531f, 55.002197265625f, 7067.4145507813f));
                HighPriority.Add(new Vector3(2214.361328125f, 52.984985351563f, 7049.8295898438f));
                HighPriority.Add(new Vector3(2498.62109375f, 55.916137695313f, 5075.2387695313f));
                HighPriority.Add(new Vector3(2083.3215332031f, 56.314208984375f, 5145.0546875f));
                HighPriority.Add(new Vector3(2888.1394042969f, 54.720703125f, 6477.1748046875f));
                HighPriority.Add(new Vector3(3913.8659667969f, 55.58984375f, 5810.2133789063f));
                HighPriority.Add(new Vector3(4137.921875f, 53.974365234375f, 6055.1010742188f));
                HighPriority.Add(new Vector3(4325.3876953125f, 54.167114257813f, 6318.9189453125f));
                HighPriority.Add(new Vector3(4363.681640625f, 54.954467773438f, 5830.9155273438f));
                HighPriority.Add(new Vector3(4373.7373046875f, 53.943603515625f, 6877.822265625f));
                HighPriority.Add(new Vector3(4439.8911132813f, 52.956787109375f, 7505.7993164063f));
                HighPriority.Add(new Vector3(4292.5170898438f, 52.443237304688f, 7800.5454101563f));
                HighPriority.Add(new Vector3(4481.2734375f, 31.203369140625f, 8167.3994140625f));
                HighPriority.Add(new Vector3(4702.11328125f, -39.108154296875f, 8327.1357421875f));
                HighPriority.Add(new Vector3(4840.8349609375f, -63.086181640625f, 8923.365234375f));
                HighPriority.Add(new Vector3(409.01028442383f, 47.910278320313f, 7925.912109375f));
                HighPriority.Add(new Vector3(5320.7036132813f, 39.920776367188f, 12485.568359375f));
                HighPriority.Add(new Vector3(6374.5859375f, 41.244140625f, 12730.94921875f));
                HighPriority.Add(new Vector3(6752.7124023438f, 44.903564453125f, 13844.407226563f));
                HighPriority.Add(new Vector3(7362.7646484375f, 51.478881835938f, 11621.13671875f));
                HighPriority.Add(new Vector3(7856.8012695313f, 49.970092773438f, 11622.293945313f));
                HighPriority.Add(new Vector3(6613.2329101563f, 54.534423828125f, 11136.745117188f));
                HighPriority.Add(new Vector3(6039.8583984375f, 54.31103515625f, 11115.771484375f));
                HighPriority.Add(new Vector3(7863.0927734375f, 53.14599609375f, 10073.770507813f));
                HighPriority.Add(new Vector3(5677.591796875f, -63.449462890625f, 9178.7578125f));
                HighPriority.Add(new Vector3(5907.1650390625f, -53.438598632813f, 8993.447265625f));
                HighPriority.Add(new Vector3(5873.8608398438f, 53.8046875f, 9841.0068359375f));
                HighPriority.Add(new Vector3(5747.2626953125f, 53.452880859375f, 10273.5546875f));
                HighPriority.Add(new Vector3(6823.5209960938f, 56.019165039063f, 8457.9013671875f));
                HighPriority.Add(new Vector3(7046.1997070313f, 56.019287109375f, 8671.56640625f));
                HighPriority.Add(new Vector3(6169.935546875f, -59.301391601563f, 8112.6264648438f));
                HighPriority.Add(new Vector3(4913.3471679688f, 54.542114257813f, 7416.4116210938f));
                HighPriority.Add(new Vector3(5156.5249023438f, 54.801025390625f, 7447.9301757813f));
                HighPriority.Add(new Vector3(7089.7197265625f, 55.59765625f, 5860.263671875f));
                HighPriority.Add(new Vector3(7146.5126953125f, 55.838500976563f, 5562.869140625f));
                HighPriority.Add(new Vector3(8042.4702148438f, -64.220581054688f, 6240.8950195313f));
                HighPriority.Add(new Vector3(9466.8701171875f, 21.286254882813f, 6207.2763671875f));
                HighPriority.Add(new Vector3(9031.029296875f, -63.957397460938f, 5445.94140625f));
                HighPriority.Add(new Vector3(9615.724609375f, -61.227905273438f, 4683.6630859375f));
                HighPriority.Add(new Vector3(9813.8505859375f, -60.410400390625f, 4538.2255859375f));
                HighPriority.Add(new Vector3(10124.901367188f, -61.733642578125f, 4861.4521484375f));
                HighPriority.Add(new Vector3(10776.483398438f, -13.516723632813f, 5262.2309570313f));
                HighPriority.Add(new Vector3(8217.439453125f, -62.027709960938f, 5401.1396484375f));
                HighPriority.Add(new Vector3(10461.926757813f, -64.395629882813f, 4236.0048828125f));
                HighPriority.Add(new Vector3(10691.946289063f, -64.254760742188f, 4401.0180664063f));
                HighPriority.Add(new Vector3(10954.478515625f, -63.434204101563f, 4601.060546875f));
                HighPriority.Add(new Vector3(11357.145507813f, -54.605102539063f, 3769.7680664063f));
                HighPriority.Add(new Vector3(9915.576171875f, 52.202392578125f, 2978.8586425781f));
                HighPriority.Add(new Vector3(9895.92578125f, 52.180786132813f, 2750.76171875f));
                HighPriority.Add(new Vector3(10120.110351563f, 53.538818359375f, 2853.455078125f));
                HighPriority.Add(new Vector3(10525.716796875f, -36.560668945313f, 3298.8117675781f));
                HighPriority.Add(new Vector3(7445.8657226563f, 55.46875f, 3257.5419921875f));
                HighPriority.Add(new Vector3(7904.7578125f, 56.58203125f, 3296.7998046875f));
                HighPriority.Add(new Vector3(8109.4311523438f, 55.375610351563f, 4634.1088867188f));
                HighPriority.Add(new Vector3(8272.7421875f, 55.947509765625f, 4302.23046875f));
                HighPriority.Add(new Vector3(6131.2407226563f, 51.67333984375f, 4458.34765625f));
                HighPriority.Add(new Vector3(5557.1635742188f, 53.145385742188f, 4852.1484375f));
                HighPriority.Add(new Vector3(5011.01171875f, 54.343505859375f, 3113.8823242188f));
                HighPriority.Add(new Vector3(5376.021484375f, 54.53125f, 3348.4313964844f));
                HighPriority.Add(new Vector3(6201.3012695313f, 53.259399414063f, 2892.7741699219f));
                HighPriority.Add(new Vector3(6689.6108398438f, 55.71728515625f, 2811.2314453125f));
                HighPriority.Add(new Vector3(5670.3720703125f, 55.274536132813f, 1833.3752441406f));
                HighPriority.Add(new Vector3(7637.787109375f, 53.322875976563f, 1630.271484375f));
                HighPriority.Add(new Vector3(7356.54296875f, 54.28955078125f, 2027.0147705078f));
                HighPriority.Add(new Vector3(7064.2900390625f, 55.656616210938f, 2463.6518554688f));
                HighPriority.Add(new Vector3(6560.2646484375f, 51.673461914063f, 4359.892578125f));
                HighPriority.Add(new Vector3(7199.0581054688f, 51.67041015625f, 4900.568359375f));
                HighPriority.Add(new Vector3(8820.9375f, 63.57958984375f, 1903.0247802734f));
                HighPriority.Add(new Vector3(11640.668945313f, 48.783569335938f, 1058.3022460938f));
                HighPriority.Add(new Vector3(12412.471679688f, 48.783569335938f, 1640.6274414063f));
                HighPriority.Add(new Vector3(12063.520507813f, 48.783569335938f, 1359.0847167969f));
                HighPriority.Add(new Vector3(12719.424804688f, 48.783447265625f, 1965.8095703125f));
                HighPriority.Add(new Vector3(13265.239257813f, 48.783569335938f, 2848.7946777344f));
                HighPriority.Add(new Vector3(12924.999023438f, 48.78369140625f, 2265.1831054688f));
                HighPriority.Add(new Vector3(12005.322265625f, 48.927612304688f, 4917.8154296875f));
                HighPriority.Add(new Vector3(12195.315429688f, 54.20849609375f, 4809.0424804688f));
                HighPriority.Add(new Vector3(12189.844726563f, 52.148803710938f, 5133.8681640625f));
                HighPriority.Add(new Vector3(11535.673828125f, 54.859985351563f, 6743.541015625f));
                HighPriority.Add(new Vector3(11195.842773438f, 54.87353515625f, 6849.5458984375f));
                HighPriority.Add(new Vector3(10161.522460938f, 54.838256835938f, 7404.8989257813f));
                HighPriority.Add(new Vector3(10823.671875f, 55.360961914063f, 7471.75390625f));
                HighPriority.Add(new Vector3(9550.7607421875f, 54.681518554688f, 7851.2338867188f));
                HighPriority.Add(new Vector3(9609.2841796875f, 53.629760742188f, 8623.1123046875f));
                HighPriority.Add(new Vector3(9738.8759765625f, 48.228637695313f, 6176.4951171875f));
                HighPriority.Add(new Vector3(8515.677734375f, 55.524291992188f, 7274.1328125f));
                HighPriority.Add(new Vector3(9203.1376953125f, 55.31787109375f, 6883.65625f));
                HighPriority.Add(new Vector3(11144.715820313f, 58.249267578125f, 8004.6669921875f));
                HighPriority.Add(new Vector3(11748.110351563f, 55.689575195313f, 7680.4853515625f));
                HighPriority.Add(new Vector3(11933.775390625f, 55.45849609375f, 8171.1162109375f));
                HighPriority.Add(new Vector3(11663.83984375f, 53.506958007813f, 8618.32421875f));
                HighPriority.Add(new Vector3(11783.155273438f, 50.942749023438f, 9116.0087890625f));
                HighPriority.Add(new Vector3(11355.23046875f, 50.350463867188f, 9551.4541015625f));
                HighPriority.Add(new Vector3(12118.708984375f, 54.836669921875f, 7175.52734375f));
                HighPriority.Add(new Vector3(11636.209960938f, 55.298583984375f, 7139.6665039063f));
                HighPriority.Add(new Vector3(12379.048828125f, 50.354858398438f, 9417.357421875f));
                HighPriority.Add(new Vector3(10719.4453125f, 50.348754882813f, 9761.5263671875f));
                HighPriority.Add(new Vector3(9533.9970703125f, 52.488647460938f, 10893.603515625f));
                HighPriority.Add(new Vector3(9010.201171875f, 54.606811523438f, 11232.607421875f));
                HighPriority.Add(new Vector3(8605.5283203125f, 51.6875f, 11134.741210938f));
                HighPriority.Add(new Vector3(8229.3330078125f, 53.65283203125f, 10201.456054688f));
                HighPriority.Add(new Vector3(8200.6875f, 53.530517578125f, 9692.642578125f));
                HighPriority.Add(new Vector3(9170.328125f, 51.405151367188f, 12593.346679688f));
                HighPriority.Add(new Vector3(9200.1787109375f, 52.487060546875f, 11952.364257813f));
                HighPriority.Add(new Vector3(9394.4716796875f, 52.48291015625f, 11301.334960938f));
                HighPriority.Add(new Vector3(13643.55859375f, 53.597534179688f, 6827.8857421875f));
                HighPriority.Add(new Vector3(7424.7919921875f, 52.602905273438f, 636.64184570313f));
                HighPriority.Add(new Vector3(4845.21484375f, 54.945678710938f, 1713.1027832031f));
                HighPriority.Add(new Vector3(4794.412109375f, 54.408081054688f, 2377.0554199219f));
                HighPriority.Add(new Vector3(4687.0810546875f, 54.071655273438f, 3056.8149414063f));
                HighPriority.Add(new Vector3(4472.8857421875f, 53.9248046875f, 3690.6577148438f));
                HighPriority.Add(new Vector3(3267.3359375f, 56.665649414063f, 4736.8735351563f));
                HighPriority.Add(new Vector3(1490.9304199219f, 57.458129882813f, 5063.8056640625f));
                HighPriority.Add(new Vector3(11211.5390625f, 55.709228515625f, 5120.58984375f));
                HighPriority.Add(new Vector3(11028.774414063f, 54.829711914063f, 6133.4228515625f));
                HighPriority.Add(new Vector3(10690.204101563f, 54.790649414063f, 6184.8364257813f));
                HighPriority.Add(new Vector3(11353.065429688f, -61.857788085938f, 4208.9169921875f));
                HighPriority.Add(new Vector3(11054.20703125f, -63.471801757813f, 4034.0634765625f));
                HighPriority.Add(new Vector3(10729.1875f, -64.608764648438f, 3886.7646484375f));
                HighPriority.Add(new Vector3(5222.2001953125f, -65.250732421875f, 9170.2705078125f));
                HighPriority.Add(new Vector3(5256.4858398438f, -64.5146484375f, 8881.8447265625f));
                HighPriority.Add(new Vector3(5374.3110351563f, -63.622436523438f, 8581.1083984375f));
                HighPriority.Add(new Vector3(5544.1318359375f, -60.822387695313f, 8338.818359375f));
                HighPriority.Add(new Vector3(5856.494140625f, -59.145751953125f, 8261.908203125f));
                HighPriority.Add(new Vector3(5315.2211914063f, 54.801147460938f, 7194.9370117188f));
                HighPriority.Add(new Vector3(3382.2592773438f, 31.458251953125f, 12508.072265625f));
                HighPriority.Add(new Vector3(3709.2463378906f, 37.916381835938f, 12512.922851563f));
                HighPriority.Add(new Vector3(5245.1381835938f, 47.917236328125f, 11228.446289063f));
                HighPriority.Add(new Vector3(7275.896484375f, 53.921508789063f, 11008.899414063f));
                HighPriority.Add(new Vector3(7651.9858398438f, 52.825317382813f, 10368.020507813f));
                HighPriority.Add(new Vector3(7562.1123046875f, 53.961547851563f, 9783.9619140625f));
                HighPriority.Add(new Vector3(6124.6577148438f, 55.156127929688f, 9741.4228515625f));
                HighPriority.Add(new Vector3(5846.0161132813f, 48.514282226563f, 9410.921875f));
                HighPriority.Add(new Vector3(4148.1767578125f, -60.990478515625f, 9254.2666015625f));
                HighPriority.Add(new Vector3(3920.0073242188f, -60.146850585938f, 9398.7421875f));
                HighPriority.Add(new Vector3(10604.783203125f, -63.342529296875f, 4872.3872070313f));
                HighPriority.Add(new Vector3(13224.2421875f, 105.00170898438f, 10037.708007813f));
                HighPriority.Add(new Vector3(10424.015625f, 106.93432617188f, 10617.844726563f));
                HighPriority.Add(new Vector3(9731.544921875f, 106.16320800781f, 13427.314453125f));
                HighPriority.Add(new Vector3(813.72741699219f, 123.41027832031f, 4628.5424804688f));
                HighPriority.Add(new Vector3(3681.4140625f, 124.169921875f, 3953.0578613281f));
                HighPriority.Add(new Vector3(4373.1611328125f, 112.74145507813f, 1091.2574462891f));
            }
        }
    }
}
