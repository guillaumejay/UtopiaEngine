using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cirrious.MvvmCross.Console.Platform;
	using Cirrious.CrossCore;
	using Cirrious.CrossCore.IoC;
    using Cirrious.MvvmCross.ViewModels;
using UE.Core;

namespace UE.Console
{
    public class Setup : MvxConsoleSetup
    {
        protected override IMvxApplication CreateApp()
        {
            var app = new App();
            return app;
        }

    }
}
