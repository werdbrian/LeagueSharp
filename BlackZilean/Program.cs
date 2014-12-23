using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace BlackZilean
{
    class Program
    {
        // Generic
        public static readonly string champName = "Zilean";
        private static readonly Obj_AI_Hero player = ObjectManager.Player;

        // Spells
        private static readonly List<Spell> spellList = new List<Spell>();
        private static Spell Q, W, E, R;
        private static SpellSlot IgniteSlot;

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
            Q = new Spell(SpellSlot.Q, 700);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 700);
            R = new Spell(SpellSlot.R, 900);
            spellList.AddRange(new[] { Q, E, R });

            IgniteSlot = player.GetSpellSlot("SummonerDot");

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
            Obj_AI_Hero target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);

            // Combo
            if (menu.SubMenu("combo").Item("comboActive").GetValue<KeyBind>().Active)
                OnCombo(target);

            // Harass
            if (menu.SubMenu("harass").Item("harassActive").GetValue<KeyBind>().Active &&
               (ObjectManager.Player.Mana / ObjectManager.Player.MaxMana * 100) >
                menu.Item("harassMana").GetValue<Slider>().Value)
                OnHarass(target);

            // WaveClear
            if (menu.SubMenu("waveclear").Item("wcActive").GetValue<KeyBind>().Active &&
            (player.Mana / player.MaxMana * 100) >
            menu.Item("wcMana").GetValue<Slider>().Value)
                waveclear();

            // AutoUlt
            if (menu.SubMenu("ult").Item("ultUseR").GetValue<bool>())
                AutoUlt();

            // Misc
            if (menu.SubMenu("misc").Item("miscEscapeToMouse").GetValue<KeyBind>().Active)
                EscapeToMouse();

            // Killsteal
            Killsteal(target);

        }

        private static void OnCombo(Obj_AI_Hero target)
        {
            Menu comboMenu = menu.SubMenu("combo");
            bool useQ = comboMenu.Item("comboUseQ").GetValue<bool>() && Q.IsReady();
            bool useW = comboMenu.Item("comboUseW").GetValue<bool>() && W.IsReady();
            bool useE = comboMenu.Item("comboUseE").GetValue<bool>() && E.IsReady();

            if (useQ && player.Distance(target) < Q.Range)
            {
                if (target != null)
                    Q.Cast(target, packets());
            }

            if (useW && !Q.IsReady())
            {
                W.Cast(player, packets());
            }

            if (useE && player.Distance(target) < E.Range)
            {
                if (target != null)
                    E.Cast(target, packets());
            }

            if (useE && player.Distance(target) > E.Range)
            {
                if (target != null)
                    E.Cast(player, packets());
            }

            if (target != null && menu.Item("miscIgnite").GetValue<bool>() && IgniteSlot != SpellSlot.Unknown &&
            player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
            {
                if (GetComboDamage(target) > target.Health)
                {
                    player.Spellbook.CastSpell(IgniteSlot, target);
                }
            }
        }

        private static void OnHarass(Obj_AI_Hero target)
        {
            Menu harassMenu = menu.SubMenu("harass");
            bool useQ = harassMenu.Item("harassUseQ").GetValue<bool>() && Q.IsReady();
            bool useW = harassMenu.Item("harassUseW").GetValue<bool>() && W.IsReady();

            if (useQ && player.Distance(target) < Q.Range)
            {
                if (target != null)
                    Q.Cast(target, packets());
            }

            if (useW && !Q.IsReady())
            {
                W.Cast(player, packets());
            }
        }

        private static void Killsteal(Obj_AI_Hero target)
        {
            Menu killstealMenu = menu.SubMenu("killsteal");
            bool useQ = killstealMenu.Item("killstealUseQ").GetValue<bool>() && Q.IsReady();
            bool useW = killstealMenu.Item("killstealUseW").GetValue<bool>() && W.IsReady();

            if (target.HasBuffOfType(BuffType.Invulnerability)) return;

            if (useQ && target.Distance(player) < Q.Range)
            {
                if (Q.IsKillable(target))
                {
                    Q.Cast(target, packets());
                }
            }

            if (useW && !Q.IsReady())
            {
                W.Cast(player, packets());
            }
        }

        private static void waveclear()
        {
            Menu waveclearMenu = menu.SubMenu("waveclear");
            bool useQ = waveclearMenu.Item("wcUseQ").GetValue<bool>() && Q.IsReady();
            bool useW = waveclearMenu.Item("wcUseW").GetValue<bool>() && W.IsReady();

            var allMinionsQ = MinionManager.GetMinions(player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Enemy);

            if (useQ && allMinionsQ.Count > 2)
            {
                foreach (var minion in allMinionsQ)
                {
                    if (minion.IsValidTarget() &&
                    Q.IsKillable(minion))
                    {
                        Q.CastOnUnit(minion, packets());
                        return;
                    }
                }
            }

            if (useW && !Q.IsReady())
            {
                W.Cast(player, packets());
            }

            var jcreeps = MinionManager.GetMinions(player.ServerPosition, E.Range, MinionTypes.All,
            MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (jcreeps.Count > 0)
            {
                var jcreep = jcreeps[0];

                if (useQ)
                {
                    Q.Cast(jcreep, packets());
                }

                if (useW && !Q.IsReady())
                {
                    W.Cast(player, packets());
                }
            }
        }

        private static void AutoUlt()
        {
            if (menu.Item("ultUseR").GetValue<bool>())
            {
                foreach (Obj_AI_Hero AChamp in ObjectManager.Get<Obj_AI_Hero>())
                    if ((AChamp.IsAlly) && (ObjectManager.Player.ServerPosition.Distance(AChamp.Position) < R.Range))
                        if (menu.Item("Ult" + AChamp.BaseSkinName).GetValue<bool>() && R.IsReady())
                            if (AChamp.Health < (AChamp.MaxHealth * (menu.Item("ultPercent").GetValue<Slider>().Value * 0.01)))
                                if ((!AChamp.IsDead) && (!AChamp.IsInvulnerable))
                                {
                                    R.CastOnUnit(AChamp, packets());
                                }
            }
        }

        private static void EscapeToMouse()
        {
            Menu miscMenu = menu.SubMenu("misc");
            bool useE = miscMenu.Item("miscUseE").GetValue<bool>() && E.IsReady();

            if(useE)
            {
                Orbwalking.Orbwalk(null, Game.CursorPos);
                E.Cast(player, packets());
            }

            else
            {
                Orbwalking.Orbwalk(null, Game.CursorPos);
            }
        }

        private static float GetComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;
            if (Q.IsReady())
                damage += player.GetSpellDamage(enemy, SpellSlot.Q);

            if (W.IsReady())
                damage += player.GetSpellDamage(enemy, SpellSlot.W);

            if (Q.IsReady())
                damage += player.GetSpellDamage(enemy, SpellSlot.Q);

            if (IgniteSlot != SpellSlot.Unknown && player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
                damage += player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);

            return (float)damage;
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
            TargetSelector.AddToMenu(ts);

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
            combo.AddItem(new MenuItem("comboActive", "Combo active!").SetValue(new KeyBind(32, KeyBindType.Press)));

            // Harass
            Menu harass = new Menu("Harass", "harass");
            menu.AddSubMenu(harass);
            harass.AddItem(new MenuItem("harassUseQ", "Use Q").SetValue(true));
            harass.AddItem(new MenuItem("harassUseW", "Use W").SetValue(false));
            harass.AddItem(new MenuItem("harassMana", "Mana To Harass").SetValue(new Slider(40, 100, 0)));
            harass.AddItem(new MenuItem("harassActive", "Harass active!").SetValue(new KeyBind('C', KeyBindType.Press)));

            // WaveClear
            Menu waveclear = new Menu("Waveclear", "waveclear");
            menu.AddSubMenu(waveclear);
            waveclear.AddItem(new MenuItem("wcUseQ", "Use Q").SetValue(true));
            waveclear.AddItem(new MenuItem("wcUseW", "Use W").SetValue(true));
            waveclear.AddItem(new MenuItem("wcMana", "Mana to Waveclear").SetValue(new Slider(40, 100, 0)));
            waveclear.AddItem(new MenuItem("wcActive", "Waveclear active!").SetValue(new KeyBind('V', KeyBindType.Press)));

            // Killsteal
            Menu killsteal = new Menu("Killsteal", "killsteal");
            menu.AddSubMenu(killsteal);
            killsteal.AddItem(new MenuItem("killstealUseQ", "Use Q").SetValue(true));
            killsteal.AddItem(new MenuItem("killstealUseW", "Use W").SetValue(true));

            // Ult
            Menu ult = new Menu("Ult", "ult");
            menu.AddSubMenu(ult);
            ult.AddItem(new MenuItem("ultUseR", "Use R on")).SetValue(true);
            ult.AddItem(new MenuItem("sep0", "========="));
            foreach (Obj_AI_Hero Champ in ObjectManager.Get<Obj_AI_Hero>())
                if (Champ.IsAlly)
                    ult.AddItem(new MenuItem("Ult" + Champ.BaseSkinName, string.Format("Ult {0}", Champ.BaseSkinName)).SetValue(true));
            ult.AddItem(new MenuItem("sep1", "========="));
            ult.AddItem(new MenuItem("ultPercent", "R at % HP")).SetValue(new Slider(25, 1, 100));

            // Misc
            Menu misc = new Menu("Misc", "misc");
            menu.AddSubMenu(misc);
            misc.AddItem(new MenuItem("miscPacket", "Use Packets").SetValue(true));
            misc.AddItem(new MenuItem("miscIgnite", "Use Ignite").SetValue(true));
            misc.AddItem(new MenuItem("miscEscapeToMouse", "Escape to mouse").SetValue(new KeyBind('G', KeyBindType.Press)));
            misc.AddItem(new MenuItem("miscUseE", "Use E in escape to mouse").SetValue(true));

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
            drawings.AddItem(new MenuItem("drawRangeQ", "Q / E range").SetValue(new Circle(true, Color.Aquamarine)));
            //drawings.AddItem(new MenuItem("drawRangeE", "E range").SetValue(new Circle(false, Color.Aquamarine)));
            drawings.AddItem(new MenuItem("drawRangeR", "R range").SetValue(new Circle(false, Color.Aquamarine)));
            drawings.AddItem(dmgAfterComboItem);

            // Finalizing
            menu.AddToMainMenu();
        }
    }
}

