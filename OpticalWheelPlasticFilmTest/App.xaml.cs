using OpticalWheelPlasticFilmTest.ViewModels.Dialogs;
using OpticalWheelPlasticFilmTest.Views;
using OpticalWheelPlasticFilmTest.Views.Dialogs;
using Prism.Ioc;
using Prism.Modularity;
using System.Windows;

namespace OpticalWheelPlasticFilmTest
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            //注册对话框
            containerRegistry.RegisterDialog<AboutDialog, AboutDialogViewModel>();
        }
    }
}
