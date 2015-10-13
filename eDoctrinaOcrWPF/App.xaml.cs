using System.Windows;

namespace eDoctrinaOcrWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            eDoctrinaUtilsWPF.WpfSingleInstance.Make();
            base.OnStartup(e);
        }
    }
}
