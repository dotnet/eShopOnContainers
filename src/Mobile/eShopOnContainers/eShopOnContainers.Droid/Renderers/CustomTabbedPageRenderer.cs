using Android.Content;
using Android.Graphics;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Views;
using Android.Widget;
using eShopOnContainers.Core.Controls;
using eShopOnContainers.Droid.Extensions;
using eShopOnContainers.Droid.Renderers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms.Platform.Android.AppCompat;

[assembly: ExportRenderer(typeof(TabbedPage), typeof(CustomTabbedPageRenderer))]
namespace eShopOnContainers.Droid.Renderers
{
    public class CustomTabbedPageRenderer : TabbedPageRenderer
    {
        protected readonly Dictionary<Element, BadgeView> BadgeViews = new Dictionary<Element, BadgeView>();
        private TabLayout _tabLayout;
        private LinearLayout _tabStrip;
        private ViewPager _viewPager;
        private TabbedPage _tabbedPage;
        private bool _firstTime = true;

        public CustomTabbedPageRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<TabbedPage> e)
        {
            base.OnElementChanged(e);

            _tabLayout = ViewGroup.FindChildOfType<TabLayout>();

            if (_tabLayout == null)
            {
                Console.WriteLine("No TabLayout found. Badge not added.");
                return;
            }

            _tabbedPage = e.NewElement as TabbedPage;
            _viewPager = (ViewPager)GetChildAt(0);

            _tabLayout.TabSelected += (s, a) =>
            {
                var page = _tabbedPage.Children[a.Tab.Position];
                SetTab(a.Tab, page.Icon.File);
                _viewPager.SetCurrentItem(a.Tab.Position, false);
            };

            _tabStrip = _tabLayout.FindChildOfType<LinearLayout>();

            for (var i = 0; i < _tabLayout.TabCount; i++)
            {
                AddTabBadge(i);
            }

            Element.ChildAdded += OnTabAdded;
            Element.ChildRemoved += OnTabRemoved;
        }

        private void SetTab(TabLayout.Tab tab, string name)
        {
            try
            {
                int id = Resources.GetIdentifier(name, "drawable", Context.PackageName);
                tab.SetIcon(null);

                LinearLayout.LayoutParams linearLayoutParams = new LinearLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent);
                linearLayoutParams.SetMargins(0, -48, 0, 0);

                ImageView img = new ImageView(Context);
                img.LayoutParameters = linearLayoutParams;
                img.SetPadding(0, 0, 0, 48);
                img.SetImageResource(id);

                tab.SetCustomView(img);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
            }
        }

        protected override void DispatchDraw(Canvas canvas)
        {
            base.DispatchDraw(canvas);

            if (!_firstTime)
            {
                return;
            }

            for (int i = 0; i < _tabLayout.TabCount; i++)
            {
                var tab = _tabLayout.GetTabAt(i);
                var page = _tabbedPage.Children[tab.Position];

                if (page is TabbedPage)
                {
                    var tabbedPage = (TabbedPage)page;
                    SetTab(tab, tabbedPage.Icon.File);
                }
                else
                {
                    SetTab(tab, page.Icon.File);
                }

                if (!string.IsNullOrEmpty(_tabbedPage.Title))
                {
                    tab.SetText(string.Empty);
                }
            }

            _firstTime = false;
        }

        private void AddTabBadge(int tabIndex)
        {
            var element = Element.Children[tabIndex];
            var view = _tabLayout?.GetTabAt(tabIndex).CustomView ?? _tabStrip?.GetChildAt(tabIndex);

            var badgeView = (view as ViewGroup)?.FindChildOfType<BadgeView>();

            if (badgeView == null)
            {
                var imageView = (view as ViewGroup)?.FindChildOfType<ImageView>();

                var badgeTarget = imageView?.Drawable != null
                    ? (Android.Views.View)imageView
                    : (view as ViewGroup)?.FindChildOfType<TextView>();

                // Create badge for tab
                badgeView = new BadgeView(Context, badgeTarget);
            }

            BadgeViews[element] = badgeView;

            // Get text
            var badgeText = CustomTabbedPage.GetBadgeText(element);
            badgeView.Text = badgeText;

            // Set color if not default
            var tabColor = CustomTabbedPage.GetBadgeColor(element);

            if (tabColor != Xamarin.Forms.Color.Default)
            {
                badgeView.BadgeColor = tabColor.ToAndroid();
            }

            element.PropertyChanged += OnTabbedPagePropertyChanged;
        }

        protected virtual void OnTabbedPagePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var element = sender as Element;
            if (element == null)
                return;

            BadgeView badgeView;
            if (!BadgeViews.TryGetValue(element, out badgeView))
            {
                return;
            }

            if (e.PropertyName == CustomTabbedPage.BadgeTextProperty.PropertyName)
            {
                badgeView.Text = CustomTabbedPage.GetBadgeText(element);
                return;
            }

            if (e.PropertyName == CustomTabbedPage.BadgeColorProperty.PropertyName)
            {
                badgeView.BadgeColor = CustomTabbedPage.GetBadgeColor(element).ToAndroid();
            }
        }

        private void OnTabRemoved(object sender, ElementEventArgs e)
        {
            e.Element.PropertyChanged -= OnTabbedPagePropertyChanged;
            BadgeViews.Remove(e.Element);
        }

        private void OnTabAdded(object sender, ElementEventArgs e)
        {
            var page = e.Element as Page;

            if (page == null)
                return;

            var tabIndex = Element.Children.IndexOf(page);
            AddTabBadge(tabIndex);
        }

        protected override void Dispose(bool disposing)
        {
            if (Element != null)
            {
                foreach (var tab in Element.Children)
                {
                    tab.PropertyChanged -= OnTabbedPagePropertyChanged;
                }

                Element.ChildRemoved -= OnTabRemoved;
                Element.ChildAdded -= OnTabAdded;

                BadgeViews.Clear();
            }

            base.Dispose(disposing);
        }
    }
}