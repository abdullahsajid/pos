using pos.ViewModels;

namespace pos
{
    public partial class MainPage : ContentPage
    {
        public bool _isBusy;

        public MainPage(HomeViewModel homeViewModel)
        {
            InitializeComponent();
            BindingContext = homeViewModel;

            Loaded += MainPage_Loaded;
        }
        private async void MainPage_Loaded(object sender, EventArgs e)
        {
            await (BindingContext as HomeViewModel).InitializeAsync();
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            if (BindingContext is HomeViewModel viewModel)
            {
                viewModel.SearchText = e.NewTextValue;
            }
        }

        private void OnSearchProductItems(object sender, TextChangedEventArgs e)
        {
            if(BindingContext is HomeViewModel viewModel)
            {
                viewModel.ProductSearch = e.NewTextValue;
            }
        }

    }

}
