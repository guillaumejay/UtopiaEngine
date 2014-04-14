using System;
using Cirrious.MvvmCross.Console.Views;
using UE.Core.ViewModels;
using System.Windows;

namespace UE.Console.Views
{
    class TitleView : MvxConsoleView<TitleViewModel>
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
            System.Console.WriteLine("S to start a new game");
            if (ViewModel.IsGameInProgress)
                System.Console.WriteLine("C to continue current game");
                     System.Console.ForegroundColor = ConsoleColor.White;
        }

        public override bool HandleInput(string input)
        {
            input = input.Trim().ToUpper();
            if (input == "C" && ViewModel.IsGameInProgress)
            {
				ViewModel.ContinueGameCommand.Execute (null);
                
                return true;
            }
            if (input == "N" )
            {
                ViewModel.NewGameCommand.Execute(null);
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
