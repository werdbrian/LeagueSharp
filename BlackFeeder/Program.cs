using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace BlackFeeder
{
    class Program
    {
        // Generic
        private static readonly Obj_AI_Hero player = ObjectManager.Player;

        // Shit
        private static bool boughtItemOne, boughtItemTwo, boughtItemThree = false;
        private static string[] deaths;
        private static double timedead;
        private static int deathcounter;
        private static int LastLaugh;
        private static double LastTouchdown;
        private static SpellSlot GhostSlot, ReviveSlot;
        private static Vector3 PurpleSpawn = new Vector3(14286f, 14382f, 172f);
        private static Vector3 BlueSpawn = new Vector3(416f, 468f, 182f);

        // Menu
        public static Menu menu;

        public static void Main(string[] args)
        {
            // Register events
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            // Create menu
            createMenu();

            // Shit
            deaths = new[] { "XD", "kek", "sorry lag", "gg", "help pls", "nooob wtf", "team???", "gg my team sucks", "matchmaking sucks", "i can't carry dis", "wtf how?", "wow rito nerf pls", "report enemys for drophacks", "tilidin y u do dis", "kappa"};
            GhostSlot = player.GetSpellSlot("SummonerHaste");
            ReviveSlot = player.GetSpellSlot("SummonerRevive");

            // Register events
            Game.OnGameUpdate += Game_OnGameUpdate;
            Game.OnGameEnd += OnGameEnd;

            // Print
            Game.PrintChat(String.Format("<font color='#08F5F8'>blacky -</font> <font color='#FFFFFF'>BlackFeeder Loaded!</font>"));
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (menu.Item("feedingActive").GetValue<bool>())
                Feederino();
        }

        private static void Feederino()
        {
            if (player.Team == GameObjectTeam.Order)
            {
                player.IssueOrder(GameObjectOrder.MoveTo, PurpleSpawn);
            }

            if (player.Team == GameObjectTeam.Chaos)
            {
                player.IssueOrder(GameObjectOrder.MoveTo, BlueSpawn);
            }

            if (menu.Item("useSkillsActive").GetValue<bool>())
                FeedSpells();

            if (menu.Item("sayShitActive").GetValue<bool>())
                sayShit();

            if (menu.Item("laughingActive").GetValue<bool>())
                Laughing();

            if (menu.Item("buyItemsActive").GetValue<bool>())
                BuyItems();
        }

        private static void FeedSpells()
        {
            if (GhostSlot != SpellSlot.Unknown && player.Spellbook.CanUseSpell(GhostSlot) == SpellState.Ready)
            {
                if (player.Distance(PurpleSpawn) < 600 | player.Distance(BlueSpawn) < 600)
                {
                    return;
                }
                else
                {
                    player.Spellbook.CastSpell(GhostSlot);
                }
            }

            if (ReviveSlot != SpellSlot.Unknown && player.Spellbook.CanUseSpell(ReviveSlot) == SpellState.Ready)
            {
                if (player.Distance(PurpleSpawn) < 600 | player.Distance(BlueSpawn) < 600)
                {
                    return;
                }
                else
                {
                    player.Spellbook.CastSpell(ReviveSlot);
                }
            }

            if (player.ChampionName == "Blitzcrank")
            {
                player.Spellbook.LevelSpell(SpellSlot.W);

                if (player.Spellbook.CanUseSpell(SpellSlot.W) == SpellState.Ready)
                {
                    if (player.Distance(PurpleSpawn) < 600 | player.Distance(BlueSpawn) < 600)
                    {
                        return;
                    }
                    else
                    {
                        player.Spellbook.CastSpell(SpellSlot.W, player);
                    }
                }
            }

            if (player.ChampionName == "Evelynn")
            {
                player.Spellbook.LevelSpell(SpellSlot.W);
                if (player.Spellbook.CanUseSpell(SpellSlot.W) == SpellState.Ready)
                {
                    if (player.Distance(PurpleSpawn) < 600 | player.Distance(BlueSpawn) < 600)
                    {
                        return;
                    }
                    else
                    {
                        player.Spellbook.CastSpell(SpellSlot.W, player);
                    }
                }
            }

            if (player.ChampionName == "Gangplank")
            {
                player.Spellbook.LevelSpell(SpellSlot.E);

                if (player.Spellbook.CanUseSpell(SpellSlot.E) == SpellState.Ready)
                {
                    if (player.Distance(PurpleSpawn) < 600 | player.Distance(BlueSpawn) < 600)
                    {
                        return;
                    }
                    else
                    {
                        player.Spellbook.CastSpell(SpellSlot.E, player);
                    }
                }
            }

            if (player.ChampionName == "Garen")
            {
                player.Spellbook.LevelSpell(SpellSlot.Q);

                if (player.Spellbook.CanUseSpell(SpellSlot.Q) == SpellState.Ready)
                {
                    if (player.Distance(PurpleSpawn) < 600 | player.Distance(BlueSpawn) < 600)
                    {
                        return;
                    }
                    else
                    {
                        player.Spellbook.CastSpell(SpellSlot.Q, player);
                    }
                }
            }

            if (player.ChampionName == "Karma")
            {
                player.Spellbook.LevelSpell(SpellSlot.E);

                if (player.Spellbook.CanUseSpell(SpellSlot.E) == SpellState.Ready)
                {
                    if (player.Distance(PurpleSpawn) < 600 | player.Distance(BlueSpawn) < 600)
                    {
                        return;
                    }
                    else
                    {
                        player.Spellbook.CastSpell(SpellSlot.E, player);
                    }
                }
            }

            if (player.ChampionName == "Kayle")
            {
                player.Spellbook.LevelSpell(SpellSlot.W);

                if (player.Spellbook.CanUseSpell(SpellSlot.W) == SpellState.Ready)
                {
                    if (player.Distance(PurpleSpawn) < 600 | player.Distance(BlueSpawn) < 600)
                    {
                        return;
                    }
                    else
                    {
                        player.Spellbook.CastSpell(SpellSlot.W, player);
                    }
                }
            }

            if (player.ChampionName == "Kennen")
            {
                player.Spellbook.LevelSpell(SpellSlot.E);

                if (player.Spellbook.CanUseSpell(SpellSlot.E) == SpellState.Ready)
                {
                    if (player.Distance(PurpleSpawn) < 600 | player.Distance(BlueSpawn) < 600)
                    {
                        return;
                    }
                    else
                    {
                        player.Spellbook.CastSpell(SpellSlot.E, player);
                    }
                }
            }

            if (player.ChampionName == "Lulu")
            {
                player.Spellbook.LevelSpell(SpellSlot.W);

                if (player.Spellbook.CanUseSpell(SpellSlot.W) == SpellState.Ready)
                {
                    if (player.Distance(PurpleSpawn) < 600 | player.Distance(BlueSpawn) < 600)
                    {
                        return;
                    }
                    else
                    {
                        player.Spellbook.CastSpell(SpellSlot.W, player);
                    }
                }
            }

            if (player.ChampionName == "MasterYi")
            {
                player.Spellbook.LevelSpell(SpellSlot.R);

                if (player.Spellbook.CanUseSpell(SpellSlot.R) == SpellState.Ready)
                {
                    if (player.Distance(PurpleSpawn) < 600 | player.Distance(BlueSpawn) < 600)
                    {
                        return;
                    }
                    else
                    {
                        player.Spellbook.CastSpell(SpellSlot.R, player);
                    }
                }
            }

            if (player.ChampionName == "Nunu")
            {
                player.Spellbook.LevelSpell(SpellSlot.W);

                if (player.Spellbook.CanUseSpell(SpellSlot.W) == SpellState.Ready)
                {
                    if (player.Distance(PurpleSpawn) < 600 | player.Distance(BlueSpawn) < 600)
                    {
                        return;
                    }
                    else
                    {
                        player.Spellbook.CastSpell(SpellSlot.W, player);
                    }
                }
            }

            if (player.ChampionName == "Poppy")
            {
                player.Spellbook.LevelSpell(SpellSlot.W);

                if (player.Spellbook.CanUseSpell(SpellSlot.W) == SpellState.Ready)
                {
                    if (player.Distance(PurpleSpawn) < 600 | player.Distance(BlueSpawn) < 600)
                    {
                        return;
                    }
                    else
                    {
                        player.Spellbook.CastSpell(SpellSlot.W, player);
                    }
                }
            }

            if (player.ChampionName == "Rammus")
            {
                player.Spellbook.LevelSpell(SpellSlot.Q);

                if (player.Spellbook.CanUseSpell(SpellSlot.Q) == SpellState.Ready && !player.HasBuff("PowerBall"))
                {
                    if (player.Distance(PurpleSpawn) < 600 | player.Distance(BlueSpawn) < 600)
                    {
                        return;
                    }
                    else
                    {
                        player.Spellbook.CastSpell(SpellSlot.Q, player);
                    }
                }
            }

            if (player.ChampionName == "Ryze")
            {
                player.Spellbook.LevelSpell(SpellSlot.R);

                if (player.Spellbook.CanUseSpell(SpellSlot.R) == SpellState.Ready)
                {
                    if (player.Distance(PurpleSpawn) < 600 | player.Distance(BlueSpawn) < 600)
                    {
                        return;
                    }
                    else
                    {
                        player.Spellbook.CastSpell(SpellSlot.R, player);
                    }
                }
            }

            if (player.ChampionName == "Shyvana")
            {
                player.Spellbook.LevelSpell(SpellSlot.W);

                if (player.Spellbook.CanUseSpell(SpellSlot.W) == SpellState.Ready)
                {
                    if (player.Distance(PurpleSpawn) < 600 | player.Distance(BlueSpawn) < 600)
                    {
                        return;
                    }
                    else
                    {
                        player.Spellbook.CastSpell(SpellSlot.W, player);
                    }
                }
            }

            if (player.ChampionName == "Sivir")
            {
                player.Spellbook.LevelSpell(SpellSlot.R);

                if (player.Spellbook.CanUseSpell(SpellSlot.R) == SpellState.Ready)
                {
                    if (player.Distance(PurpleSpawn) < 600 | player.Distance(BlueSpawn) < 600)
                    {
                        return;
                    }
                    else
                    {
                        player.Spellbook.CastSpell(SpellSlot.R, player);
                    }
                }
            }

            if (player.ChampionName == "Sona")
            {
                player.Spellbook.LevelSpell(SpellSlot.E);

                if (player.Spellbook.CanUseSpell(SpellSlot.E) == SpellState.Ready)
                {
                    if (player.Distance(PurpleSpawn) < 600 | player.Distance(BlueSpawn) < 600)
                    {
                        return;
                    }
                    else
                    {
                        player.Spellbook.CastSpell(SpellSlot.E, player);
                    }
                }
            }

            if (player.ChampionName == "Teemo")
            {
                player.Spellbook.LevelSpell(SpellSlot.W);

                if (player.Spellbook.CanUseSpell(SpellSlot.W) == SpellState.Ready)
                {
                    if (player.Distance(PurpleSpawn) < 600 | player.Distance(BlueSpawn) < 600)
                    {
                        return;
                    }
                    else
                    {
                        player.Spellbook.CastSpell(SpellSlot.W, player);
                    }
                }
            }

            if (player.ChampionName == "Volibear")
            {
                player.Spellbook.LevelSpell(SpellSlot.Q);

                if (player.Spellbook.CanUseSpell(SpellSlot.Q) == SpellState.Ready)
                {
                    if (player.Distance(PurpleSpawn) < 600 | player.Distance(BlueSpawn) < 600)
                    {
                        return;
                    }
                    else
                    {
                        player.Spellbook.CastSpell(SpellSlot.Q, player);
                    }
                }
            }

            if (player.ChampionName == "Zilean")
            {
                player.Spellbook.LevelSpell(SpellSlot.E);
                player.Spellbook.LevelSpell(SpellSlot.W);

                if (player.Spellbook.CanUseSpell(SpellSlot.E) == SpellState.Ready)
                {
                    if (player.Distance(PurpleSpawn) < 600 | player.Distance(BlueSpawn) < 600)
                    {
                        return;
                    }
                    else
                    {
                        player.Spellbook.CastSpell(SpellSlot.E, player);
                    }
                }

                if (player.Spellbook.CanUseSpell(SpellSlot.W) == SpellState.Ready && player.Spellbook.CanUseSpell(SpellSlot.E) != SpellState.Ready)
                {
                    if (player.Distance(PurpleSpawn) < 600 | player.Distance(BlueSpawn) < 600)
                    {
                        return;
                    }
                    else
                    {
                        player.Spellbook.CastSpell(SpellSlot.W, player);
                    }
                }
            }
        }

        private static void sayShit()
        {
            if (player.IsDead && Game.Time - timedead > 80)
            {
                Game.Say(deaths[deathcounter]);
                deathcounter++;
                timedead = Game.Time;
            }

            if (player.Team == GameObjectTeam.Chaos && player.Distance(BlueSpawn) < 600)
            {
                if (Game.Time - LastTouchdown > 80)
                {
                    Game.Say("/all TOUCHDOWN!");
                    LastTouchdown = Game.Time;
                }
            }

            if (player.Team == GameObjectTeam.Order && player.Distance(PurpleSpawn) < 600)
            {
                if (Game.Time - LastTouchdown > 80)
                {
                    Game.Say("/all TOUCHDOWN!");
                    LastTouchdown = Game.Time;
                }
            }

        }

        private static void Laughing()
        {
            if (Environment.TickCount > LastLaugh + 2500)
            {
                Packet.C2S.Emote.Encoded(new Packet.C2S.Emote.Struct((byte)Packet.Emotes.Laugh)).Send();
                LastLaugh = Environment.TickCount;
            }
        }

        private static void BuyItems()
        {
                if (Utility.InFountain() && player.Gold > 325 && !boughtItemOne)
                {
                    //Packet.C2S.BuyItem.Encoded(new Packet.C2S.BuyItem.Struct(1001)).Send();
                    player.BuyItem(ItemId.Boots_of_Speed);
                    boughtItemOne = true;
                }

                if (Utility.InShopRange() && player.Gold > 475 && boughtItemOne && !boughtItemTwo)
                {
                    //Packet.C2S.BuyItem.Encoded(new Packet.C2S.BuyItem.Struct(3117)).Send();
                    player.BuyItem(ItemId.Boots_of_Mobility);
                    boughtItemTwo = true;
                }

                if (Utility.InShopRange() && player.Gold > 475 && boughtItemTwo && !boughtItemThree)
                {
                    //Packet.C2S.BuyItem.Encoded(new Packet.C2S.BuyItem.Struct(3270)).Send();
                    player.BuyItem(ItemId.Boots_of_Mobility_Enchantment_Homeguard);
                    boughtItemThree = true;
                }

                if (Utility.InShopRange() && player.Gold > 1100 && boughtItemThree)
                {
                    //Packet.C2S.BuyItem.Encoded(new Packet.C2S.BuyItem.Struct(3086)).Send();
                    player.BuyItem(ItemId.Zeal);
                }
        }

        private static void OnGameEnd(EventArgs args)
        {
            Game.Say("/all Good game guys, well played.");
        }

        private static void createMenu()
        {
            menu = new Menu("BlackFeeder", "blackfeeder", true);

            // Feeding
            Menu feeder = new Menu("BlackFeeder", "blackfeeder");
            menu.AddSubMenu(feeder);
            feeder.AddItem(new MenuItem("feedingActive", "Feeding active!").SetValue(true));
            feeder.AddItem(new MenuItem("useSkillsActive", "Use Skills to feed!").SetValue(true));
            feeder.AddItem(new MenuItem("sayShitActive", "Say stuff while feeding!").SetValue(true));
            feeder.AddItem(new MenuItem("laughingActive", "Laugh while Feed!").SetValue(true));
            feeder.AddItem(new MenuItem("buyItemsActive", "Buy Items!").SetValue(true));

            // Finalizing
            menu.AddToMainMenu();
        }
    }
}
