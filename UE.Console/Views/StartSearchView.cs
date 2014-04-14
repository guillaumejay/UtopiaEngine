using System;
using Cirrious.MvvmCross.Console.Views;
using UE.Core.ViewModels;
using System.Windows;

namespace UE.Console.Views
{
    class StartSearchView : MvxConsoleView<StartSearchViewModel>
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
            System.Console.WriteLine("Where do you want to search ? ");
            
            System.Console.WriteLine("");
                              System.Console.ForegroundColor = ConsoleColor.White;
        }

        public override bool HandleInput(string input)
        {
            input = input.Trim().ToUpper();
            
            return base.HandleInput(input);
        }
    }
}
