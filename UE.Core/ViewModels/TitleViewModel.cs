// --------------------------------------------------------------------------------------------------------------------
// <summary>
//    Defines the TitleViewModel type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Cirrious.CrossCore.UI;
using UE.Core.Interfaces;

namespace UE.Core.ViewModels
{
    using System.Windows.Input;

    using Cirrious.MvvmCross.ViewModels;

    /// <summary>
    /// Define the TitleViewModel type.
    /// </summary>
    public class TitleViewModel : BaseViewModel
    {
        private IRepository _repository;

        public TitleViewModel(IRepository repository, IGameEngine gameEngine)
            : base(gameEngine)
        {
            _repository = repository;
            IsGameInProgress = _repository.IsAutoSaveAvailable;
        //   DisplayGameLoad = (_repository.IsAutoSaveAvailable) ? MvxVisibility.Visible : MvxVisibility.Collapsed;
        }

//        public MvxVisibility DisplayGameLoad 
//        {
//            get; set; } 
//        
 
        public bool IsGameInProgress { get; set; }


        public ICommand NewGameCommand
        {
            get
            {
                return new MvxCommand(() =>
                    {
                        _repository.DeleteAutoSave();
                        _gameEngine.ResetGameState();
                        ShowViewModel<MainViewModel>();
                    }
                    );
            }
        }

        public ICommand ContinueGameCommand
        {
            get { return new MvxCommand(ContinueGame); }
        }


        private void ContinueGame()
        {
            _gameEngine.LoadAutoSave();
            ShowViewModel<MainViewModel>();
        }

        protected override void FillViewModel()
        {
           
        }
    }
}
