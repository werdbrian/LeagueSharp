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

using LeagueSharp;

namespace NidaleeTheBestialHuntress
{
    internal class CooldownTracker
    {
        private readonly Obj_AI_Hero _player;
        private readonly float[] bushwhackCooldown = { 13, 12, 11, 10, 9 };
        private const float JavelinCooldown = 6;
        private const float PrimalsurgeCooldown = 12;
        private float javelinTime, bushwhackTime, primalSurgeTime;
        private float takedownTime, pounceTime, swipeTime;

        public CooldownTracker()
        {
            _player = ObjectManager.Player;
            Obj_AI_Base.OnProcessSpellCast += OnSpellCast;
        }

        public bool CheckSpell(CooldownSpell spell)
        {
            switch (spell)
            {
                case CooldownSpell.Javelin:
                    return (javelinTime - Game.Time) <= 0;
                case CooldownSpell.Bushwhack:
                    return (bushwhackTime - Game.Time) <= 0;
                case CooldownSpell.PrimalSurge:
                    return (primalSurgeTime - Game.Time) <= 0;
                case CooldownSpell.Takedown:
                    return (takedownTime - Game.Time) <= 0;
                case CooldownSpell.Pounce:
                    return (pounceTime - Game.Time) <= 0;
                case CooldownSpell.Swipe:
                    return (swipeTime - Game.Time) <= 0;
                default:
                    return false;
            }
        }

        private void OnSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe)
            {
                return;
            }
            switch (args.SData.Name)
            {
                case "JavelinToss":
                    javelinTime = Game.Time + JavelinCooldown + (JavelinCooldown * _player._PercentCooldownMod);
                    break;
                case "Bushwhack":
                    float bushCd = bushwhackCooldown[_player.Spellbook.GetSpell(SpellSlot.W).Level - 1];
                    bushwhackTime = Game.Time + bushCd + (bushCd * _player.PercentCooldownMod);
                    break;
                case "PrimalSurge":
                    primalSurgeTime = Game.Time + PrimalsurgeCooldown +
                                      (PrimalsurgeCooldown * _player.PercentCooldownMod);
                    break;
                case "Takedown":
                    takedownTime = Game.Time + 5 + (5 * _player.PercentCooldownMod);
                    break;
                case "Pounce":
                    pounceTime = Game.Time + 5 + (5 * _player.PercentCooldownMod);
                    break;
                case "Swipe":
                    swipeTime = Game.Time + 5 + (5 * _player.PercentCooldownMod);
                    break;
            }
        }

        internal enum CooldownSpell
        {
            Javelin,
            Bushwhack,
            PrimalSurge,
            Takedown,
            Pounce,
            Swipe
        }
    }
}