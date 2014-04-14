using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UE.Core;
using UE.Core.Architecture;
using UE.Core.Architecture.Messages;
using UE.Core.Entities;
using UE.NUnit;

namespace ConsoleGame
{
    /// <summary>
    /// Dirty proof of concept
    /// </summary>
    class Program
    {
        private static List<String> saves =new List<string>(); 
        private static GameEngine g;
        private static string Culture = "US";
        static void Main(string[] args)
        {
            g = new GameEngine(new XmlRepository(), new RandomDice());
            g.Init("Data\\DefinitionStandard.xml");

            WriteInRed("Be careful : this is a rough test, and there's not much input check");
            LoadSaves();
            if (saves.Any())
            {
                DisplayAndChooseSaves();
            }
            
            while (!g.IsFinished)
            {
                DisplayState();

                Console.Write("Enter 1-6 to search a region,");
                if (g.CanRest)
                {
                    Console.Write("R to rest,");
                }
                if (g.GameState.ConstructsUnactivated.Any())
                {
                    Console.Write(" A to activate a Construct,");
                }
                if (g.GameState.PossibleLinks.Any())
                {
                    Console.Write(" C to connect 2 Constructs,");
                }

                if (g.IsFinalActivationPossible)
                {
                    WriteInRed(" F for final Activation,",false);
                }

                Console.WriteLine("Q to quit");
                string command = Console.ReadLine().ToLower();
                if (command == "q")
                    break;
                switch (command)
                {
                    case "1":
                    case "2":
                    case "3":
                    case "4":
                    case "5":
                    case "6":
                        int region = Convert.ToInt32(command);
                        SearchRegion(region);
                        break;
                    case "r":
                        Rest();
                        break;
                    case "a":
                        Activate();
                        break;
                    case "c":
                        Connect();
                        break;
                    case "f":
                        FinalActivation();
                        break;
                }
                if (!g.IsFinished && g.GameState.CurrentHitPoint == 0)
                {
                    Console.WriteLine("Unconscious ! You have to rest !");
                    if (g.RecoverFromUnconsciousness().eventOccured)
                    {
                        DisplayEvents(true);
                    }

                }
                if (!(g.IsGameLost || g.IsGameWon))
                {
                    g.SaveGameState("autosave.xml");
                }
            }


            if (g.IsGameLost || g.IsGameWon)
            {
                DisplayState();
                if (g.IsGameLost)
                {
                    WriteInRed("Game is LOST");

                }
                if (g.IsGameWon)
                {
                    WriteInRed("Game is Won");
                }
                Console.WriteLine("Score : " + g.Score);
                Console.ReadLine();
            }
        }

        private static void FinalActivation()
        {
            DisplayActivationDifficulty();
            Console.WriteLine("You can spend up to {0} hp to reduce the difficulty. How much ?", g.GameState.CurrentHitPoint);
            int modif = Convert.ToInt32(  Console.ReadLine());
            if (modif > 0)
            {
                g.SpendHPToReduceActivationDifficulty(modif);
                DisplayActivationDifficulty();
            }
            while (!g.IsFinished)
            {
                Console.WriteLine(g.ToString());
                FinalActivationResult far = g.WorkForfinalActivation();
                Console.WriteLine("Rolled : {0}", far.Roll.ToString());
 
            }
        }

        private static void DisplayActivationDifficulty()
        {
           Console.WriteLine("Final activation Difficulty " + g.GameState.FinalActivationDifficulty);
           
        }

        private static bool DisplayAndChooseSaves()
        {
            int i = 1;
            Console.WriteLine("Choose a saved game ( 0 for none)");
            foreach (string file in saves)
            {
                Console.WriteLine(String.Format("{0} : {1}",i++,Path.GetFileNameWithoutExtension(file)));
            }
            int choix = Convert.ToInt32(Console.ReadLine());
            if (choix == 0)
                return false;

            g.LoadGameState(saves[choix-1]);
            return true;
        }

        private static void LoadSaves()
        {
            string[] files = Directory.GetFiles(".","*.xml");
            saves.Clear();
            foreach (string file in files)
            {
                saves.Add(file);
            }

        }

