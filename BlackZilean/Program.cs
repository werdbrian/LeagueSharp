using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace BlackZilean
{
    internal class Program
    {
        // Generic
        public static readonly string ChampName = "Zilean";
        private static readonly Obj_AI_Hero Player = ObjectManager.Player;
        // Spells
        private static readonly List<Spell> SpellList = new List<Spell>();
        private static Spell _q, _w, _e, _r;
        private static SpellSlot _igniteSlot;
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
            _q = new Spell(SpellSlot.Q, 700);
            _w = new Spell(SpellSlot.W);
            _e = new Spell(SpellSlot.E, 700);
            _r = new Spell(SpellSlot.R, 900);
            SpellList.AddRange(new[] {_q, _e, _r});

            _igniteSlot = Player.GetSpellSlot("SummonerDot");

            // Create menu
            CreateMenu();

            // Register events
            Game.OnUpdate += Game_OnGameUpdate;
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
                (ObjectManager.Player.Mana/ObjectManager.Player.MaxMana*100) >
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

            // AutoUlt
            if (Menu.SubMenu("ult").Item("ultUseR").GetValue<bool>())
            {
                AutoUlt();
            }

            // Misc
            if (Menu.SubMenu("misc").Item("miscEscapeToMouse").GetValue<KeyBind>().Active)
            {
                EscapeToMouse();
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

            if (useQ && Player.Distance(target.Position) < _q.Range)
            {
                _q.Cast(target, Packets());
            }

            if (useW && !_q.IsReady())
            {
                _w.Cast(Player, Packets());
            }

            if (useE && Player.Distance(target.Position) < _e.Range)
            {
                _e.Cast(target, Packets());
            }

            if (useE && Player.Distance(target.Position) > _e.Range)
            {
                _e.Cast(Player, Packets());
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
            var useW = harassMenu.Item("harassUseW").GetValue<bool>() && _w.IsReady();

            if (useQ && Player.Distance(target.Position) < _q.Range)
            {
                _q.Cast(target, Packets());
            }

            if (useW && !_q.IsReady())
            {
                _w.Cast(Player, Packets());
            }
        }

        private static void Killsteal(Obj_AI_Hero target)
        {
            var killstealMenu = Menu.SubMenu("killsteal");
            var useQ = killstealMenu.Item("killstealUseQ").GetValue<bool>() && _q.IsReady();
            var useW = killstealMenu.Item("killstealUseW").GetValue<bool>() && _w.IsReady();

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

            if (useW && !_q.IsReady())
            {
                _w.Cast(Player, Packets());
            }
        }

        private static void WaveClear()
        {
            var waveclearMenu = Menu.SubMenu("waveclear");
            var useQ = waveclearMenu.Item("wcUseQ").GetValue<bool>() && _q.IsReady();
            var useW = waveclearMenu.Item("wcUseW").GetValue<bool>() && _w.IsReady();

            var allMinionsQ = MinionManager.GetMinions(Player.ServerPosition, _q.Range);

            if (useQ && allMinionsQ.Count > 2)
            {
                foreach (var minion in allMinionsQ.Where(minion => minion.IsValidTarget() &&
                                                                   _q.IsKillable(minion)))
                {
                    _q.CastOnUnit(minion, Packets());
                    return;
                }
            }

            if (useW && !_q.IsReady())
            {
                _w.Cast(Player, Packets());
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

            if (useW && !_q.IsReady())
            {
                _w.Cast(Player, Packets());
            }
        }

        private static void AutoUlt()
        {
            if (!Menu.Item("ultUseR").GetValue<bool>())
            {
                return;
            }

            foreach (var aChamp in from aChamp in ObjectManager.Get<Obj_AI_Hero>()
                where (aChamp.IsAlly) && (ObjectManager.Player.ServerPosition.Distance(aChamp.Position) < _r.Range)
                where Menu.Item("Ult" + aChamp.BaseSkinName).GetValue<bool>() && _r.IsReady()
                where aChamp.Health < (aChamp.MaxHealth*(Menu.Item("ultPercent").GetValue<Slider>().Value*0.01))
                where (!aChamp.IsDead) && (!aChamp.IsInvulnerable)
                select aChamp)
            {
                _r.CastOnUnit(aChamp, Packets());
            }
        }

        private static void EscapeToMouse()
        {
            var miscMenu = Menu.SubMenu("misc");
            var useE = miscMenu.Item("miscUseE").GetValue<bool>() && _e.IsReady();

            if (useE)
            {
                Orbwalking.Orbwalk(null, Game.CursorPos);
                _e.Cast(Player, Packets());
            }
            else
            {
                Orbwalking.Orbwalk(null, Game.CursorPos);
            }
        }

        private static float GetComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;
            if (_q.IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q);
            }

            if (_w.IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.W);
            }

            if (_q.IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q);
            }

            if (_igniteSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(_igniteSlot) == SpellState.Ready)
            {
                damage += Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);
            }

            return (float) damage;
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
            combo.AddItem(new MenuItem("comboActive", "Combo active!").SetValue(new KeyBind(32, KeyBindType.Press)));

            // Harass
            var harass = new Menu("Harass", "harass");
            Menu.AddSubMenu(harass);
            harass.AddItem(new MenuItem("harassUseQ", "Use Q").SetValue(true));
            harass.AddItem(new MenuItem("harassUseW", "Use W").SetValue(false));
            harass.AddItem(new MenuItem("harassMana", "Mana To Harass").SetValue(new Slider(40, 100, 0)));
            harass.AddItem(new MenuItem("harassActive", "Harass active!").SetValue(new KeyBind('C', KeyBindType.Press)));

            // WaveClear
            var waveclear = new Menu("Waveclear", "waveclear");
            Menu.AddSubMenu(waveclear);
            waveclear.AddItem(new MenuItem("wcUseQ", "Use Q").SetValue(true));
            waveclear.AddItem(new MenuItem("wcUseW", "Use W").SetValue(true));
            waveclear.AddItem(new MenuItem("wcMana", "Mana to Waveclear").SetValue(new Slider(40, 100, 0)));
            waveclear.AddItem(new MenuItem("wcActive", "Waveclear active!").SetValue(new KeyBind('V', KeyBindType.Press)));

            // Killsteal
            var killsteal = new Menu("Killsteal", "killsteal");
            Menu.AddSubMenu(killsteal);
            killsteal.AddItem(new MenuItem("killstealUseQ", "Use Q").SetValue(true));
            killsteal.AddItem(new MenuItem("killstealUseW", "Use W").SetValue(true));

            // Ult
            var ult = new Menu("Ult", "ult");
            Menu.AddSubMenu(ult);
            ult.AddItem(new MenuItem("ultUseR", "Use R on")).SetValue(true);
            ult.AddItem(new MenuItem("sep0", "========="));
            foreach (var Champ in ObjectManager.Get<Obj_AI_Hero>().Where(champ => champ.IsAlly))
            {
                ult.AddItem(
                    new MenuItem("Ult" + Champ.BaseSkinName, string.Format("Ult {0}", Champ.BaseSkinName)).SetValue(true));
            }
            ult.AddItem(new MenuItem("sep1", "========="));
            ult.AddItem(new MenuItem("ultPercent", "R at % HP")).SetValue(new Slider(25, 1));

            // Misc
            var misc = new Menu("Misc", "misc");
            Menu.AddSubMenu(misc);
            misc.AddItem(new MenuItem("miscPacket", "Use Packets").SetValue(true));
            misc.AddItem(new MenuItem("miscIgnite", "Use Ignite").SetValue(true));
            misc.AddItem(
                new MenuItem("miscEscapeToMouse", "Escape to mouse").SetValue(new KeyBind('G', KeyBindType.Press)));
            misc.AddItem(new MenuItem("miscUseE", "Use E in escape to mouse").SetValue(true));

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
            drawings.AddItem(new MenuItem("drawRangeQ", "Q / E range").SetValue(new Circle(true, Color.Aquamarine)));
            //drawings.AddItem(new MenuItem("drawRangeE", "E range").SetValue(new Circle(false, Color.Aquamarine)));
            drawings.AddItem(new MenuItem("drawRangeR", "R range").SetValue(new Circle(false, Color.Aquamarine)));
            drawings.AddItem(dmgAfterComboItem);

            // Finalizing
            Menu.AddToMainMenu();
        }
    }
}