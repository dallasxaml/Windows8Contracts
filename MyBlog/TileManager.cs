using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Notifications;
using NotificationsExtensions.TileContent;
using MyBlog.Data;

namespace MyBlog
{
    public class TileManager
    {
        public static async Task UpdateTile()
        {
            var article = SampleDataSource.GetItem("Group-1-Item-1");

            var tileContent = TileContentFactory.CreateTileWideImageAndText01();

            tileContent.TextCaptionWrap.Text = article.Title;
            tileContent.Image.Src = article.ImagePath;
            tileContent.Image.Alt = article.Subtitle;

            var squareContent = TileContentFactory.CreateTileSquareImage();

            squareContent.Image.Src = article.ImagePath;
            squareContent.Image.Alt = article.Title;

            tileContent.SquareContent = squareContent;

            TileNotification notification = tileContent
                .CreateNotification();
            TileUpdateManager
                .CreateTileUpdaterForApplication()
                .Update(notification);
        }
    }
}
