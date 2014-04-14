using Cirrious.MvvmCross.ViewModels;
using UE.Core.Interfaces;

namespace UE.Core.ViewModels
{
    public abstract class BaseViewModel : MvxViewModel
    {
        protected IGameEngine _gameEngine;
        private int _daysRemaining;
        private int _currentHp;

        protected BaseViewModel(IGameEngine engine)
        {
            _gameEngine = engine;
            CurrentHP = engine.GameState.CurrentHitPoint;
            DaysRemaining = engine.DaysRemaining;
            FillViewModel();

        }

        public int CurrentHP
        {
            get { return _currentHp; }
            set
            {
                _currentHp = value;
                RaisePropertyChanged(() => CurrentHP);
            }
        }

        public int DaysRemaining
        {
            get { return _daysRemaining; }
            set
            {
                _daysRemaining = value;
                RaisePropertyChanged(() => DaysRemaining);
            }
        }

        protected abstract void FillViewModel();
    }
}