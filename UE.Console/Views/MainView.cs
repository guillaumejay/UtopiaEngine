using System;
using Cirrious.MvvmCross.Console.Views;
using UE.Core.ViewModels;
using System.Windows;

namespace UE.Console.Views
{
    class MainView : MvxConsoleView<MainViewModel>
    {
        protected override void OnViewModelChanged()
        {
            base.OnViewModelChanged();
            ViewModel.PropertyChanged += (sender, args) => RefreshDisplay();
            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            System.Console.BackgroundColor = ConsoleColor.Black;
            System.Console.ForegroundColor = ConsoleColor.White;
            System.Console.Clear();
            System.Console.WriteLine("What do you want to do ? ");
            System.Console.WriteLine();

            System.Console.WriteLine("Days remaining {0} - Current HP {1}", ViewModel.DaysRemaining,ViewModel.CurrentHP);
            System.Console.WriteLine();
            
            if (ViewModel.TimePassed != null)
            {
                if (ViewModel.TimePassed.eventOccured)
                {
                    System.Console.ForegroundColor=ConsoleColor.Red;
                    System.Console.WriteLine("Events have changed");
                }
            }
            System.Console.ForegroundColor = ConsoleColor.Yellow;
            System.Console.WriteLine("To search, (S)EARCH");
            if (ViewModel.CanRest)
            {
                System.Console.WriteLine("To Rest for one day, (R)est");
            }
                       System.Console.ForegroundColor = ConsoleColor.White;
        }

        public override bool HandleInput(string input)
        {
            input = input.Trim().ToUpper();
            if (input == "R" && ViewModel.CanRest)
            {
				ViewModel.RestCommand.Execute (1);
                RefreshDisplay();
                return true;
            }
            if (input == "S" )
            {
               
                return true;
            }

            
            //    case "SEARCH":
            //    case "S":
            //        ViewModel.Commands["Search"].Execute(null);
            //        return true;
            //    case "RANDOM":
                
            //    default:
            //        string searchTerm = null;
            //        if (input.StartsWith("EDIT ") && input.Length > 5)
            //            searchTerm = input.Substring(5);
            //        else if (input.StartsWith("E ") && input.Length > 2)
            //            searchTerm = input.Substring(2);
            //        if (searchTerm != null)
            //        {
            //            ViewModel.SearchText = searchTerm;
            //            return true;
            //        }
           

            return base.HandleInput(input);
        }
    }
}
