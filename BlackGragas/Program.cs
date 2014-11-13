using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace BlackGragas
{
    class Program
    {
        // Generic
        public static readonly string champName = "Gragas";
        private static readonly Obj_AI_Hero player = ObjectManager.Player;

        // Spells
        private static readonly List<Spell> spellList = new List<Spell>();
        private static Spell Q, Q2, W, E, R;
        private static SpellSlot IgniteSlot;

        // Barrel
        private static GameObject QBarrel;
        private static float QBarrelCreateTime;
        private static bool _canThrowBarrel = true;
        public static bool CanThrowBarrel
        {
            get { return _canThrowBarrel; }
            set { _canThrowBarrel = value; }
        }

        public static float QBarrelMaxDamageTime { get; set; }

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
            Q = new Spell(SpellSlot.Q, 850);
            Q2 = new Spell(SpellSlot.Q, 250);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 600);
            R = new Spell(SpellSlot.R, 1150);
            spellList.AddRange(new[] { Q, E, R });

            IgniteSlot = player.GetSpellSlot("SummonerDot");

            // Finetune spells
            Q.SetSkillshot(0.3f, 160f, 1000f, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.3f, 80, 1000, true, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.3f, 400f, 1000f, false, SkillshotType.SkillshotCircle);

            // Create menu
            createMenu();

            // Register events
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            GameObject.OnCreate += OnCreateObject;
            GameObject.OnDelete += OnDeleteObject;

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
            Obj_AI_Hero target = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Magical);

            // Combo
            if (menu.SubMenu("combo").Item("comboActive").GetValue<KeyBind>().Active)
                OnCombo(target);

            // Harass
            if (menu.SubMenu("harass").Item("harassActive").GetValue<KeyBind>().Active &&
               (player.Mana / player.MaxMana * 100) >
                menu.Item("harassMana").GetValue<Slider>().Value)
                OnHarass(target);

            // WaveClear
            if (menu.SubMenu("waveclear").Item("wcActive").GetValue<KeyBind>().Active &&
               (player.Mana / player.MaxMana * 100) >
                menu.Item("wcMana").GetValue<Slider>().Value)
                waveclear();

            // Killsteal
            Killsteal(target);

            // Detonate Q
            DetonateQ();
        }

        private static void OnCombo(Obj_AI_Hero target)
        {
            Menu comboMenu = menu.SubMenu("combo");
            bool useQ = comboMenu.Item("comboUseQ").GetValue<bool>() && Q.IsReady();
            bool useW = comboMenu.Item("comboUseW").GetValue<bool>() && W.IsReady();
            bool useE = comboMenu.Item("comboUseE").GetValue<bool>() && E.IsReady();
            bool useR = comboMenu.Item("comboUseR").GetValue<bool>() && R.IsReady();

            var comboDamage = target != null ? GetComboDamage(target) : 0;

            if (target.HasBuffOfType(BuffType.Invulnerability)) return;

            if (useR && player.Distance(target) < R.Range)
            {
                if (target != null)
                    smartUlt();
            }

            if (useW && player.Distance(target) < E.Range)
            {
                if (target != null)
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
                    E.Cast(target, packets());
            }

            if (useQ && player.Distance(target) < Q.Range && Q.GetPrediction(target).Hitchance >= HitChance.High && QBarrel == null)
            {
                if (target != null)
                    Utility.DelayAction.Add(500, () => Q.Cast(target, packets()));
                    QBarrel = new GameObject();

                    if (QBarrel != null)
                    {
                        if ((Game.Time - QBarrelMaxDamageTime) >= 0)
                        {
                            if (target.Distance(QBarrel.Position) < Q2.Range)
                            {
                                Q.Cast(packets());
                                QBarrel = null;
                            }
                        }
                    }
            }

            if (target != null && menu.Item("miscIgnite").GetValue<bool>() && IgniteSlot != SpellSlot.Unknown &&
            player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
            {
                if (GetComboDamage(target) > target.Health)
                {
                    player.SummonerSpellbook.CastSpell(IgniteSlot, target);
                }
            }
        }

        private static void OnHarass(Obj_AI_Hero target)
        {
            Menu harassMenu = menu.SubMenu("harass");
            bool useQ = harassMenu.Item("harassUseQ").GetValue<bool>() && Q.IsReady();
            bool useE = harassMenu.Item("harassUseE").GetValue<bool>() && E.IsReady();

            if (target.HasBuffOfType(BuffType.Invulnerability)) return;

            if (useQ && player.Distance(target) < Q.Range)
            {
                if (target != null)
                    Q.Cast(target, packets());
                    QBarrel = new GameObject();

                    if (QBarrel != null)
                    {
                        if ((Game.Time - QBarrelMaxDamageTime) >= 0)
                        {
                            if (target.Distance(QBarrel.Position) < Q2.Range)
                            {
                                Q.Cast(packets());
                                QBarrel = null;
                            }
                        }
                    }
            }

            if (useE && player.Distance(target) < E.Range)
            {
                if (target != null)
                    E.Cast(target, packets());
            }
        }

        private static void Killsteal(Obj_AI_Hero target)
        {
            Menu killstealMenu = menu.SubMenu("killsteal");
            bool useQ = killstealMenu.Item("killstealUseQ").GetValue<bool>() && Q.IsReady();
            bool useE = killstealMenu.Item("killstealUseE").GetValue<bool>() && E.IsReady();
            bool useR = killstealMenu.Item("killstealUseR").GetValue<bool>() && R.IsReady();

            if (target.HasBuffOfType(BuffType.Invulnerability)) return;

            if (useQ && target.Distance(player) < Q.Range)
            {
                if (Q.IsKillable(target))
                {
                    Q.Cast(target, packets());
                    QBarrel = new GameObject();
                }

                if (QBarrel != null)
                {
                    if ((Game.Time - QBarrelMaxDamageTime) >= 0)
                    {
                        if (target.Distance(QBarrel.Position) < Q2.Range)
                        {
                            Q.Cast(packets());
                            QBarrel = null;
                        }
                    }
                }
            }
            if (useE && target.Distance(player) < E.Range)
            {
                if (E.IsKillable(target))
                {
                    E.Cast(target, packets());
                }
            }

            if (useR && target.Distance(player) < R.Range)
            {
                if (R.IsKillable(target))
                {
                    R.Cast(target, packets());
                }
            }
        }

        private static void waveclear()
        {
            Menu waveclearMenu = menu.SubMenu("waveclear");
            bool useQ = waveclearMenu.Item("wcUseQ").GetValue<bool>() && Q.IsReady();
            bool useW = waveclearMenu.Item("wcUseW").GetValue<bool>() && W.IsReady();
            bool useE = waveclearMenu.Item("wcUseE").GetValue<bool>() && E.IsReady();

            var allMinionsQ = MinionManager.GetMinions(player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Enemy);
            var allMinionsE = MinionManager.GetMinions(player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Enemy);

            if (useQ && allMinionsQ.Count > 2)
            {
                var farm = Q.GetCircularFarmLocation(allMinionsQ, Q.Width);
                var QBuff = player.HasBuff("GragasQ");

                foreach (var minion in allMinionsQ)
                {
                    if (minion.IsValidTarget() &&
                    HealthPrediction.GetHealthPrediction(minion,
                    (int)(player.Distance(minion) * 1000 / 1000)) <
                    player.GetSpellDamage(minion, SpellSlot.Q))
                    {
                        Q.Cast(farm.Position, packets());
                        return;
                    }
                   
                    if (QBuff)
                    {
                        Q.Cast(packets());
                    }
                }
            }

            if (useE && allMinionsE.Count > 1)
            {
                var farm = E.GetLineFarmLocation(allMinionsE, E.Width);

                foreach (var minion in allMinionsE)
                {
                    if (minion.IsValidTarget() &&
                    HealthPrediction.GetHealthPrediction(minion,
                    (int)(player.Distance(minion) * 1000 / 1000)) <
                    player.GetSpellDamage(minion, SpellSlot.E))
                    {
                        E.Cast(farm.Position, packets());
                        return;
                    }
                }
            }

            if (useW && allMinionsQ.Count > 1)
            {
                W.Cast(player, packets());
            }

            var jcreeps = MinionManager.GetMinions(player.ServerPosition, E.Range, MinionTypes.All,
            MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (jcreeps.Count > 0)
            {
                var jcreep = jcreeps[0];
                var QBuff = player.HasBuff("GragasQ");

                if (useQ)
                {
                    Q.Cast(jcreep, packets());

                    if (QBuff)
                    {
                        Q.Cast(packets());
                    }
                }

                if (useE)
                {
                    E.Cast(jcreep, packets());
                }

                if (useW)
                {
                    W.Cast(player, packets());
                }
            }
        }

        private static float GetComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;
            if (Q.IsReady())
                damage += player.GetSpellDamage(enemy, SpellSlot.Q);

            if (W.IsReady())
                damage += player.GetSpellDamage(enemy, SpellSlot.W);

            if (E.IsReady())
                damage += player.GetSpellDamage(enemy, SpellSlot.E);

            if (R.IsReady())
                damage += player.GetSpellDamage(enemy, SpellSlot.R);

            if (IgniteSlot != SpellSlot.Unknown && player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
                damage += player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);

            return (float)damage;
        }

        private static bool packets()
        {
            return menu.Item("miscPacket").GetValue<bool>();
        }

        private static void smartUlt()
        {
            foreach (var unit in ObjectManager.Get<Obj_AI_Hero>().Where(champ => champ.IsValidTarget(R.Range) && !champ.IsDead && champ.IsEnemy).OrderBy(champ => player.Distance(champ)))
            {
                var RPred = R.GetPrediction(unit);
                var throwRBehind = RPred.CastPosition - Vector3.Normalize(unit.ServerPosition - player.ServerPosition) * -125;
                if (R.IsReady())
                    R.Cast(throwRBehind, packets());
            }
        }

        private static void DetonateQ()
        {
            if (!menu.Item("miscDetonateQ").GetValue<bool>() || QBarrel.Position == default(Vector3) || QBarrel == null)
                return;

            if (QBarrel.Position.CountEnemysInRange(250) >= 1)
            {
                Q.Cast(packets());
                QBarrel = null;
            }
        }

        private static void OnCreateObject(GameObject sender, EventArgs args)
        {
            if (!sender.Name.Contains("Gragas") || !sender.Name.Contains("Q_Ally")) return;

            //Game.PrintChat(sender.Name);
            //Game.PrintChat("Gragas Q is out!");
            QBarrel = sender;
            QBarrelCreateTime = Game.Time;
            QBarrelMaxDamageTime = QBarrelCreateTime + 2;
            CanThrowBarrel = false;
        }
        private static void OnDeleteObject(GameObject sender, EventArgs args)
        {
            if (!sender.Name.Contains("Gragas") || !sender.Name.Contains("Q_Ally")) return;

            //Game.PrintChat(sender.Name);
            //Game.PrintChat("Gragas Q exploded!");
            QBarrel = null;
            CanThrowBarrel = true;
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

            // WaveClear
            Menu waveclear = new Menu("Waveclear", "waveclear");
            menu.AddSubMenu(waveclear);
            waveclear.AddItem(new MenuItem("wcUseQ", "Use Q").SetValue(true));
            waveclear.AddItem(new MenuItem("wcUseW", "Use W").SetValue(true));
            waveclear.AddItem(new MenuItem("wcUseE", "Use E").SetValue(true));
            waveclear.AddItem(new MenuItem("wcMana", "Mana to Waveclear").SetValue(new Slider(40, 100, 0)));
            waveclear.AddItem(new MenuItem("wcActive", "Waveclear active!").SetValue(new KeyBind('V', KeyBindType.Press)));

            // Killsteal
            Menu killsteal = new Menu("Killsteal", "killsteal");
            menu.AddSubMenu(killsteal);
            killsteal.AddItem(new MenuItem("killstealUseQ", "Use Q").SetValue(true));
            killsteal.AddItem(new MenuItem("killstealUseE", "Use E").SetValue(false));
            killsteal.AddItem(new MenuItem("killstealUseR", "Use R").SetValue(false));

            // Misc
            Menu misc = new Menu("Misc", "misc");
            menu.AddSubMenu(misc);
            misc.AddItem(new MenuItem("miscPacket", "Use Packets").SetValue(true));
            misc.AddItem(new MenuItem("miscIgnite", "Use Ignite").SetValue(true));
            misc.AddItem(new MenuItem("miscDetonateQ", "Auto detonate Q").SetValue(true));
            //misc.AddItem(new MenuItem("miscInterrupt", "Interrupt Spells").SetValue(true));
            //misc.AddItem(new MenuItem("sep0", "========="));
            //misc.AddItem(new MenuItem("miscInterruptE", "Interrupt with E").SetValue(true));
            //misc.AddItem(new MenuItem("miscInterruptR", "Interrupt with R").SetValue(true));
            //misc.AddItem(new MenuItem("sep1", "========="));

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
            drawings.AddItem(new MenuItem("drawRangeE", "E range").SetValue(new Circle(false, Color.Aquamarine)));
            drawings.AddItem(new MenuItem("drawRangeR", "R range").SetValue(new Circle(false, Color.Aquamarine)));
            drawings.AddItem(dmgAfterComboItem);

            // Finalizing
            menu.AddToMainMenu();
        }
    }
}

