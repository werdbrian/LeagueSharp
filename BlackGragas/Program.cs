using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace BlackGragas
{
    internal class Program
    {
        // Generic
        public static readonly string ChampName = "Gragas";
        private static readonly Obj_AI_Hero Player = ObjectManager.Player;
        // Spells
        private static readonly List<Spell> SpellList = new List<Spell>();
        private static Spell _q, _q2, _w, _e, _r;
        private static SpellSlot _igniteSlot;
        // Barrel
        private static GameObject _qBarrel;
        private static float _qBarrelCreateTime;
        // Menu
        public static Menu Menu;
        private static Orbwalking.Orbwalker _ow;
        public static float QBarrelMaxDamageTime { get; set; }

        public static void Main(string[] args)
        {
            // Register events
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            //Champ validation
            if (Player.ChampionName != ChampName) return;

            //Define spells
            _q = new Spell(SpellSlot.Q, 850);
            _q2 = new Spell(SpellSlot.Q, 270);
            _w = new Spell(SpellSlot.W);
            _e = new Spell(SpellSlot.E, 600);
            _r = new Spell(SpellSlot.R, 1150);
            SpellList.AddRange(new[] {_q, _e, _r});

            _igniteSlot = Player.GetSpellSlot("SummonerDot");

            // Finetune spells
            _q.SetSkillshot(0.3f, 160f, 1000f, false, SkillshotType.SkillshotCircle);
            _e.SetSkillshot(0.3f, 80, 1000, true, SkillshotType.SkillshotLine);
            _r.SetSkillshot(0.3f, 400f, 1000f, false, SkillshotType.SkillshotCircle);

            // Create menu
            CreateMenu();

            // Register events
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            GameObject.OnCreate += OnCreateObject;
            GameObject.OnDelete += OnDeleteObject;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;

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
                    Utility.DrawCircle(Player.Position, spell.Range, circleEntry.Color);
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

            // Killsteal
            Killsteal(target);

            // Detonate Q
            DetonateQ();
        }

        private static void OnCombo(Obj_AI_Hero target)
        {
            var comboMenu = Menu.SubMenu("combo");
            var useQ = comboMenu.Item("comboUseQ").GetValue<bool>() && _q.IsReady();
            var useW = comboMenu.Item("comboUseW").GetValue<bool>() && _w.IsReady();
            var useE = comboMenu.Item("comboUseE").GetValue<bool>() && _e.IsReady();
            var useR = comboMenu.Item("comboUseR").GetValue<bool>() && _r.IsReady();

            if (target != null && target.HasBuffOfType(BuffType.Invulnerability))
            {
                return;
            }

            if (target != null && (useR && Player.Distance(target.Position) < _r.Range))
            {
                SmartUlt();
            }

            if (target != null && (useW && Player.Distance(target.Position) < _e.Range))
            {
                _w.Cast(Player, Packets());
            }

            if (target != null && (useE && Player.Distance(target.Position) < _e.Range))
            {
                _e.Cast(target, Packets());
            }

            if (target != null && (useE && Player.Distance(target.Position) > _e.Range))
            {
                _e.Cast(target, Packets());
            }

            if (target != null &&
                (useQ && Player.Distance(target.Position) < _q.Range &&
                 _q.GetPrediction(target).Hitchance >= HitChance.High))
            {
                Utility.DelayAction.Add(500, () => _q.Cast(target, Packets()));

                if (_qBarrel != null)
                {
                    if ((Game.Time - QBarrelMaxDamageTime) >= 0)
                    {
                        if (target.Distance(_qBarrel.Position) < _q2.Range)
                        {
                            _q.Cast(Packets());
                        }
                    }
                }
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

                if (_qBarrel != null)
                {
                    if ((Game.Time - QBarrelMaxDamageTime) >= 0)
                    {
                        if (target.Distance(_qBarrel.Position) < _q2.Range)
                        {
                            _q.Cast(Packets());
                        }
                    }
                }
            }

            if (!useE || !(Player.Distance(target.Position) < _e.Range))
            {
                return;
            }

            _e.Cast(target, Packets());
        }

        private static void Killsteal(Obj_AI_Hero target)
        {
            var killstealMenu = Menu.SubMenu("killsteal");
            var useQ = killstealMenu.Item("killstealUseQ").GetValue<bool>() && _q.IsReady();
            var useE = killstealMenu.Item("killstealUseE").GetValue<bool>() && _e.IsReady();
            var useR = killstealMenu.Item("killstealUseR").GetValue<bool>() && _r.IsReady();

            if (target.HasBuffOfType(BuffType.Invulnerability)) return;

            if (useQ && target.Distance(Player.Position) < _q.Range)
            {
                if (_q.IsKillable(target))
                {
                    _q.Cast(target, Packets());
                }

                if (_qBarrel != null)
                {
                    if ((Game.Time - QBarrelMaxDamageTime) >= 0)
                    {
                        if (target.Distance(_qBarrel.Position) < _q2.Range)
                        {
                            _q.Cast(Packets());
                        }
                    }
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
            var useW = waveclearMenu.Item("wcUseW").GetValue<bool>() && _w.IsReady();
            var useE = waveclearMenu.Item("wcUseE").GetValue<bool>() && _e.IsReady();

            var allMinionsQ = MinionManager.GetMinions(Player.ServerPosition, _q.Range);
            var allMinionsE = MinionManager.GetMinions(Player.ServerPosition, _e.Range);

            if (useQ && allMinionsQ.Count > 2)
            {
                var farm = _q.GetCircularFarmLocation(allMinionsQ, _q.Width);
                var qBuff = Player.HasBuff("GragasQ");

                foreach (var minion in allMinionsQ)
                {
                    if (minion.IsValidTarget() &&
                        HealthPrediction.GetHealthPrediction(minion,
                            (int) (Player.Distance(minion.Position)*1000/1000)) <
                        Player.GetSpellDamage(minion, SpellSlot.Q))
                    {
                        _q.Cast(farm.Position, Packets());
                        return;
                    }

                    if (_qBarrel != null && qBuff)
                    {
                        _q.Cast(Packets());
                    }
                }
            }

            if (useE && allMinionsE.Count > 1)
            {
                var farm = _e.GetLineFarmLocation(allMinionsE, _e.Width);

                if (allMinionsE.Any(minion => minion.IsValidTarget() &&
                                              HealthPrediction.GetHealthPrediction(minion,
                                                  (int) (Player.Distance(minion.Position)*1000/1000)) <
                                              Player.GetSpellDamage(minion, SpellSlot.E)))
                {
                    _e.Cast(farm.Position, Packets());
                    return;
                }
            }

            if (useW && allMinionsQ.Count > 1)
            {
                _w.Cast(Player, Packets());
            }

            var jcreeps = MinionManager.GetMinions(Player.ServerPosition, _e.Range, MinionTypes.All,
                MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (jcreeps.Count > 0)
            {
                var jcreep = jcreeps[0];
                var qBuff = Player.HasBuff("GragasQ");

                if (useQ)
                {
                    _q.Cast(jcreep, Packets());

                    if (qBuff)
                    {
                        _q.Cast(Packets());
                    }
                }

                if (useE)
                {
                    _e.Cast(jcreep, Packets());
                }

                if (useW)
                {
                    _w.Cast(Player, Packets());
                }
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

            if (_e.IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.E);
            }

            if (_r.IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.R);
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

        private static void SmartUlt() //Kappa
        {
            foreach (
                var throwRBehind in
                    from unit in
                        ObjectManager.Get<Obj_AI_Hero>()
                            .Where(champ => champ.IsValidTarget(_r.Range) && !champ.IsDead && champ.IsEnemy)
                            .OrderBy(champ => Player.Distance(champ.Position))
                    let rPred = _r.GetPrediction(unit)
                    select rPred.CastPosition - Vector3.Normalize(unit.ServerPosition - Player.ServerPosition)*-125
                    into throwRBehind
                    where _r.IsReady()
                    select throwRBehind)
            {
                _r.Cast(throwRBehind, Packets());
            }
        }

        private static void DetonateQ()
        {
            if (!Menu.Item("miscDetonateQ").GetValue<bool>() || _qBarrel.Position == default(Vector3) ||
                _qBarrel == null)
            {
                return;
            }

            if (_qBarrel.Position.CountEnemysInRange(250) >= 1)
            {
                _q.Cast(Packets());
            }
        }

        private static void OnCreateObject(GameObject sender, EventArgs args)
        {
            if (!sender.Name.Contains("Gragas") || !sender.Name.Contains("Q_Ally"))
            {
                return;
            }

            _qBarrel = sender;
            _qBarrelCreateTime = Game.Time;
            QBarrelMaxDamageTime = _qBarrelCreateTime + 2;
        }

        private static void OnDeleteObject(GameObject sender, EventArgs args)
        {
            if (!sender.Name.Contains("Gragas") || !sender.Name.Contains("Q_Ally"))
            {
                return;
            }

            _qBarrel = null;
        }

        private static void Interrupter_OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            var miscInterruptE = Menu.Item("miscInterruptE").GetValue<bool>();
            var miscInterruptR = Menu.Item("miscInterruptR").GetValue<bool>();

            if (spell.DangerLevel != InterruptableDangerLevel.High)
            {
                return;
            }

            if (miscInterruptE && Player.Distance(unit.Position) < _e.Range)
            {
                _e.Cast(unit, Packets());
            }

            if (miscInterruptR && Player.Distance(unit.Position) < _r.Range)
            {
                _r.Cast(unit, Packets());
            }
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
            waveclear.AddItem(new MenuItem("wcUseW", "Use W").SetValue(true));
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
            misc.AddItem(new MenuItem("miscDetonateQ", "Auto detonate Q").SetValue(true));
            misc.AddItem(new MenuItem("miscInterrupt", "Interrupt Spells").SetValue(true));
            misc.AddItem(new MenuItem("sep0", "========="));
            misc.AddItem(new MenuItem("miscInterruptE", "Interrupt with E").SetValue(true));
            misc.AddItem(new MenuItem("miscInterruptR", "Interrupt with R").SetValue(false));
            misc.AddItem(new MenuItem("sep1", "========="));

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
            drawings.AddItem(new MenuItem("drawRangeE", "E range").SetValue(new Circle(false, Color.Aquamarine)));
            drawings.AddItem(new MenuItem("drawRangeR", "R range").SetValue(new Circle(false, Color.Aquamarine)));
            drawings.AddItem(dmgAfterComboItem);

            // Finalizing
            Menu.AddToMainMenu();
        }
    }
}