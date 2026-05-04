using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
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

            MessagingCenter.Subscribe<HomeViewModel>(this, "FocusCartSearch", (sender) => {
                CartSearchBar.Focus();
            });
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

        private void OnSearchButtonPressed(object sender, EventArgs e)
        {
            if (BindingContext is HomeViewModel viewModel)
            {
                viewModel.SelectFirstSearchResult();
                CartSearchBar.Text = string.Empty;
                CartSearchBar.Unfocus();
            }
        }

        private void OnSearchProductItems(object sender, TextChangedEventArgs e)
        {
            if(BindingContext is HomeViewModel viewModel)
            {
                viewModel.ProductSearch = e.NewTextValue;
            }
        }

        /* Keyboard shortcuts */
        protected override void OnHandlerChanged()
        {
            base.OnHandlerChanged();
#if WINDOWS
            var window = App.Current.Windows[0].Handler.PlatformView as Microsoft.UI.Xaml.Window;
            if (window != null)
            {
                window.Content.KeyDown += (s, e) =>
                {
                    var viewModel = BindingContext as HomeViewModel;
                    if (viewModel == null) return;

                    if (e.Key == Windows.System.VirtualKey.F5)
                    {
                        viewModel.PrintInvoiceCommand.Execute(null);
                        e.Handled = true;
                    }
                    else if (e.Key == Windows.System.VirtualKey.F1)
                    {
                        viewModel.FocusSearchCommand.Execute(null);
                        e.Handled = true;
                    }
                };
            }
#endif
        }
    }

}
