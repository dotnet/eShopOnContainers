using eShopOnContainers.Core.Models.Basket;
using eShopOnContainers.Core.Models.Catalog;
using eShopOnContainers.Core.Services.Basket;
using eShopOnContainers.Core.Services.Settings;
using eShopOnContainers.Core.Services.User;
using eShopOnContainers.Core.ViewModels.Base;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace eShopOnContainers.Core.ViewModels
{
    public class BasketViewModel : ViewModelBase
    {
        private int _badgeCount;
        private ObservableCollection<BasketItem> _basketItems;
        private decimal _total;

        private readonly ISettingsService _settingsService;
        private readonly IBasketService _basketService;
        private readonly IUserService _userService;

        public BasketViewModel(
            ISettingsService settingsService,
            IBasketService basketService,
            IUserService userService)
        {
            _settingsService = settingsService;
            _basketService = basketService;
            _userService = userService;
        }

        public int BadgeCount
        {
            get { return _badgeCount; }
            set
            {
                _badgeCount = value;
                RaisePropertyChanged(() => BadgeCount);
            }
        }

        public ObservableCollection<BasketItem> BasketItems
        {
            get { return _basketItems; }
            set
            {
                _basketItems = value;
                RaisePropertyChanged(() => BasketItems);
            }
        }

        public decimal Total
        {
            get { return _total; }
            set
            {
                _total = value;
                RaisePropertyChanged(() => Total);
            }
        }

        public ICommand AddCommand => new Command<BasketItem>(async (item) => await AddItemAsync(item));

        public ICommand CheckoutCommand => new Command(async () => await CheckoutAsync());

        public override async Task InitializeAsync(object navigationData)
        {
            if (BasketItems == null)
                BasketItems = new ObservableCollection<BasketItem>();

            var authToken = _settingsService.AuthAccessToken;
            var userInfo = await _userService.GetUserInfoAsync(authToken);

            // Update Basket
            var basket = await _basketService.GetBasketAsync(userInfo.UserId, authToken);

            if (basket != null && basket.Items != null && basket.Items.Any())
            {
                BadgeCount = 0;
                BasketItems.Clear();

                foreach (var basketItem in basket.Items)
                {
                    BadgeCount += basketItem.Quantity;
                    await AddBasketItemAsync(basketItem);
                }
            }

            MessagingCenter.Unsubscribe<CatalogViewModel, CatalogItem>(this, MessageKeys.AddProduct);
            MessagingCenter.Subscribe<CatalogViewModel, CatalogItem>(this, MessageKeys.AddProduct, async (sender, arg) =>
            {
                BadgeCount++;

                await AddCatalogItemAsync(arg);
            });

            await base.InitializeAsync(navigationData);
        }

        private async Task AddCatalogItemAsync(CatalogItem item)
        {
            BasketItems.Add(new BasketItem
            {
                ProductId = item.Id,
                ProductName = item.Name,
                PictureUrl = item.PictureUri,
                UnitPrice = item.Price,
                Quantity = 1
            });

            await ReCalculateTotalAsync();
        }

        private async Task AddItemAsync(BasketItem item)
        {
            BadgeCount++;
            await AddBasketItemAsync(item);
            RaisePropertyChanged(() => BasketItems);
        }

        private async Task AddBasketItemAsync(BasketItem item)
        {
            BasketItems.Add(item);
            await ReCalculateTotalAsync();
        }

        private async Task ReCalculateTotalAsync()
        {
            Total = 0;

            if (BasketItems == null)
            {
                return;
            }

            foreach (var orderItem in BasketItems)
            {
                Total += (orderItem.Quantity * orderItem.UnitPrice);
            }

            var authToken = _settingsService.AuthAccessToken;
            var userInfo = await _userService.GetUserInfoAsync(authToken);

            await _basketService.UpdateBasketAsync(new CustomerBasket
            {
                BuyerId = userInfo.UserId,
                Items = BasketItems.ToList()
            }, authToken);
        }

        private async Task CheckoutAsync()
        {
            if (BasketItems.Any())
            {
                await NavigationService.NavigateToAsync<CheckoutViewModel>(BasketItems);
            }
        }
    }
}
