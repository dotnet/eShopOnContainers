using eShopOnContainers.Core.Controls;
using eShopOnContainers.iOS.Renderers;
using Foundation;
using System.Threading.Tasks;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(TabbedPage), typeof(CustomTabbedPageRenderer))]
namespace eShopOnContainers.iOS.Renderers
{
    [Preserve]
    public class CustomTabbedPageRenderer : TabbedRenderer
    {
        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            for (var i = 0; i < TabBar.Items.Length; i++)
            {
                AddTabBadge(i);
            }

            Element.ChildAdded += OnTabAdded;
            Element.ChildRemoved += OnTabRemoved;
        }

        private void AddTabBadge(int tabIndex)
        {
            var element = Tabbed.Children[tabIndex];
            element.PropertyChanged += OnTabbedPagePropertyChanged;

            if (TabBar.Items.Length > tabIndex)
            {
                var tabBarItem = TabBar.Items[tabIndex];
                UpdateTabBadgeText(tabBarItem, element);

                var tabColor = CustomTabbedPage.GetBadgeColor(element);
                if (tabColor != Color.Default)
                {
                    tabBarItem.BadgeColor = tabColor.ToUIColor();
                }
            }
        }

        private void UpdateTabBadgeText(UITabBarItem tabBarItem, Element element)
        {
            var text = CustomTabbedPage.GetBadgeText(element);

            tabBarItem.BadgeValue = string.IsNullOrEmpty(text) ? null : text;
        }

        private void OnTabbedPagePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var page = sender as Page;
            if (page == null)
                return;

            if (e.PropertyName == CustomTabbedPage.BadgeTextProperty.PropertyName)
            {
                var tabIndex = Tabbed.Children.IndexOf(page);

                if (tabIndex < TabBar.Items.Length)
                    UpdateTabBadgeText(TabBar.Items[tabIndex], page);

                return;
            }

            if (e.PropertyName == CustomTabbedPage.BadgeColorProperty.PropertyName)
            {
                var tabIndex = Tabbed.Children.IndexOf(page);
                if (tabIndex < TabBar.Items.Length)
                    TabBar.Items[tabIndex].BadgeColor = CustomTabbedPage.GetBadgeColor(page).ToUIColor();
            }
        }

        private void OnTabAdded(object sender, ElementEventArgs e)
        {
            var page = e.Element as Page;
            if (page == null)
                return;

            var tabIndex = Tabbed.Children.IndexOf(page);
            AddTabBadge(tabIndex);
        }

        private void OnTabRemoved(object sender, ElementEventArgs e)
        {
            e.Element.PropertyChanged -= OnTabbedPagePropertyChanged;
        }

        protected override void Dispose(bool disposing)
        {
            if (Tabbed != null)
            {
                foreach (var tab in Tabbed.Children)
                {
                    tab.PropertyChanged -= OnTabbedPagePropertyChanged;
                }

                Tabbed.ChildAdded -= OnTabAdded;
                Tabbed.ChildRemoved -= OnTabRemoved;
            }

            base.Dispose(disposing);
        }
    }
}