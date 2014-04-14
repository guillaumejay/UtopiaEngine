using System.Windows.Input;
using Cirrious.MvvmCross.ViewModels;
using UE.Core.Architecture.Messages;
using UE.Core.Interfaces;

namespace UE.Core.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public MainViewModel(IGameEngine engine): base(engine)
        {

        }

        public TimePassed TimePassed { get; set; }

        protected override void FillViewModel()
        {
            CurrentHP = _gameEngine.GameState.CurrentHitPoint;
            CanLink = _gameEngine.CanLink;
            CanActivate = _gameEngine.CanActivate;
            CanFinalActivate = _gameEngine.IsFinalActivationPossible;
            CanRest = _gameEngine.CanRest;
        }

        public ICommand GoToSearchCommand
        {
            get
            {
                return new MvxCommand(() => ShowViewModel<StartSearchViewModel>());
            }
        }

        public ICommand RestCommand { get { return new MvxCommand<int>(Rest); } }

        protected void Rest(int nbDays)
        {
            TimePassed = _gameEngine.Rest(nbDays);
            FillViewModel();
        }

        public bool CanActivate { get; set; }

        public bool CanLink { get; set; }

        public bool CanFinalActivate { get; set; }

        public bool CanRest { get; set; }


    }

}
