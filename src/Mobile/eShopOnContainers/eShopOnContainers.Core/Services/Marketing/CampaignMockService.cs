using eShopOnContainers.Core.Models.Marketing;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace eShopOnContainers.Core.Services.Marketing
{
    public class CampaignMockService : ICampaignService
    {
        private readonly ObservableCollection<CampaignItem> _mockCampaign = new ObservableCollection<CampaignItem>
        {
            new CampaignItem
            {
                Id = Common.Common.MockCampaignId01,
                PictureUri = Device.RuntimePlatform != Device.UWP
                    ? "fake_campaign_01.png"
                    : "Assets/fake_campaign_01.png",
                Name = ".NET Bot Black Hoodie 50% OFF",
                Description = "Campaign Description 1",
                From = DateTime.Now,
                To = DateTime.Now.AddDays(7)
            },

            new CampaignItem
            {
                Id = Common.Common.MockCampaignId02,
                PictureUri = Device.RuntimePlatform != Device.UWP
                    ? "fake_campaign_02.png"
                    : "Assets/fake_campaign_02.png",
                Name = "Roslyn Red T-Shirt 3x2",
                Description = "Campaign Description 2",
                From = DateTime.Now.AddDays(-7),
                To = DateTime.Now.AddDays(14)
            }
        };

        public Task<ObservableCollection<CampaignItem>> GetAllCampaignsAsync(string token)
        {
            return Task.FromResult(_mockCampaign);
        }

        public Task<CampaignItem> GetCampaignByIdAsync(int campaignId, string token)
        {
            return Task.FromResult(_mockCampaign.SingleOrDefault(c => c.Id == campaignId));
        }
    }
}