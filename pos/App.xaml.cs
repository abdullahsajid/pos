using pos.Data;
using System.Globalization;

namespace pos
{
    public partial class App : Application
    {
        private readonly DB_Services _dbServices;
        public App(DB_Services dbservice)
        {
            InitializeComponent();
            _dbServices = dbservice;
            var culture = new CultureInfo("en-PK");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }

        protected override async void OnStart()
        {
            base.OnStart();
            await _dbServices.initDatabase();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(new AppShell());

            var displayInfo = DeviceDisplay.Current.MainDisplayInfo;

            window.MaximumWidth = double.PositiveInfinity;
            window.MaximumHeight = double.PositiveInfinity;
            window.Width = displayInfo.Width / displayInfo.Density;
            window.Height = displayInfo.Height / displayInfo.Density;

            window.X = 0;
            window.Y = 0;

            return window;
        }
    }
}