using pos.Data;

namespace pos
{
    public partial class App : Application
    {
        private readonly DB_Services _dbServices;
        public App(DB_Services dbservice)
        {
            InitializeComponent();
            _dbServices = dbservice;
        }

        protected override async void OnStart()
        {
            base.OnStart();
            await _dbServices.initDatabase();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}