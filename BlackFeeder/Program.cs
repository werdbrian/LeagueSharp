using System;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace BlackFeeder
{
    internal class Program
    {
        // Generic
        private static readonly Obj_AI_Hero Player = ObjectManager.Player;
        // Shit
        private static readonly bool[] BoughtItems = { false, false, false, false };
        private static string[] _deaths;
        private static double _timeDead;
        private static int _deathCounter;
        private static int _lastLaugh;
        private static double _lastTouchdown;
        private static SpellSlot _ghostSlot, _reviveSlot;
        private static readonly Vector3 PurpleSpawn = new Vector3(14286f, 14382f, 172f);
        private static readonly Vector3 BlueSpawn = new Vector3(416f, 468f, 182f);
        // Menu
        public static Menu Menu;

        public static void Main(string[] args)
        {
            // Register events
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            // Create menu
            CreateMenu();

            // Shit
            _deaths = new[]
            {
                "XD", "kek", "sorry lag", "gg", "help pls", "nooob wtf", "team???", "gg my team sucks",
                "matchmaking sucks", "i can't carry dis", "wtf how?", "wow rito nerf pls", "report enemys for drophacks",
                "tilidin y u do dis", "kappa"
            };
            _ghostSlot = Player.GetSpellSlot("SummonerHaste");
            _reviveSlot = Player.GetSpellSlot("SummonerRevive");

            // Register events
            Game.OnUpdate += Game_OnGameUpdate;
            Game.OnEnd += OnGameEnd;

            // Print
            Game.PrintChat(
                String.Format("<font color='#08F5F8'>blacky -</font> <font color='#FFFFFF'>BlackFeeder Loaded!</font>"));
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Menu.Item("feedingActive").GetValue<bool>())
            {
                Feederino();
            }
        }

        private static void Feederino()
        {
            if (Player.Team == GameObjectTeam.Order)
            {
                Player.IssueOrder(GameObjectOrder.MoveTo, PurpleSpawn);
            }

            if (Player.Team == GameObjectTeam.Chaos)
            {
                Player.IssueOrder(GameObjectOrder.MoveTo, BlueSpawn);
            }

            if (Menu.Item("useSkillsActive").GetValue<bool>())
            {
                FeedSpells();
            }

            if (Menu.Item("sayShitActive").GetValue<bool>())
            {
                SayShit();
            }

            if (Menu.Item("laughingActive").GetValue<bool>())
            {
                Laughing();
            }

            if (Menu.Item("buyItemsActive").GetValue<bool>())
            {
                BuyItems();
            }
        }

        private static void FeedSpells()
        {
            if (_ghostSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(_ghostSlot) == SpellState.Ready)
            {
                if (Player.Distance(PurpleSpawn) < 600 | Player.Distance(BlueSpawn) < 600)
                {
                    return;
                }

                Player.Spellbook.CastSpell(_ghostSlot);
            }

            if (_reviveSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(_reviveSlot) == SpellState.Ready)
            {
                if (Player.Distance(PurpleSpawn) < 600 | Player.Distance(BlueSpawn) < 600)
                {
                    return;
                }

                Player.Spellbook.CastSpell(_reviveSlot);
            }

            if (Player.ChampionName == "Blitzcrank")
            {
                Player.Spellbook.LevelSpell(SpellSlot.W);
                if (Player.Spellbook.CanUseSpell(SpellSlot.W) == SpellState.Ready)
                {
                    if (Player.Distance(PurpleSpawn) < 600 | Player.Distance(BlueSpawn) < 600)
                    {
                        return;
                    }

                    Player.Spellbook.CastSpell(SpellSlot.W, Player);
                }
            }

            if (Player.ChampionName == "Evelynn")
            {
                Player.Spellbook.LevelSpell(SpellSlot.W);
                if (Player.Spellbook.CanUseSpell(SpellSlot.W) == SpellState.Ready)
                {
                    if (Player.Distance(PurpleSpawn) < 600 | Player.Distance(BlueSpawn) < 600)
                    {
                        return;
                    }

                    Player.Spellbook.CastSpell(SpellSlot.W, Player);
                }
            }

            if (Player.ChampionName == "Gangplank")
            {
                Player.Spellbook.LevelSpell(SpellSlot.E);
                if (Player.Spellbook.CanUseSpell(SpellSlot.E) == SpellState.Ready)
                {
                    if (Player.Distance(PurpleSpawn) < 600 | Player.Distance(BlueSpawn) < 600)
                    {
                        return;
                    }

                    Player.Spellbook.CastSpell(SpellSlot.E, Player);
                }
            }

            if (Player.ChampionName == "Garen")
            {
                Player.Spellbook.LevelSpell(SpellSlot.Q);
                if (Player.Spellbook.CanUseSpell(SpellSlot.Q) == SpellState.Ready)
                {
                    if (Player.Distance(PurpleSpawn) < 600 | Player.Distance(BlueSpawn) < 600)
                    {
                        return;
                    }

                    Player.Spellbook.CastSpell(SpellSlot.Q, Player);
                }
            }

            if (Player.ChampionName == "Karma")
            {
                Player.Spellbook.LevelSpell(SpellSlot.E);
                if (Player.Spellbook.CanUseSpell(SpellSlot.E) == SpellState.Ready)
                {
                    if (Player.Distance(PurpleSpawn) < 600 | Player.Distance(BlueSpawn) < 600)
                    {
                        return;
                    }

                    Player.Spellbook.CastSpell(SpellSlot.E, Player);
                }
            }

            if (Player.ChampionName == "Kayle")
            {
                Player.Spellbook.LevelSpell(SpellSlot.W);
                if (Player.Spellbook.CanUseSpell(SpellSlot.W) == SpellState.Ready)
                {
                    if (Player.Distance(PurpleSpawn) < 600 | Player.Distance(BlueSpawn) < 600)
                    {
                        return;
                    }

                    Player.Spellbook.CastSpell(SpellSlot.W, Player);
                }
            }

            if (Player.ChampionName == "Kennen")
            {
                Player.Spellbook.LevelSpell(SpellSlot.E);
                if (Player.Spellbook.CanUseSpell(SpellSlot.E) == SpellState.Ready)
                {
                    if (Player.Distance(PurpleSpawn) < 600 | Player.Distance(BlueSpawn) < 600)
                    {
                        return;
                    }

                    Player.Spellbook.CastSpell(SpellSlot.E, Player);
                }
            }

            if (Player.ChampionName == "Lulu")
            {
                Player.Spellbook.LevelSpell(SpellSlot.W);
                if (Player.Spellbook.CanUseSpell(SpellSlot.W) == SpellState.Ready)
                {
                    if (Player.Distance(PurpleSpawn) < 600 | Player.Distance(BlueSpawn) < 600)
                    {
                        return;
                    }

                    Player.Spellbook.CastSpell(SpellSlot.W, Player);
                }
            }

            if (Player.ChampionName == "MasterYi")
            {
                Player.Spellbook.LevelSpell(SpellSlot.R);
                if (Player.Spellbook.CanUseSpell(SpellSlot.R) == SpellState.Ready)
                {
                    if (Player.Distance(PurpleSpawn) < 600 | Player.Distance(BlueSpawn) < 600)
                    {
                        return;
                    }

                    Player.Spellbook.CastSpell(SpellSlot.R, Player);
                }
            }

            if (Player.ChampionName == "Nunu")
            {
                Player.Spellbook.LevelSpell(SpellSlot.W);
                if (Player.Spellbook.CanUseSpell(SpellSlot.W) == SpellState.Ready)
                {
                    if (Player.Distance(PurpleSpawn) < 600 | Player.Distance(BlueSpawn) < 600)
                    {
                        return;
                    }

                    Player.Spellbook.CastSpell(SpellSlot.W, Player);
                }
            }

            if (Player.ChampionName == "Poppy")
            {
                Player.Spellbook.LevelSpell(SpellSlot.W);
                if (Player.Spellbook.CanUseSpell(SpellSlot.W) == SpellState.Ready)
                {
                    if (Player.Distance(PurpleSpawn) < 600 | Player.Distance(BlueSpawn) < 600)
                    {
                        return;
                    }

                    Player.Spellbook.CastSpell(SpellSlot.W, Player);
                }
            }

            if (Player.ChampionName == "Rammus")
            {
                Player.Spellbook.LevelSpell(SpellSlot.Q);
                if (Player.Spellbook.CanUseSpell(SpellSlot.Q) == SpellState.Ready && !Player.HasBuff("PowerBall"))
                {
                    if (Player.Distance(PurpleSpawn) < 600 | Player.Distance(BlueSpawn) < 600)
                    {
                        return;
                    }

                    Player.Spellbook.CastSpell(SpellSlot.Q, Player);
                }
            }

            if (Player.ChampionName == "Ryze")
            {
                Player.Spellbook.LevelSpell(SpellSlot.R);
                if (Player.Spellbook.CanUseSpell(SpellSlot.R) == SpellState.Ready)
                {
                    if (Player.Distance(PurpleSpawn) < 600 | Player.Distance(BlueSpawn) < 600)
                    {
                        return;
                    }

                    Player.Spellbook.CastSpell(SpellSlot.R, Player);
                }
            }

            if (Player.ChampionName == "Shyvana")
            {
                Player.Spellbook.LevelSpell(SpellSlot.W);
                if (Player.Spellbook.CanUseSpell(SpellSlot.W) == SpellState.Ready)
                {
                    if (Player.Distance(PurpleSpawn) < 600 | Player.Distance(BlueSpawn) < 600)
                    {
                        return;
                    }

                    Player.Spellbook.CastSpell(SpellSlot.W, Player);
                }
            }

            if (Player.ChampionName == "Sivir")
            {
                Player.Spellbook.LevelSpell(SpellSlot.R);
                if (Player.Spellbook.CanUseSpell(SpellSlot.R) == SpellState.Ready)
                {
                    if (Player.Distance(PurpleSpawn) < 600 | Player.Distance(BlueSpawn) < 600)
                    {
                        return;
                    }

                    Player.Spellbook.CastSpell(SpellSlot.R, Player);
                }
            }

            if (Player.ChampionName == "Sona")
            {
                Player.Spellbook.LevelSpell(SpellSlot.E);
                if (Player.Spellbook.CanUseSpell(SpellSlot.E) == SpellState.Ready)
                {
                    if (Player.Distance(PurpleSpawn) < 600 | Player.Distance(BlueSpawn) < 600)
                    {
                        return;
                    }

                    Player.Spellbook.CastSpell(SpellSlot.E, Player);
                }
            }

            if (Player.ChampionName == "Teemo")
            {
                Player.Spellbook.LevelSpell(SpellSlot.W);
                if (Player.Spellbook.CanUseSpell(SpellSlot.W) == SpellState.Ready)
                {
                    if (Player.Distance(PurpleSpawn) < 600 | Player.Distance(BlueSpawn) < 600)
                    {
                        return;
                    }

                    Player.Spellbook.CastSpell(SpellSlot.W, Player);
                }
            }

            if (Player.ChampionName == "Volibear")
            {
                Player.Spellbook.LevelSpell(SpellSlot.Q);
                if (Player.Spellbook.CanUseSpell(SpellSlot.Q) == SpellState.Ready)
                {
                    if (Player.Distance(PurpleSpawn) < 600 | Player.Distance(BlueSpawn) < 600)
                    {
                        return;
                    }

                    Player.Spellbook.CastSpell(SpellSlot.Q, Player);
                }
            }

            if (Player.ChampionName == "Zilean")
            {
                Player.Spellbook.LevelSpell(SpellSlot.E);
                Player.Spellbook.LevelSpell(SpellSlot.W);
                if (Player.Spellbook.CanUseSpell(SpellSlot.E) == SpellState.Ready)
                {
                    if (Player.Distance(PurpleSpawn) < 600 | Player.Distance(BlueSpawn) < 600)
                    {
                        return;
                    }

                    Player.Spellbook.CastSpell(SpellSlot.E, Player);
                }

                if (Player.Spellbook.CanUseSpell(SpellSlot.W) == SpellState.Ready &&
                    Player.Spellbook.CanUseSpell(SpellSlot.E) != SpellState.Ready)
                {
                    if (Player.Distance(PurpleSpawn) < 600 | Player.Distance(BlueSpawn) < 600)
                    {
                        return;
                    }

                    Player.Spellbook.CastSpell(SpellSlot.W, Player);
                }
            }
        }

        private static void SayShit()
        {
            if (Player.IsDead && Game.Time - _timeDead > 80)
            {
                Game.Say(_deaths[_deathCounter]);
                _deathCounter++;
                _timeDead = Game.Time;
            }

            if (Player.Team == GameObjectTeam.Chaos && Player.Distance(BlueSpawn) < 600)
            {
                if (Game.Time - _lastTouchdown > 80)
                {
                    Game.Say("/all TOUCHDOWN!");
                    _lastTouchdown = Game.Time;
                }
            }

            if (Player.Team == GameObjectTeam.Order && Player.Distance(PurpleSpawn) < 600)
            {
                if (Game.Time - _lastTouchdown > 80)
                {
                    Game.Say("/all TOUCHDOWN!");
                    _lastTouchdown = Game.Time;
                }
            }
        }

        private static void Laughing()
        {
            if (Environment.TickCount <= _lastLaugh + 2500)
            {
                return;
            }

            //Packet.C2S.Emote.Encoded(new Packet.C2S.Emote.Struct((byte) Packet.Emotes.Laugh)).Send();
            Game.Say("/l");
            _lastLaugh = Environment.TickCount;
        }

        private static void BuyItems()
        {
            if (Player.InFountain() && Player.Gold > 325 && !BoughtItems[0])
            {
                //Packet.C2S.BuyItem.Encoded(new Packet.C2S.BuyItem.Struct(1001)).Send();
                Player.BuyItem(ItemId.Boots_of_Speed);
                BoughtItems[0] = true;
            }

            if (Player.InShop() && Player.Gold > 475 && BoughtItems[0] && !BoughtItems[1])
            {
                //Packet.C2S.BuyItem.Encoded(new Packet.C2S.BuyItem.Struct(3117)).Send();
                Player.BuyItem(ItemId.Boots_of_Mobility);
                BoughtItems[1] = true;
            }

            if (Player.InShop() && Player.Gold > 475 && BoughtItems[1] && !BoughtItems[2])
            {
                //Packet.C2S.BuyItem.Encoded(new Packet.C2S.BuyItem.Struct(3270)).Send();
                Player.BuyItem(ItemId.Boots_of_Mobility_Enchantment_Homeguard);
                BoughtItems[2] = true;
            }

            if (Player.InShop() && Player.Gold > 950 && BoughtItems[2] && !BoughtItems[3])
            {
                //Packet.C2S.BuyItem.Encoded(new Packet.C2S.BuyItem.Struct(3086)).Send();
                Player.BuyItem(ItemId.Aether_Wisp);
                BoughtItems[3] = true;
            }

            if (Player.InShop() && Player.Gold > 1100 && BoughtItems[3])
            {
                Player.BuyItem(ItemId.Zeal);
            }
        }

        private static void OnGameEnd(EventArgs args)
        {
            Game.Say("/all Good game guys, well played.");
        }

        private static void CreateMenu()
        {
            Menu = new Menu("BlackFeeder", "blackfeeder", true);

            // Feeding
            var feeder = new Menu("BlackFeeder", "blackfeeder");
            Menu.AddSubMenu(feeder);
            feeder.AddItem(new MenuItem("feedingActive", "Feeding active!").SetValue(true));
            feeder.AddItem(new MenuItem("useSkillsActive", "Use Skills to feed!").SetValue(true));
            feeder.AddItem(new MenuItem("sayShitActive", "Say stuff while feeding!").SetValue(true));
            feeder.AddItem(new MenuItem("laughingActive", "Laugh while Feed!").SetValue(true));
            feeder.AddItem(new MenuItem("buyItemsActive", "Buy Items!").SetValue(true));

            // Finalizing
            Menu.AddToMainMenu();
        }
    }
}