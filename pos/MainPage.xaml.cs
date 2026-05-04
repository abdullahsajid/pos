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

                    // --- Global Shortcuts ---
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

                    // --- Search Navigation Shortcuts (only when search is open) ---
                    else if (viewModel.IsSearchActive && viewModel.ProductItems != null && viewModel.ProductItems.Any())
                    {
                        if (e.Key == Windows.System.VirtualKey.Down || e.Key == Windows.System.VirtualKey.Tab)
                        {
                            // Move selection down
                            var items = viewModel.ProductItems;
                            int currentIndex = viewModel.SelectedSearchResult != null
                                ? items.IndexOf(viewModel.SelectedSearchResult) : -1;
                            int nextIndex = Math.Min(currentIndex + 1, items.Count - 1);
                            viewModel.SelectedSearchResult = items[nextIndex];
                            SearchResultsView.ScrollTo(viewModel.SelectedSearchResult);
                            e.Handled = true;
                        }
                        else if (e.Key == Windows.System.VirtualKey.Up)
                        {
                            // Move selection up
                            var items = viewModel.ProductItems;
                            int currentIndex = viewModel.SelectedSearchResult != null
                                ? items.IndexOf(viewModel.SelectedSearchResult) : 0;
                            int prevIndex = Math.Max(currentIndex - 1, 0);
                            viewModel.SelectedSearchResult = items[prevIndex];
                            SearchResultsView.ScrollTo(viewModel.SelectedSearchResult);
                            e.Handled = true;
                        }
                        else if (e.Key == Windows.System.VirtualKey.Enter)
                        {
                            // Add selected item to cart
                            viewModel.SelectFirstSearchResult();
                            CartSearchBar.Text = string.Empty;
                            CartSearchBar.Focus(); // keep focus on search bar for quick scanning
                            e.Handled = true;
                        }
                        else if (e.Key == Windows.System.VirtualKey.Escape)
                        {
                            // Close search
                            viewModel.IsSearchActive = false;
                            viewModel.SelectedSearchResult = null;
                            CartSearchBar.Text = string.Empty;
                            e.Handled = true;
                        }
                    }
                };
            }
#endif
        }
    }

}
