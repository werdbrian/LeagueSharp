using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace BlackKassadin
{
    internal class Program
    {
        // Generic
        public static readonly string ChampName = "Kassadin";
        private static readonly Obj_AI_Hero Player = ObjectManager.Player;
        // Spells
        private static readonly List<Spell> SpellList = new List<Spell>();
        private static Spell _q, _w, _e, _r;
        private static SpellSlot _igniteSlot;
        private static Items.Item _dfg;
        // Menu
        public static Menu Menu;
        private static Orbwalking.Orbwalker _ow;

        public static void Main(string[] args)
        {
            // Register events
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            //Champ validation
            if (Player.ChampionName != ChampName)
            {
                return;
            }

            //Define spells
            _q = new Spell(SpellSlot.Q, 650);
            _w = new Spell(SpellSlot.W, 150);
            _e = new Spell(SpellSlot.E, 650);
            _r = new Spell(SpellSlot.R, 700);
            SpellList.AddRange(new[] {_q, _w, _e, _r});

            _igniteSlot = Player.GetSpellSlot("SummonerDot");

            _dfg = Utility.Map.GetMap().Type == Utility.Map.MapType.TwistedTreeline ||
                   Utility.Map.GetMap().Type == Utility.Map.MapType.CrystalScar
                ? new Items.Item(3188, 750)
                : new Items.Item(3128, 750);

            // Finetune spells
            _q.SetTargetted(0.5f, 1400f);
            _e.SetSkillshot(0.5f, 10f, float.MaxValue, false, SkillshotType.SkillshotCone);
            _r.SetSkillshot(0.5f, 150f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            // Create menu
            CreateMenu();

            // Register events
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;

            // Print
            Game.PrintChat(
                String.Format("<font color='#08F5F8'>blacky -</font> <font color='#FFFFFF'>{0} Loaded!</font>",
                    ChampName));
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            // Spell ranges
            foreach (var spell in SpellList)
            {
                // Regular spell ranges
                var circleEntry = Menu.Item("drawRange" + spell.Slot).GetValue<Circle>();
                if (circleEntry.Active)
                {
                    Utility.DrawCircle(Player.Position, spell.Range, circleEntry.Color);
                }
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            var target = TargetSelector.GetTarget(_r.Range, TargetSelector.DamageType.Magical);

            // Combo
            if (Menu.SubMenu("combo").Item("comboActive").GetValue<KeyBind>().Active)
            {
                OnCombo(target);
            }

            // Harass
            if (Menu.SubMenu("harass").Item("harassActive").GetValue<KeyBind>().Active &&
                (Player.Mana/Player.MaxMana*100) >
                Menu.Item("harassMana").GetValue<Slider>().Value)
            {
                OnHarass(target);
            }

            // WaveClear
            if (Menu.SubMenu("waveclear").Item("wcActive").GetValue<KeyBind>().Active &&
                (Player.Mana/Player.MaxMana*100) >
                Menu.Item("wcMana").GetValue<Slider>().Value)
            {
                WaveClear();
            }

            // Misc
            if (Menu.SubMenu("misc").Item("miscUltToMouse").GetValue<KeyBind>().Active)
            {
                UltToMouse();
            }

            // Killsteal
            Killsteal(target);
        }

        private static void OnCombo(Obj_AI_Hero target)
        {
            var comboMenu = Menu.SubMenu("combo");
            var useQ = comboMenu.Item("comboUseQ").GetValue<bool>() && _q.IsReady();
            var useW = comboMenu.Item("comboUseW").GetValue<bool>() && _w.IsReady();
            var useE = comboMenu.Item("comboUseE").GetValue<bool>() && _e.IsReady();
            var useR = comboMenu.Item("comboUseR").GetValue<bool>() && _r.IsReady();

            var comboDamage = target != null ? GetComboDamage(target) : 0;

            if (target != null && target.HasBuffOfType(BuffType.Invulnerability))
            {
                return;
            }

            if (target != null && comboDamage > target.Health && _dfg.IsReady())
            {
                _dfg.Cast(target);
            }

            if (target != null && (useR && Player.Distance(target.Position) < _r.Range))
            {
                _r.Cast(target, Packets());
            }

            if (useW)
            {
                _w.Cast(Player, Packets());
            }

            if (target != null && (useQ && Player.Distance(target.Position) < _q.Range))
            {
                _q.Cast(target, Packets());
            }

            if (target != null && (useE && Player.Distance(target.Position) < _e.Range))
            {
                _e.Cast(target, Packets());
            }

            if (target == null || !Menu.Item("miscIgnite").GetValue<bool>() || _igniteSlot == SpellSlot.Unknown ||
                Player.Spellbook.CanUseSpell(_igniteSlot) != SpellState.Ready)
            {
                return;
            }

            if (GetComboDamage(target) > target.Health)
            {
                Player.Spellbook.CastSpell(_igniteSlot, target);
            }
        }

        private static void OnHarass(Obj_AI_Hero target)
        {
            var harassMenu = Menu.SubMenu("harass");
            var useQ = harassMenu.Item("harassUseQ").GetValue<bool>() && _q.IsReady();
            var useE = harassMenu.Item("harassUseE").GetValue<bool>() && _e.IsReady();

            if (target.HasBuffOfType(BuffType.Invulnerability))
            {
                return;
            }

            if (useQ && Player.Distance(target.Position) < _q.Range)
            {
                _q.Cast(target, Packets());
            }

            if (useE && Player.Distance(target.Position) < _e.Range)
            {
                _e.Cast(target, Packets());
            }
        }

        private static void Killsteal(Obj_AI_Hero target)
        {
            var killstealMenu = Menu.SubMenu("killsteal");
            var useQ = killstealMenu.Item("killstealUseQ").GetValue<bool>() && _q.IsReady();
            var useE = killstealMenu.Item("killstealUseE").GetValue<bool>() && _e.IsReady();
            var useR = killstealMenu.Item("killstealUseR").GetValue<bool>() && _r.IsReady();

            if (target.HasBuffOfType(BuffType.Invulnerability))
            {
                return;
            }

            if (useQ && target.Distance(Player.Position) < _q.Range)
            {
                if (_q.IsKillable(target))
                {
                    _q.Cast(target, Packets());
                }
            }

            if (useE && target.Distance(Player.Position) < _e.Range)
            {
                if (_e.IsKillable(target))
                {
                    _e.Cast(target, Packets());
                }
            }

            if (useR && target.Distance(Player.Position) < _r.Range)
            {
                if (_r.IsKillable(target))
                {
                    _r.Cast(target, Packets());
                }
            }
        }

        private static void WaveClear()
        {
            var waveclearMenu = Menu.SubMenu("waveclear");
            var useQ = waveclearMenu.Item("wcUseQ").GetValue<bool>() && _q.IsReady();
            var useE = waveclearMenu.Item("wcUseE").GetValue<bool>() && _e.IsReady();

            var allMinionsQ = MinionManager.GetMinions(Player.ServerPosition, _q.Range);
            var allMinionsE = MinionManager.GetMinions(Player.ServerPosition, _e.Range);

            if (useQ)
            {
                foreach (var minion in allMinionsQ.Where(minion => minion.IsValidTarget() &&
                                                                   HealthPrediction.GetHealthPrediction(minion,
                                                                       (int)
                                                                           (Player.Distance(minion.Position)*1000/1400)) <
                                                                   Player.GetSpellDamage(minion, SpellSlot.Q)))
                {
                    _q.CastOnUnit(minion, Packets());
                    return;
                }
            }

            if (useE && allMinionsE.Count > 3)
            {
                var farm = _e.GetLineFarmLocation(allMinionsE, _e.Width);
                if (allMinionsE.Any(minion => minion.IsValidTarget() &&
                                              HealthPrediction.GetHealthPrediction(minion,
                                                  (int) (Player.Distance(minion.Position)*1000/1400)) <
                                              Player.GetSpellDamage(minion, SpellSlot.E)))
                {
                    _e.Cast(farm.Position, Packets());
                    return;
                }
            }

            var jcreeps = MinionManager.GetMinions(Player.ServerPosition, _e.Range, MinionTypes.All,
                MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            if (jcreeps.Count <= 0)
            {
                return;
            }

            var jcreep = jcreeps[0];
            if (useQ)
            {
                _q.Cast(jcreep, Packets());
            }

            if (useE)
            {
                _e.Cast(jcreep, Packets());
            }
        }

        private static void UltToMouse()
        {
            var miscMenu = Menu.SubMenu("misc");
            var useR = miscMenu.Item("miscUseR").GetValue<bool>() && _r.IsReady();
            var rOnPlayer = RBuffCount();
            var keepStacks = miscMenu.Item("miscUltStacks").GetValue<Slider>().Value;

            if (useR && rOnPlayer < keepStacks)
            {
                Orbwalking.Orbwalk(null, Game.CursorPos);
                _r.Cast(Game.CursorPos, Packets());
            }
            else
            {
                Orbwalking.Orbwalk(null, Game.CursorPos);
            }
        }

        private static int RBuffCount()
        {
            var buff =
                ObjectManager.Player.Buffs.FirstOrDefault(buff1 => buff1.Name.Equals("RiftWalk"));

            return buff != null ? buff.Count : 0;
        }

        private static float GetComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;
            if (_r.IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.R);
            }

            if (_dfg.IsReady())
            {
                damage += Player.GetItemDamage(enemy, Damage.DamageItems.Dfg)/1.2;
            }

            if (_w.IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.W);
            }

            if (_q.IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q);
            }

            if (_e.IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.E);
            }

            if (_igniteSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(_igniteSlot) == SpellState.Ready)
            {
                damage += Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);
            }

            return (float) damage*(_dfg.IsReady() ? 1.2f : 1);
        }

        private static bool Packets()
        {
            return Menu.Item("miscPacket").GetValue<bool>();
        }

        private static void CreateMenu()
        {
            Menu = new Menu("Black" + ChampName, "black" + ChampName, true);

            // Target selector
            var ts = new Menu("Target Selector", "ts");
            Menu.AddSubMenu(ts);
            TargetSelector.AddToMenu(ts);

            // Orbwalker
            var orbwalk = new Menu("Orbwalking", "orbwalk");
            Menu.AddSubMenu(orbwalk);
            _ow = new Orbwalking.Orbwalker(orbwalk);

            // Combo
            var combo = new Menu("Combo", "combo");
            Menu.AddSubMenu(combo);
            combo.AddItem(new MenuItem("comboUseQ", "Use Q").SetValue(true));
            combo.AddItem(new MenuItem("comboUseW", "Use W").SetValue(true));
            combo.AddItem(new MenuItem("comboUseE", "Use E").SetValue(true));
            combo.AddItem(new MenuItem("comboUseR", "Use R").SetValue(true));
            combo.AddItem(new MenuItem("comboActive", "Combo active!").SetValue(new KeyBind(32, KeyBindType.Press)));

            // Harass
            var harass = new Menu("Harass", "harass");
            Menu.AddSubMenu(harass);
            harass.AddItem(new MenuItem("harassUseQ", "Use Q").SetValue(true));
            harass.AddItem(new MenuItem("harassUseE", "Use E").SetValue(false));
            harass.AddItem(new MenuItem("harassMana", "Mana To Harass").SetValue(new Slider(40, 100, 0)));
            harass.AddItem(new MenuItem("harassActive", "Harass active!").SetValue(new KeyBind('C', KeyBindType.Press)));

            // WaveClear
            var waveclear = new Menu("Waveclear", "waveclear");
            Menu.AddSubMenu(waveclear);
            waveclear.AddItem(new MenuItem("wcUseQ", "Use Q").SetValue(true));
            waveclear.AddItem(new MenuItem("wcUseE", "Use E").SetValue(true));
            waveclear.AddItem(new MenuItem("wcMana", "Mana to Waveclear").SetValue(new Slider(40, 100, 0)));
            waveclear.AddItem(new MenuItem("wcActive", "Waveclear active!").SetValue(new KeyBind('V', KeyBindType.Press)));

            // Killsteal
            var killsteal = new Menu("Killsteal", "killsteal");
            Menu.AddSubMenu(killsteal);
            killsteal.AddItem(new MenuItem("killstealUseQ", "Use Q").SetValue(true));
            killsteal.AddItem(new MenuItem("killstealUseE", "Use E").SetValue(false));
            killsteal.AddItem(new MenuItem("killstealUseR", "Use R").SetValue(false));

            // Misc
            var misc = new Menu("Misc", "misc");
            Menu.AddSubMenu(misc);
            misc.AddItem(new MenuItem("miscPacket", "Use Packets").SetValue(true));
            misc.AddItem(new MenuItem("miscIgnite", "Use Ignite").SetValue(true));
            misc.AddItem(new MenuItem("miscDFG", "Use DFG").SetValue(true));
            misc.AddItem(new MenuItem("miscUltToMouse", "Ult to mouse").SetValue(new KeyBind('G', KeyBindType.Press)));
            misc.AddItem(new MenuItem("miscUseR", "Use R in Ult to mouse").SetValue(true));
            misc.AddItem(new MenuItem("miscUltStacks", "Limit to X Stacks")).SetValue(new Slider(3, 1, 4));


            //Damage after combo:
            var dmgAfterComboItem = new MenuItem("DamageAfterCombo", "Draw damage after combo").SetValue(true);
            Utility.HpBarDamageIndicator.DamageToUnit = GetComboDamage;
            Utility.HpBarDamageIndicator.Enabled = dmgAfterComboItem.GetValue<bool>();
            dmgAfterComboItem.ValueChanged +=
                delegate(object sender, OnValueChangeEventArgs eventArgs)
                {
                    Utility.HpBarDamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
                };

            // Drawings
            var drawings = new Menu("Drawings", "drawings");
            Menu.AddSubMenu(drawings);
            drawings.AddItem(new MenuItem("drawRangeQ", "Q range").SetValue(new Circle(true, Color.Aquamarine)));
            drawings.AddItem(new MenuItem("drawRangeW", "W range").SetValue(new Circle(false, Color.Aquamarine)));
            drawings.AddItem(new MenuItem("drawRangeE", "E / R range").SetValue(new Circle(false, Color.Aquamarine)));
            //drawings.AddItem(new MenuItem("drawRangeR", "R range").SetValue(new Circle(false, Color.Aquamarine)));
            drawings.AddItem(dmgAfterComboItem);

            // Finalizing
            Menu.AddToMainMenu();
        }
    }
}