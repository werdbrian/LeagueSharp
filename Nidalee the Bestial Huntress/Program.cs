// This file is part of LeagueSharp.Common.
// 
// LeagueSharp.Common is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// LeagueSharp.Common is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with LeagueSharp.Common.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace NidaleeTheBestialHuntress
{
    public static class Program
    {
        private const string ChampionName = "Nidalee";
        private static readonly List<Spell> HumanSpellList = new List<Spell>();
        private static readonly List<Spell> CougarSpellList = new List<Spell>();
        private static Menu _menu;
        private static Orbwalking.Orbwalker _orbwalker;
        private static HealManager _healManager;
        private static ManaManager _manaManager;
        private static Obj_AI_Hero _player;
        private static Spell _javelinToss, _takedown, _bushwhack, _pounce, _primalSurge, _swipe, _aspectOfTheCougar;
        private static Vector3? _fleeTargetPosition;
        public static Spell Ignite { get; private set; }

        #region hitchance

        private static HitChance CustomHitChance
        {
            get { return GetHitchance(); }
        }

        #endregion

        #region Main

        public static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        #endregion

        #region OnDraw

        private static void Drawing_OnDraw(EventArgs args)
        {
            foreach (var spell in HumanSpellList)
            {
                var circleEntry = _menu.Item("drawRange" + spell.Slot).GetValue<Circle>();
                if (circleEntry.Active && !_player.IsCougar() && !_player.IsDead)
                {
                    Render.Circle.DrawCircle(_player.Position, spell.Range, circleEntry.Color);
                }
            }

            foreach (var spell in CougarSpellList)
            {
                var circleEntry = _menu.Item("drawRange" + spell.Slot).GetValue<Circle>();
                if (circleEntry.Active && _player.IsCougar() && !_player.IsDead)
                {
                    Render.Circle.DrawCircle(_player.Position, spell.Range, circleEntry.Color);
                }
            }

            Circle damageCircle = _menu.Item("drawDamage").GetValue<Circle>();

            DamageIndicator.DrawingColor = damageCircle.Color;
            DamageIndicator.Enabled = damageCircle.Active;
        }

        #endregion

        #region OnGameUpdate

        private static void Game_OnGameUpdate(EventArgs args)
        {
            var target = TargetSelector.GetTarget(_javelinToss.Range, TargetSelector.DamageType.Magical);

            Killsteal();
            OnImmobile();

            if (_menu.Item("useCombo").GetValue<KeyBind>().Active)
            {
                OnCombo(target);
            }

            if (_menu.Item("useHarass").GetValue<KeyBind>().Active)
            {
                OnHarass(target);
            }

            if (_menu.Item("useWC").GetValue<KeyBind>().Active)
            {
                WaveClear();
            }

            if (_menu.Item("useJC").GetValue<KeyBind>().Active)
            {
                JungleClear();
            }

            if (_menu.Item("useFlee").GetValue<KeyBind>().Active)
            {
                Flee();
            }
        }

        #endregion

        #region Combo

        private static void OnCombo(Obj_AI_Hero target)
        {
            float pounceDistance = target.IsHunted() ? 740 : _pounce.Range;
            if (_player.IsCougar())
            {
                if (_menu.Item("useTakedown").GetValue<bool>() && _takedown.IsReady() &&
                    _player.Distance(target.Position) <= _takedown.Range)
                {
                    _takedown.Cast(true);
                }

                if (_pounce.IsReady() && _menu.Item("usePounce").GetValue<bool>())
                {
                    if (_menu.Item("turretSafety").GetValue<bool>() && IsUnderEnemyTurret(target))
                    {
                        ShowNotification("Target is under turret, won't pounce.", Color.Red, 3100);
                        return;
                    }

                    if (target.IsHunted() && _player.Distance(target.Position) <= 740)
                    {
                        _pounce.Cast(target.ServerPosition);
                    }
                    else if (_player.Distance(target.Position) <= 400)
                    {
                        _pounce.Cast(target.Position);
                    }
                }

                if (_menu.Item("useSwipe").GetValue<bool>() && _swipe.IsReady() &&
                    _player.Distance(target.Position) <= _swipe.RangeSqr)
                {
                    if (!_pounce.IsReady())
                    {
                        _swipe.Cast(target);
                    }
                }

                if (_menu.Item("useHuman").GetValue<bool>())
                {
                   //TODO...
                }
            }
            else
            {
                if (_menu.Item("useJavelin").GetValue<bool>() && _javelinToss.IsReady() &&
                    target.IsValidTarget(_javelinToss.Range) && _player.Distance(target.Position) <= _javelinToss.Range)
                {
                    _javelinToss.CastIfHitchanceEquals(target, CustomHitChance);
                }

                if (_menu.Item("useBushwhack").GetValue<bool>() && _bushwhack.IsReady() &&
                    target.IsValidTarget(_bushwhack.Range) && _player.Distance(target.Position) <= _bushwhack.Range)
                {
                    _bushwhack.CastIfHitchanceEquals(target, CustomHitChance);
                }

                if (target.IsHunted() && !_javelinToss.IsReady() && _player.Distance(target.Position) < pounceDistance)
                {
                   //TODO
                }
            }
        }

        #endregion

        #region Harass

        private static void OnHarass(Obj_AI_Hero target)
        {
            if (!target.IsValidTarget(_javelinToss.Range) || !_manaManager.CanHarass())
            {
                return;
            }

            var pred = _javelinToss.GetPrediction(target);
            if (!_player.IsCougar() && _menu.Item("useJavelinHarass").GetValue<bool>() && _javelinToss.IsReady() &&
                target.IsValidTarget(_javelinToss.Range) && _player.Distance(target.Position) <= _javelinToss.Range &&
                pred.Hitchance >= CustomHitChance)
            {
                _javelinToss.Cast(pred.CastPosition);
            }
        }

        #endregion

        #region WaveClear

        private static void WaveClear()
        {
            List<Obj_AI_Base> allMinionsQ = MinionManager.GetMinions(
                _player.ServerPosition, _takedown.Range, MinionTypes.All, MinionTeam.NotAlly);
            List<Obj_AI_Base> allMinionsW = MinionManager.GetMinions(
                _player.ServerPosition, _pounce.Range, MinionTypes.All, MinionTeam.NotAlly);
            List<Obj_AI_Base> allMinionsE = MinionManager.GetMinions(
                _player.ServerPosition, _swipe.Range, MinionTypes.All, MinionTeam.NotAlly);
            List<Obj_AI_Base> allMinionsQ2 = MinionManager.GetMinions(
                _player.ServerPosition, _javelinToss.Range, MinionTypes.All, MinionTeam.NotAlly);
            List<Obj_AI_Base> allMinionsW2 = MinionManager.GetMinions(
                _player.ServerPosition, _bushwhack.Range, MinionTypes.All, MinionTeam.NotAlly);

            if (!allMinionsQ[0].IsValidTarget(_takedown.Range) || !allMinionsW[0].IsValidTarget(_pounce.Range) ||
                !allMinionsE[0].IsValidTarget(_swipe.Range) || !allMinionsQ2[0].IsValidTarget(_javelinToss.Range) ||
                !allMinionsW2[0].IsValidTarget(_bushwhack.Range) || !_manaManager.CanLaneclear() && !_player.IsCougar())
            {
                return;
            }

            if (_player.IsCougar())
            {
                if (_menu.Item("wcUseCougarQ").GetValue<bool>() && allMinionsQ.Count > 0 &&
                    allMinionsQ[0].IsValidTarget(_takedown.Range) && _takedown.IsReady())
                {
                    _takedown.Cast();
                }

                if (_menu.Item("wcUseCougarW").GetValue<bool>() && allMinionsW.Count > 0 &&
                    allMinionsW[0].IsValidTarget(_pounce.Range) && _pounce.IsReady())
                {
                    _pounce.Cast(allMinionsW[0]);
                }

                if (_menu.Item("wcUseCougarE").GetValue<bool>() && allMinionsE.Count > 0 &&
                    allMinionsE[0].IsValidTarget(_swipe.Range) && _swipe.IsReady())
                {
                    _swipe.Cast(allMinionsE[0]);
                }
            }
            else
            {
                if (_menu.Item("wcUseHumanQ").GetValue<bool>() && allMinionsQ2.Count > 0 &&
                    allMinionsQ2[0].IsValidTarget(_javelinToss.Range) && _javelinToss.IsReady())
                {
                    _javelinToss.Cast(allMinionsQ2[0]);
                }

                if (_menu.Item("wcUseHumanW").GetValue<bool>() && allMinionsW2.Count > 0 &&
                    allMinionsW2[0].IsValidTarget(_bushwhack.Range) && _bushwhack.IsReady())
                {
                    _bushwhack.Cast(allMinionsW2[0]);
                }
            }
        }

        #endregion

        #region JungleClear

        private static void JungleClear()
        {
            List<Obj_AI_Base> allMinionsQ = MinionManager.GetMinions(
                _player.ServerPosition, _takedown.Range, MinionTypes.All, MinionTeam.Neutral);
            List<Obj_AI_Base> allMinionsW = MinionManager.GetMinions(
                _player.ServerPosition, _pounce.Range, MinionTypes.All, MinionTeam.Neutral);
            List<Obj_AI_Base> allMinionsE = MinionManager.GetMinions(
                _player.ServerPosition, _swipe.Range, MinionTypes.All, MinionTeam.Neutral);
            List<Obj_AI_Base> allMinionsQ2 = MinionManager.GetMinions(
                _player.ServerPosition, _javelinToss.Range, MinionTypes.All, MinionTeam.Neutral);
            List<Obj_AI_Base> allMinionsW2 = MinionManager.GetMinions(
                _player.ServerPosition, _bushwhack.Range, MinionTypes.All, MinionTeam.Neutral);

            if (!allMinionsQ[0].IsValidTarget(_takedown.Range) || !allMinionsW[0].IsValidTarget(_pounce.Range) ||
                !allMinionsE[0].IsValidTarget(_swipe.Range) || !allMinionsQ2[0].IsValidTarget(_javelinToss.Range) ||
                !allMinionsW2[0].IsValidTarget(_bushwhack.Range) || !_manaManager.CanLaneclear() && !_player.IsCougar())
            {
                return;
            }

            if (_player.IsCougar())
            {
                if (_menu.Item("jcUseCougarQ").GetValue<bool>() && allMinionsQ.Count > 0 &&
                    allMinionsQ[0].IsValidTarget(_takedown.Range) && _takedown.IsReady())
                {
                    _takedown.Cast();
                }

                if (_menu.Item("jcUseCougarW").GetValue<bool>() && allMinionsW.Count > 0 &&
                    allMinionsW[0].IsValidTarget(_pounce.Range) && _pounce.IsReady())
                {
                    _pounce.Cast(allMinionsW[0]);
                }

                if (_menu.Item("jcUseCougarE").GetValue<bool>() && allMinionsE.Count > 0 &&
                    allMinionsE[0].IsValidTarget(_swipe.Range) && _swipe.IsReady())
                {
                    _swipe.Cast(allMinionsE[0]);
                }
            }
            else
            {
                if (_menu.Item("jcUseHumanQ").GetValue<bool>() && allMinionsQ2.Count > 0 &&
                    allMinionsQ2[0].IsValidTarget(_javelinToss.Range) && _javelinToss.IsReady())
                {
                    _javelinToss.Cast(allMinionsQ2[0]);
                }

                if (_menu.Item("jcUseHumanW").GetValue<bool>() && allMinionsW2.Count > 0 &&
                    allMinionsW2[0].IsValidTarget(_bushwhack.Range) && _bushwhack.IsReady())
                {
                    _bushwhack.Cast(allMinionsW2[0]);
                }
            }
        }

        #endregion

        #region Flee Credits to Hellsing.

        private static void Flee()
        {
            if (!_player.IsCougar() && _aspectOfTheCougar.IsReady() && _pounce.IsReady())
            {
                _aspectOfTheCougar.Cast();
            }
            // We need to define a new move position since jumping over walls
            // requires you to be close to the specified wall. Therefore we set the move
            // point to be that specific piont. People will need to get used to it,
            // but this is how it works.
            var wallCheck = VectorHelper.GetFirstWallPoint(_player.Position, Game.CursorPos);
            // Be more precise
            if (wallCheck != null)
            {
                wallCheck = VectorHelper.GetFirstWallPoint((Vector3) wallCheck, Game.CursorPos, 5);
            }
            // Define more position point
            var movePosition = wallCheck != null ? (Vector3) wallCheck : Game.CursorPos;
            // Update fleeTargetPosition
            var tempGrid = NavMesh.WorldToGrid(movePosition.X, movePosition.Y);
            _fleeTargetPosition = NavMesh.GridToWorld((short) tempGrid.X, (short) tempGrid.Y);
            // Also check if we want to AA aswell
            Obj_AI_Base target = null;
            // Reset walljump indicators
            var wallJumpPossible = false;
            // Only calculate stuff when our Q is up and there is a wall inbetween
            if (_player.IsCougar() && _pounce.IsReady() && wallCheck != null)
            {
                // Get our wall position to calculate from
                var wallPosition = movePosition;
                // Check 300 units to the cursor position in a 160 degree cone for a valid non-wall spot
                Vector2 direction = (Game.CursorPos.To2D() - wallPosition.To2D()).Normalized();
                const float maxAngle = 80;
                const float step = maxAngle / 20;
                float currentAngle = 0;
                float currentStep = 0;
                bool jumpTriggered = false;
                while (true)
                {
                    // Validate the counter, break if no valid spot was found in previous loops
                    if (currentStep > maxAngle && currentAngle < 0)
                    {
                        break;
                    }
                    // Check next angle
                    if ((Math.Abs(currentAngle) < 0.001 || currentAngle < 0) && Math.Abs(currentStep) > 0.000)
                    {
                        currentAngle = (currentStep) * (float) Math.PI / 180;
                        currentStep += step;
                    }
                    else if (currentAngle > 0)
                    {
                        currentAngle = -currentAngle;
                    }
                    Vector3 checkPoint;
                    // One time only check for direct line of sight without rotating
                    if (Math.Abs(currentStep) < 0.001)
                    {
                        currentStep = step;
                        checkPoint = wallPosition + _pounce.Range * direction.To3D();
                    }
                    // Rotated check
                    else
                    {
                        checkPoint = wallPosition + _pounce.Range * direction.Rotated(currentAngle).To3D();
                    }
                    // Check if the point is not a wall
                    if (!checkPoint.IsWall())
                    {
                        // Check if there is a wall between the checkPoint and wallPosition
                        wallCheck = VectorHelper.GetFirstWallPoint(checkPoint, wallPosition);
                        if (wallCheck != null)
                        {
                            // There is a wall inbetween, get the closes point to the wall, as precise as possible
                            Vector2? firstWallPoint = VectorHelper.GetFirstWallPoint(
                                (Vector3) wallCheck, wallPosition, 5);
                            if (firstWallPoint != null)
                            {
                                Vector3 wallPositionOpposite = (Vector3) firstWallPoint;
                                // Check if it's worth to jump considering the path length
                                if (_player.GetPath(wallPositionOpposite).ToList().To2D().PathLength() -
                                    _player.Distance(wallPositionOpposite) > 200)
                                {
                                    // Check the distance to the opposite side of the wall
                                    if (_player.Distance(wallPositionOpposite, true) <
                                        Math.Pow(375 - _player.BoundingRadius / 2, 2))
                                    {
                                        _pounce.Cast(wallPositionOpposite);
                                        jumpTriggered = true;
                                    }
                                    else
                                    {
                                        wallJumpPossible = true;
                                    }
                                }
                                else
                                {
                                    Render.Circle.DrawCircle(Game.CursorPos, 35, Color.Red, 2);
                                }
                            }
                        }
                    }
                }
                // Check if the loop triggered the jump, if not just orbwalk
                if (!jumpTriggered)
                {
                    Orbwalking.Orbwalk(null, Game.CursorPos, 90f, 0f, false, false);
                }
            }
            // Either no wall or W on cooldown, just move towards to wall then
            else
            {
                Orbwalking.Orbwalk(null, Game.CursorPos, 90f, 0f, false, false);
                if (_player.IsCougar() && _pounce.IsReady())
                {
                    _pounce.Cast(Game.CursorPos);
                }
            }
        }

        #endregion

        #region KillSteal

        private static void Killsteal()
        {
            if (_menu.Item("killstealUseQ").GetValue<bool>())
            {
                foreach (var pred in
                    from target in
                        ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(_javelinToss.Range))
                    let prediction = _javelinToss.GetPrediction(target)
                    let javelinDamage = GetActualSpearDamage(target)
                    where target.Health <= javelinDamage && prediction.Hitchance >= CustomHitChance
                    select prediction)
                {
                    _javelinToss.Cast(pred.CastPosition);
                }
            }
            //TODO temp disabled SwitchKillsteal();
        }

        #endregion

        #region CreateMenu

        private static void CreateMenu()
        {
            _menu = new Menu(ChampionName + " the Bestial Huntress", ChampionName + " the bestial huntress", true);

            var targetSelectorMenu = new Menu("Target Selector", "ts");
            _menu.AddSubMenu(targetSelectorMenu);
            TargetSelector.AddToMenu(targetSelectorMenu);

            var orbwalkingMenu = new Menu("Orbwalking", "orbwalk");
            _menu.AddSubMenu(orbwalkingMenu);
            _orbwalker = new Orbwalking.Orbwalker(orbwalkingMenu);

            var keybindings = new Menu("Key Bindings", "keybindings");
            {
                keybindings.AddItem(new MenuItem("useCombo", "Combo").SetValue(new KeyBind(32, KeyBindType.Press)));
                keybindings.AddItem(new MenuItem("useHarass", "Harass").SetValue(new KeyBind('C', KeyBindType.Press)));
                keybindings.AddItem(new MenuItem("useWC", "Waveclear").SetValue(new KeyBind('V', KeyBindType.Press)));
                keybindings.AddItem(new MenuItem("useJC", "Jungleclear").SetValue(new KeyBind('V', KeyBindType.Press)));
                keybindings.AddItem(new MenuItem("useFlee", "Flee").SetValue(new KeyBind('G', KeyBindType.Press)));
                _menu.AddSubMenu(keybindings);
            }

            var combo = new Menu("Combo Options", "combo");
            {
                var humanMenu = new Menu("Human Spells", "human");
                {
                    humanMenu.AddItem(new MenuItem("useJavelin", "Use Javelin (Q)").SetValue(true));
                    humanMenu.AddItem(new MenuItem("useBushwhack", "Use Bushwhack (W)").SetValue(false));
                    humanMenu.AddItem(new MenuItem("useCougar", "Auto Transform to Cougar").SetValue(true));
                    combo.AddSubMenu(humanMenu);
                }
                var cougarMenu = new Menu("Cougar Spells", "cougar");
                {
                    cougarMenu.AddItem(new MenuItem("useTakedown", "Use Takedown (Q)").SetValue(true));
                    cougarMenu.AddItem(new MenuItem("usePounce", "Use Pounce (W)").SetValue(true));
                    cougarMenu.AddItem(new MenuItem("useSwipe", "Use Swipe (E)").SetValue(true));
                    cougarMenu.AddItem(new MenuItem("useHuman", "Auto Transform to Human").SetValue(true));
                    combo.AddSubMenu(cougarMenu);
                }
                _menu.AddSubMenu(combo);
            }

            var harass = new Menu("Harass Options", "harass");
            {
                harass.AddItem(new MenuItem("useJavelinHarass", "Use Javelin (Q)").SetValue(true));
                _menu.AddSubMenu(harass);
            }

            var waveclear = new Menu("Waveclear Options", "waveclear");
            {
                waveclear.AddItem(new MenuItem("wcUseHumanQ", "Use Javelin Toss").SetValue(false));
                waveclear.AddItem(new MenuItem("wcUseHumanW", "Use Bushwhack").SetValue(false));
                waveclear.AddItem(new MenuItem("wcUseCougarQ", "Use Takedown").SetValue(true));
                waveclear.AddItem(new MenuItem("wcUseCougarW", "Use Pounce").SetValue(true));
                waveclear.AddItem(new MenuItem("wcUseCougarE", "Use Swipe").SetValue(true));
                _menu.AddSubMenu(waveclear);
            }

            var jungleclear = new Menu("Jungleclear Options", "jungleclear");
            {
                jungleclear.AddItem(new MenuItem("jcUseHumanQ", "Use Javelin Toss").SetValue(false));
                jungleclear.AddItem(new MenuItem("jcUseHumanW", "Use Bushwhack").SetValue(false));
                jungleclear.AddItem(new MenuItem("jcUseCougarQ", "Use Takedown").SetValue(true));
                jungleclear.AddItem(new MenuItem("jcUseCougarW", "Use Pounce").SetValue(true));
                jungleclear.AddItem(new MenuItem("jcUseCougarE", "Use Swipe").SetValue(true));
                jungleclear.AddItem(new MenuItem("jcMana", "Mana to Jungleclear").SetValue(new Slider(40, 100, 0)));
                _menu.AddSubMenu(jungleclear);
            }

            var killsteal = new Menu("Killsteal Options", "killsteal");
            {
                killsteal.AddItem(new MenuItem("killstealUseQ", "Use Javelin (Q)").SetValue(true));
                killsteal.AddItem(new MenuItem("killstealSwitchForm", "Switch form").SetValue(true));
                _menu.AddSubMenu(killsteal);
            }

            _manaManager.AddToMenu(ref _menu);
            _healManager.AddToMenu(ref _menu);

            var misc = new Menu("Misc Options", "misc");
            {
                misc.AddItem(new MenuItem("miscIgnite", "Use Ignite").SetValue(true));
                misc.AddItem(new MenuItem("miscImmobile", "Use Javelin / Bushwhack on immobile").SetValue(true));
                misc.AddItem(
                    new MenuItem("hitChanceSetting", "Hitchance").SetValue(
                        new StringList(new[] { "Low", "Medium", "High", "Very High" }, 3)));
                misc.AddItem(new MenuItem("turretSafety", "Don't use pounce under turret").SetValue(true));
                _menu.AddSubMenu(misc);
            }

            var drawings = new Menu("Drawing Options", "drawings");
            {
                drawings.AddItem(new MenuItem("drawRangeQ", "Q range").SetValue(new Circle(true, Color.Aquamarine)));
                drawings.AddItem(new MenuItem("drawRangeW", "W range").SetValue(new Circle(false, Color.Aquamarine)));
                drawings.AddItem(new MenuItem("drawRangeE", "E range").SetValue(new Circle(false, Color.Aquamarine)));
                drawings.AddItem(
                    new MenuItem("drawDamage", "Draw Spell Damage").SetValue(new Circle(false, Color.GreenYellow)));
                _menu.AddSubMenu(drawings);
            }
            _menu.AddToMainMenu();
        }

        #endregion

        #region Notifications Credits to Beaving.

        public static Notification ShowNotification(string message, Color color, int duration = -1, bool dispose = true)
        {
            var notify = new Notification(message).SetTextColor(color);
            Notifications.AddNotification(notify);
            if (dispose)
            {
                Utility.DelayAction.Add(duration, () => notify.Dispose());
            }
            return notify;
        }

        #endregion

        #region spear calculation

        private static float GetActualSpearDamage(Obj_AI_Hero target)
        {
            double baseDamage = new double[] { 50, 75, 100, 125, 150 }[_javelinToss.Level - 1] +
                                0.4 * _player.FlatMagicDamageMod;
            float increasedDamageFactor = 1f;
            float distance = _player.Distance(target);
            if (distance > 525)
            {
                if (distance > 1300)
                {
                    distance = 1300;
                }
                float delta = distance - 525;
                increasedDamageFactor = delta / 7.75f * 0.02f;
            }
            return (float) (baseDamage * increasedDamageFactor);
        }

        #endregion

        #region OnImmobile

        private static void OnImmobile()
        {
            if (_menu.Item("miscImmobile").GetValue<bool>() && !_player.IsCougar())
            {
                foreach (var pred in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(hero => hero.IsValidTarget() && hero.Distance(_player.Position) <= _javelinToss.Range)
                        .Select(target => _javelinToss.GetPrediction(target))
                        .Where(pred => pred.Hitchance == HitChance.Immobile))
                {
                    _javelinToss.Cast(pred.CastPosition);
                }

                foreach (var pred in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(hero => hero.IsValidTarget() && hero.Distance(_player.Position) <= _bushwhack.Range)
                        .Select(target => _bushwhack.GetPrediction(target))
                        .Where(pred => pred.Hitchance == HitChance.Immobile))
                {
                    _bushwhack.Cast(pred.CastPosition);
                }
            }
        }

        #endregion

        #region IsUnderEnemyTurret

        private static bool IsUnderEnemyTurret(Obj_AI_Base unit)
        {
            IEnumerable<Obj_AI_Turret> turrets;
            if (unit.IsEnemy)
            {
                turrets =
                    ObjectManager.Get<Obj_AI_Turret>()
                        .Where(
                            x =>
                                x.IsAlly && x.IsValid && !x.IsDead &&
                                unit.ServerPosition.Distance(x.ServerPosition) < x.AttackRange);
            }
            else
            {
                turrets =
                    ObjectManager.Get<Obj_AI_Turret>()
                        .Where(
                            x =>
                                x.IsEnemy && x.IsValid && !x.IsDead &&
                                unit.ServerPosition.Distance(x.ServerPosition) < x.AttackRange);
            }
            return (turrets.Any());
        }

        #endregion

        #region OnGameLoad

        private static void Game_OnGameLoad(EventArgs args)
        {
            _player = ObjectManager.Player;
            if (_player.ChampionName != ChampionName)
            {
                return;
            }

            DamageIndicator.Initialize(GetComboDamage);
            DamageIndicator.Enabled = true;
            DamageIndicator.DrawingColor = Color.Green;

            _javelinToss = new Spell(SpellSlot.Q, 1500f);
            _takedown = new Spell(SpellSlot.Q, 200f);
            _bushwhack = new Spell(SpellSlot.W, 900f);
            _pounce = new Spell(SpellSlot.W, 375f);
            _primalSurge = new Spell(SpellSlot.E, 600f);
            _swipe = new Spell(SpellSlot.E, 300f);
            _aspectOfTheCougar = new Spell(SpellSlot.R);

            HumanSpellList.AddRange(new[] { _javelinToss, _bushwhack, _primalSurge });
            CougarSpellList.AddRange(new[] { _takedown, _pounce, _swipe });

            var igniteslot = _player.Spellbook.GetSpell(ObjectManager.Player.GetSpellSlot("summonerdot")).Slot;
            if (igniteslot != SpellSlot.Unknown)
            {
                Ignite = new Spell(igniteslot, 600);
            }

            _javelinToss.SetSkillshot(0.125f, 40f, 1300f, true, SkillshotType.SkillshotLine);
            _bushwhack.SetSkillshot(0.50f, 100f, 1500f, false, SkillshotType.SkillshotCircle);
            _swipe.SetSkillshot(0.50f, 375f, 1500f, false, SkillshotType.SkillshotCone);
            _pounce.SetSkillshot(0.50f, 400f, 1500f, false, SkillshotType.SkillshotCone);

            _healManager = new HealManager();
            _manaManager = new ManaManager();

            CreateMenu();

            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalking.AfterAttack += AfterAttack;

            ShowNotification("Nidalee by blacky & iJabba", Color.Crimson, 4000);
            ShowNotification("Heal & ManaManager by iJabba", Color.Crimson, 4000);
        }

        private static void AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            //TODO
        }

        #endregion

        #region calculations

        public static bool IsCougar(this Obj_AI_Hero player)
        {
            return player.Spellbook.GetSpell(SpellSlot.Q).Name == "Takedown";
        }

        public static bool IsHunted(this Obj_AI_Hero target)
        {
            return target.HasBuff("nidaleepassivehunted");
        }

        private static HitChance GetHitchance()
        {
            switch (_menu.Item("hitChanceSetting").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    return HitChance.Low;
                case 1:
                    return HitChance.Medium;
                case 2:
                    return HitChance.High;
                case 3:
                    return HitChance.VeryHigh;
                default:
                    return HitChance.Medium;
            }
        }

        private static float GetComboDamage(Obj_AI_Hero target)
        {
            double damage = 0d;

            //TODO ignite.

            if (_player.IsCougar())
            {
                if (_takedown.IsReady())
                {
                    if (target.IsHunted())
                    {
                        damage += _player.GetSpellDamage(target, SpellSlot.Q) * 0.33f;
                    }
                    else
                    {
                        damage += _player.GetSpellDamage(target, SpellSlot.Q);
                    }
                }
                if (_pounce.IsReady())
                {
                    damage += _player.GetSpellDamage(target, SpellSlot.W);
                }
                if (_swipe.IsReady())
                {
                    damage += _player.GetSpellDamage(target, SpellSlot.E);
                }
            }
            else
            {
                if (_javelinToss.IsReady())
                {
                    damage += GetActualSpearDamage(target);
                }
            }

            return (float) damage;
        }


        private static void SwitchKillsteal()
        {
            Obj_AI_Hero target = TargetSelector.GetTarget(_javelinToss.Range, TargetSelector.DamageType.Magical);

            if (target == null || !_menu.Item("killstealSwitchForm").GetValue<bool>())
            {
                return;
            }

            float pounceDistance = target.IsHunted() ? 740 : 400;

            if (_player.IsCougar() && _player.Distance(target.Position) > pounceDistance &&
                _player.Distance(target.Position) < _javelinToss.Range && (GetActualSpearDamage(target) > target.Health))
                // TODO add a hardcoded getDamage when in cougar form.
            {
                if (_pounce.IsReady())
                {
                    _pounce.Cast(Game.CursorPos);
                }
                if (_aspectOfTheCougar.IsReady())
                {
                    _aspectOfTheCougar.Cast();
                }
                if (_javelinToss.GetPrediction(target).Hitchance >= HitChance.Medium &&
                    _javelinToss.GetPrediction(target).Hitchance != HitChance.Collision) {}
                {
                    _javelinToss.Cast(target);
                }
            }
        }

        #endregion
    }
}
