using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace BlackKassadin
{
    class Program
    {
        // Generic
        public static readonly string champName = "Kassadin";
        private static readonly Obj_AI_Hero player = ObjectManager.Player;

        // Spells
        private static readonly List<Spell> spellList = new List<Spell>();
        private static Spell Q, W, E, R;
        private static SpellSlot IgniteSlot;
        private static Items.Item DFG;
        
        // Menu
        public static Menu menu;
        
        private static Orbwalking.Orbwalker OW;

        public static void Main(string[] args)
        {
            // Register events
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            //Champ validation
            if (player.ChampionName != champName) return;

            //Define spells
            Q = new Spell(SpellSlot.Q, 650);
            W = new Spell(SpellSlot.W, 200);
            E = new Spell(SpellSlot.E, 700);
            R = new Spell(SpellSlot.R, 700);
            spellList.AddRange(new []{Q, W, E, R});

            IgniteSlot = player.GetSpellSlot("SummonerDot");

            DFG = Utility.Map.GetMap()._MapType == Utility.Map.MapType.TwistedTreeline ? new Items.Item(3188, 750) : new Items.Item(3128, 750);

            // Finetune spells
            Q.SetTargetted(0.5f, 1400f);
            E.SetSkillshot(0.5f, 10f, float.MaxValue, false, SkillshotType.SkillshotCone);
            R.SetSkillshot(0.5f, 150f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            // Create menu
            createMenu();

            // Register events
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;

            // Print
            Game.PrintChat(String.Format("<font color='#08F5F8'>blacky -</font> <font color='#FFFFFF'>{0} Loaded!</font>", champName));
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            // Spell ranges
            foreach (var spell in spellList)
            {
                // Regular spell ranges
                var circleEntry = menu.Item("drawRange" + spell.Slot).GetValue<Circle>();
                if (circleEntry.Active)
                    Utility.DrawCircle(player.Position, spell.Range, circleEntry.Color);
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            // Combo
            if (menu.SubMenu("combo").Item("comboActive").GetValue<KeyBind>().Active)
                OnCombo();

            // Harass
            if (menu.SubMenu("harass").Item("harassActive").GetValue<KeyBind>().Active && 
               (ObjectManager.Player.Mana / ObjectManager.Player.MaxMana * 100) >
                menu.Item("harassMana").GetValue<Slider>().Value)
                OnHarass();

            // Misc
            if (menu.SubMenu("misc").Item("miscUltToMouse").GetValue<KeyBind>().Active)
                UltToMouse();

        }

        private static void OnCombo()
        {
            Menu comboMenu = menu.SubMenu("combo");
            bool useQ = comboMenu.Item("comboUseQ").GetValue<bool>() && Q.IsReady();
            bool useW = comboMenu.Item("comboUseW").GetValue<bool>() && W.IsReady();
            bool useE = comboMenu.Item("comboUseE").GetValue<bool>() && E.IsReady();
            bool useR = comboMenu.Item("comboUseR").GetValue<bool>() && R.IsReady();

            var Target = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Magical);

            if (useR)
            {
                var target = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Magical);
                if (target != null)
                    R.Cast(target, packets());
            }

            if (useW)
            {
                var target = SimpleTs.GetTarget(W.Range, SimpleTs.DamageType.Magical);
                if (target != null)
                    W.Cast(target, packets());
            }

            if (useQ)
            {
                var target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
                if (target != null)
                    Q.Cast(target, packets());
            }

            if (useE)
            {
                var target = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Magical);
                if (target != null)
                    E.Cast(target, packets());
            }

            if (Target != null && menu.Item("miscIgnite").GetValue<bool>() && IgniteSlot != SpellSlot.Unknown &&
            player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
            {
                if (GetComboDamage(Target) > Target.Health)
                {
                    player.SummonerSpellbook.CastSpell(IgniteSlot, Target);
                }
            }
        }

        private static void OnHarass()
        {
            Menu harassMenu = menu.SubMenu("harass");
            bool useQ = harassMenu.Item("harassUseQ").GetValue<bool>() && Q.IsReady();
            bool useE = harassMenu.Item("harassUseE").GetValue<bool>() && E.IsReady();

            if (useQ)
            {
                var target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
                if (target != null)
                    Q.Cast(target, packets());
            }

            if (useE)
            {
                var target = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Magical);
                if (target != null)
                    E.Cast(target, packets());
            }
        }

        private static void UltToMouse()
        {
            Menu miscMenu = menu.SubMenu("misc");
            bool useR = miscMenu.Item("miscUseR").GetValue<bool>() && R.IsReady();

            if (useR)
            {
                Orbwalking.Orbwalk(null, Game.CursorPos);
                R.Cast(Game.CursorPos, packets());
            }

            else
            {
                Orbwalking.Orbwalk(null, Game.CursorPos);
            }
        }

        private static float GetComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;
            if (R.IsReady())
                damage += player.GetSpellDamage(enemy, SpellSlot.R);

            if (DFG.IsReady())
                damage += player.GetItemDamage(enemy, Damage.DamageItems.Dfg) / 1.2;

            if (W.IsReady())
                damage += player.GetSpellDamage(enemy, SpellSlot.W);

            if (Q.IsReady())
                damage += player.GetSpellDamage(enemy, SpellSlot.Q);

            if (E.IsReady())
                damage += player.GetSpellDamage(enemy, SpellSlot.E);

            if (IgniteSlot != SpellSlot.Unknown && player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
                damage += player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);

            return (float)damage * (DFG.IsReady() ? 1.2f : 1);
        }

        private static bool packets()
        {
            return menu.Item("miscPacket").GetValue<bool>();
        }

        private static void createMenu()
        {
            menu = new Menu("Black" + champName, "black" + champName, true);

            // Target selector
            Menu ts = new Menu("Target Selector", "ts");
            menu.AddSubMenu(ts);
            SimpleTs.AddToMenu(ts);

            // Orbwalker
            Menu orbwalk = new Menu("Orbwalking", "orbwalk");
            menu.AddSubMenu(orbwalk);
            OW = new Orbwalking.Orbwalker(orbwalk);

            // Combo
            Menu combo = new Menu("Combo", "combo");
            menu.AddSubMenu(combo);
            combo.AddItem(new MenuItem("comboUseQ", "Use Q").SetValue(true));
            combo.AddItem(new MenuItem("comboUseW", "Use W").SetValue(true));
            combo.AddItem(new MenuItem("comboUseE", "Use E").SetValue(true));
            combo.AddItem(new MenuItem("comboUseR", "Use R").SetValue(true));
            combo.AddItem(new MenuItem("comboActive", "Combo active!").SetValue(new KeyBind(32, KeyBindType.Press)));

            // Harass
            Menu harass = new Menu("Harass", "harass");
            menu.AddSubMenu(harass);
            harass.AddItem(new MenuItem("harassUseQ", "Use Q").SetValue(true));
            harass.AddItem(new MenuItem("harassUseE", "Use E").SetValue(false));
            harass.AddItem(new MenuItem("harassMana", "Mana To Harass").SetValue(new Slider(40, 100, 0)));
            harass.AddItem(new MenuItem("harassActive", "Harass active!").SetValue(new KeyBind('C', KeyBindType.Press)));

            // Misc
            Menu misc = new Menu("Misc", "misc");
            menu.AddSubMenu(misc);
            misc.AddItem(new MenuItem("miscPacket", "Use Packets").SetValue(true));
            misc.AddItem(new MenuItem("miscIgnite", "Use Ignite").SetValue(true));
            //misc.AddItem(new MenuItem("miscDFG", "Use DFG").SetValue(true));
            misc.AddItem(new MenuItem("miscUltToMouse", "Ult to mouse").SetValue(new KeyBind('G', KeyBindType.Press)));
            misc.AddItem(new MenuItem("miscUseR", "Use R in Ult to mouse").SetValue(true));


            //Damage after combo:
            var dmgAfterComboItem = new MenuItem("DamageAfterCombo", "Draw damage after combo").SetValue(true);
            Utility.HpBarDamageIndicator.DamageToUnit = GetComboDamage;
            Utility.HpBarDamageIndicator.Enabled = dmgAfterComboItem.GetValue<bool>();
            dmgAfterComboItem.ValueChanged += delegate(object sender, OnValueChangeEventArgs eventArgs)
            {
                Utility.HpBarDamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
            };

            // Drawings
            Menu drawings = new Menu("Drawings", "drawings");
            menu.AddSubMenu(drawings);
            drawings.AddItem(new MenuItem("drawRangeQ", "Q range").SetValue(new Circle(true, Color.Aquamarine)));
            drawings.AddItem(new MenuItem("drawRangeW", "W range").SetValue(new Circle(false, Color.Aquamarine)));
            drawings.AddItem(new MenuItem("drawRangeE", "E range").SetValue(new Circle(false, Color.Aquamarine)));
            drawings.AddItem(new MenuItem("drawRangeR", "R range").SetValue(new Circle(false, Color.Aquamarine)));
            drawings.AddItem(dmgAfterComboItem);

            // Finalizing
            menu.AddToMainMenu();
        }
    }
}