        private static void Connect()
        {
            LinkState ls = g.GameState.PossibleLinks.First();
            if (g.GameState.PossibleLinks.Count() > 1)
            {
                Console.WriteLine("Connect which constructs ?");
                var list =
                    g.GameState.PossibleLinks.Select(
                        (x, i) =>
                            new
                            {
                                x.Name,
                                Index = i + 1
                            });

                foreach (var element in list)
                {
                    Console.WriteLine("{1} : {0}", element.Name, element.Index);
                }
                int index = Convert.ToInt32(Console.ReadLine()) - 1;
                ls = g.GameState.PossibleLinks.ElementAt(index);
            }
            Console.WriteLine("Activating " + ls.Name);
            if (g.HasAbility(Ability.AutomaticallyConnect))
            {
                if (GetYesNoAnswer("Do you want to use the Ancient Record ?"))
                {
                    g.UseAutomaticConnect(ls);
                    return;
                }
            }
            do
            {
                DisplayTable(ls.Connection);
                TwoDice td = g.DiceGenerator.Get2d6();
                string choice = GetNumbers(ls.Connection, td, !g.GameState.IsWasteBasketFull);

                TwoDiceAsString tdas = new TwoDiceAsString(choice);
                LinkResult result = g.WorkToLink(ls.ID, tdas.First, td.First, true);
                if (!result.IsLinkFinished)
                {
                    result = g.WorkToLink(ls.ID, tdas.Second, td.Second, false);
                }
                if (result.IsLinkFinished)
                {
                    Console.Write(result.ResultText);
                    break;
                }
                if (g.GameState.CurrentHitPoint == 0)
                    break;
            }
            while (!g.IsGameLost);
        }



