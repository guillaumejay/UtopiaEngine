using Cirrious.CrossCore;
using Cirrious.CrossCore.IoC;
using UE.Core.Interfaces;
using UE.Core.Repository;

namespace UE.Core
{
    public class App : Cirrious.MvvmCross.ViewModels.MvxApplication
    {
        public override void Initialize()
        {
            Mvx.RegisterType<IDiceRoller, RandomDice>();
            Mvx.RegisterType<IGameEngine, GameEngine>();
            //  Mvx.RegisterType<IMvxFileStore, MVvxC>();
            Mvx.RegisterType<IRepository, MVVMRepository>();
            Mvx.RegisterSingleton<IGameEngine>(new GameEngine(Mvx.Resolve<IRepository>(), Mvx.Resolve<IDiceRoller>()));
            IGameEngine g = Mvx.Resolve<IGameEngine>();
            g.Init("Data\\DefinitionStandard.xml");
            g.GameState.CurrentHitPoint = 3;
            RegisterAppStart<ViewModels.TitleViewModel>();
        }
    }
}