        private static void Activate()
        {
            ConstructState c = g.GameState.ConstructsUnactivated.First();
            if (g.GameState.ConstructsUnactivated.Count() > 1)
            {
                Console.WriteLine("Activate which construct ?");
                var list =
                    g.GameState.ConstructsUnactivated.Select((x, i) => new { Name = x.Construct.Name.Text, Index = i + 1 });
                foreach (var element in list)
                {
                    Console.WriteLine("{1} : {0}", element.Name, element.Index);
                }
                int index = Convert.ToInt32(Console.ReadLine()) - 1;
                c = g.GameState.ConstructsUnactivated.ElementAt(index);
            }
            Console.WriteLine("Activating " + c.Construct.Name.Text);
            if (g.GameState.Inventory.FocusCharmCharged)
            {
                if (GetYesNoAnswer("Do you want to use the focus charm ?"))
                {
                    g.UseFocusCharm(c.Construct.ID);
                }
            }
            do
            {
                DisplayTable(c.CurrentActivationTable);

                TwoDice td = g.DiceGenerator.Get2d6();
                string choice;
                do
                {
                    Console.Write("Place " + td + " on 1-8 (1=TopLeft,4=TopRight, 5=BottomLeft, enter 81 for {1} on topLeft, {0} bottommRight) :",
                                      td.First, td.Second);
                    choice = Console.ReadLine().ToLower();
                    if (choice == "q") return;
                } while (choice.Length != 2);

                TwoDiceAsString tdas = new TwoDiceAsString(choice);
                ActivationResult ar = g.WorkToActivate(c, tdas.First, td.First);
                ar = g.WorkToActivate(c, tdas.Second, td.Second);
                Console.WriteLine("CurrentDay {0} CurrentHP {1} Field Energy Point {2}", g.GameState.CurrentDay, g.GameState.CurrentHitPoint, ar.EnergyPoints);
                if (ar.eventOccured)
                    DisplayEvents(true);

            } while (!g.IsGameLost && !c.HasBeenActivated && g.GameState.CurrentHitPoint > 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="table">should be used to chnage the displayed example</param>
        /// <returns></returns>
        private static string GetNumbers(Table table, TwoDice td, bool wasteBasket)
        {
            string choice;

            do
            {
                string text = "Place " + td + " on 1-6 (1=TopLeft,3=TopRight, 4=BottomLeft, enter 61 for {1} on topLeft, {0} bottommRight) :";
                if (wasteBasket)
                {
                    text += "( 0 for WasteBasket)";
                }
                Console.Write(text,
                                  td.First, td.Second);
                choice = Console.ReadLine().ToLower();
                if (choice == "q") break; ;
            } while (choice.Length != 2);
            return choice;
        }


        private static void Rest()
        {
            int day;

            Console.WriteLine("How many days do you want to rest ?");
            day = Convert.ToInt32(Console.ReadLine());
            if (day == 0)
                return;

            TimePassed t = g.Rest(day);
            if (t.eventOccured)
                DisplayEvents(true);
        }

        private static void DisplayState()
        {
            Console.WriteLine(g.ToString());
            Console.WriteLine(g.GameState.Inventory.ToString());
            DisplayConstructs();
            if (g.GameState.TreasuresFound.Any())
            {
                Console.WriteLine(String.Join("  ", g.GameState.TreasuresFound.Select(x => x.Name.Text)));
            }
            if (g.GameState.ConnectedLinks.Any())
            {
                Console.WriteLine("Links :" + String.Join("  ", g.GameState.ConnectedLinks.Select(x => x.Name)));
            }
        }

        private static void DisplayConstructs()
        {
            if (!g.GameState.ConstructsFound.Any())
                return;
            foreach (var st in g.GameState.ConstructsFound)
            {
                if (st.HasBeenActivated)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                Console.Write(st.Construct.Name.Text + "  ");
                Console.ResetColor();
            }
            Console.WriteLine();
        }

        private static void SearchRegion(int indexRegion)
        {
            Region r = g.GameDefinition.Regions.Single(x => x.Index == indexRegion);
            WriteInRed(String.Format("{2} looking for {0}{1}", r.Construct.Name.TextFor(Culture), "", r.Name.TextFor(Culture)));
            g.EnterRegionForSearch(r);
            DisplayEvents(false);
            Table searchBox;
            bool searchAgainSameRegion = false;
            do
            {
                searchBox = g.CurrentSearchBoxForRegion(indexRegion);
                TimePassed t = g.CrossRegionTracker(r);
                if (g != null)
                {
                    Console.WriteLine("Current day : " + g.GameState.CurrentDay + " Available Searchboxes " + g.NumberOfAvailableSearchesBoxesFor(indexRegion));
                    if (t.eventOccured)
                    {
                        DisplayEvents(true);
                    }
                }
                while (!searchBox.IsFull)
                {
                    TwoDice td = g.DiceGenerator.Get2d6();
                    DisplayTable(searchBox);
                    string choice;
                    TwoDiceAsString tdas;
                    do
                    {
                        Console.Write(
                            String.Format("Place " + td.ToString() +
                                          " on 1-6 (1=TopLeft,3=TopRight, 4=BottomLeft, enter 52 for {1} on topmiddle, {0} bottommiddle) :",
                                          td.First, td.Second));
                        choice = Console.ReadLine().ToLower();
                        if (choice == "q") return;
                        tdas=new TwoDiceAsString(choice);
                    } while (choice.Length != 2);

                   

                    g.PlaceSearchNumberOnRegion(indexRegion, tdas.First, td.First);
                    g.PlaceSearchNumberOnRegion(indexRegion, tdas.Second, td.Second);
                }
                int result = searchBox.SearchResult;
                Console.WriteLine("Search result is : " + result);
                bool modifiedResult = false;
                CanSearchResult crs = g.CanModifySearchResult(result, indexRegion);
                if (crs.CanModify)
                {
                    int number;
                    if (crs.CanUseDowsingRod)
                    {

                        number = GetNumberToModifyResult("DowsingRod", 100);
                        if (number != 0)
                        {
                            result = g.UseDowsingRod(result, number);
                            modifiedResult = true;
                        }
                    }
                    if (g.HasAbility(Ability.GoodFortune, indexRegion) && result != 0)
                    {
                        result = ModifiySearchResult(result, ref modifiedResult, "EventGoodFortune", 10);
                    }
                    if (((indexRegion == 3) || (indexRegion == 4)) && g.HasAbility(Ability.HelpSearch34))
                    {
                        result = ModifiySearchResult(result, ref modifiedResult, "ACtivated Scrying Lens", 10);
                    }
                    if (((indexRegion == 1) || (indexRegion == 6)) && g.HasAbility(Ability.HelpSearch16))
                    {
                        result = ModifiySearchResult(result, ref modifiedResult, "ACtivated Hermetic Mirror", 10);
                    }
                }

                SearchResult sr = g.ApplySearch(result, r, modifiedResult);
                Console.WriteLine(sr);
                if (sr.MonsterLevel > 0)
                {
                    Encounter e = r.GetEncounter(sr.MonsterLevel);
                    if (g.HasAbility(Ability.IgnoreEncounter))
                    {
                        Console.WriteLine("You have encountered " + e.Name.Text);
                        if (GetYesNoAnswer("Do you want to flee using the shimmering moonlace ?"))
                            break;
                    }
                    Fight(r, e);
                    if (g.GameState.CurrentHitPoint == 0)
                        break;
                }
                if (g.NumberOfAvailableSearchesBoxesFor(indexRegion) == 0)
                {
                    Console.WriteLine("You have fully searched the region.");
                    Console.Write("Spend a day and press C to get a component");
                    RegionState rs = g.GetRegionStateFor(r);
                    if (!rs.LegendaryTreasureFound)
                    { Console.Write(", press S to get the " + r.Construct.Name.Text); }
                    Console.WriteLine(" or Q to stop right now");
                    string answer = Console.ReadLine().ToLower();
                    if (!rs.LegendaryTreasureFound && answer == "s")
                    {
                        g.FindConstruct(rs, false);
                        t = g.AddToCurrentDay(1);
                        if (t.eventOccured)
                        {
                            DisplayEvents(true);
                        }
                    }
                    if (answer == "c")
                    {
                        g.AddComponent(1, r.Component);
                        t = g.AddToCurrentDay(1);
                        if (t.eventOccured)
                        {
                            DisplayEvents(true);
                        }
                    }
                    searchAgainSameRegion = false;
                }
                else
                {
                    searchAgainSameRegion = GetYesNoAnswer("Do you want to search the same region again ?");
                }
            } while (searchAgainSameRegion);

        }

        private static int ModifiySearchResult(int result, ref bool modifiedResult, string text, int max)
        {
            int number = GetNumberToModifyResult(text, max);
            if (number != 0)
            {
                result -= number;
                modifiedResult = true;
            }
            return result;
        }

        private static bool GetYesNoAnswer(string text)
        {
            Console.WriteLine(text + " ( Y for yes)");
            string answer = Console.ReadLine().ToLower();
            return answer == "y";
        }

        private static void DisplayEvents(bool changed)
        {
            if (changed)
                WriteInRed("Events changed");
            for (int i = 1; i <= 6; i++)
            {
                RegionState rs = g.GetRegionStateFor(i);
                foreach (RegionEvent a in rs.Events)
                {

                    Console.Write("{0}={1}   ", rs.Region.Name.Text, a.ToString());
                }
            }
            Console.WriteLine();
        }

        private static void WriteInRed(string p, bool jumpLine = true)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            if (jumpLine)
                Console.WriteLine(p);
            else
            {
                Console.Write(p);
            }
            Console.ResetColor();
        }

        private static void Fight(Region r, Encounter e)
        {
            WriteInRed("You must fight " + e.ToString());
            g.StartFight();
            CombatResult cr;
            do
            {
                TwoDice td = g.DiceGenerator.Get2d6();
                if (g.GameState.ParalysisWandInUse)
                    td.ModifyBothDie(2);
                Console.WriteLine("You rolled {0} and {1}", td.First, td.Second);
                if (g.GameState.Inventory.ParalysisWandCharged)
                {
                    bool yes = GetYesNoAnswer("Do you want to use the paralysis Wand ?");

                    if (yes)
                    {
                        g.UseParalysisWand();
                        td.ModifyBothDie(2);
                        Console.WriteLine("Modified result {0} and {1}", td.First, td.Second);
                    }
                }
                cr = g.ApplyCombatRoll(td, e, r);
                Console.WriteLine("Combat result : " + cr.ToString());
            } while (g.GameState.CurrentHitPoint > 0 && cr.EncounterDead == false);

        }

        private static int GetNumberToModifyResult(string abilityName, int limit)
        {
            int number;
            bool ValueOk = false;
            do
            {
                Console.WriteLine("to use  " + abilityName + " , enter a number between 1 and " + limit + " (0 to not use)");
                number = Convert.ToInt32(Console.ReadLine());
                ValueOk = !(number < 0 || number > limit);
            } while (!ValueOk);
            return number;
        }

        private static void DisplayTable(Table table)
        {
            int width = table.Columns.Count * 2 + 1;
            Console.WriteLine(new string('-', width));
            Console.Write("|");
            for (int i = 0; i < table.ColumnCount; i++)
            {
                Console.Write(table.Columns[i].TopNumber + "|");
            }
            Console.WriteLine();
            Console.WriteLine(new string('-', width));
            Console.Write("|");
            for (int i = 0; i < table.ColumnCount; i++)
            {
                Console.Write(table.Columns[i].BottomNumber + "|");
            }
            Console.WriteLine();
            Console.WriteLine(new string('-', width));
        }
    }
}


