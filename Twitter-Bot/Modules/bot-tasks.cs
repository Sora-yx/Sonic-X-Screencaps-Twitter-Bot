using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading.Tasks;

using Tweetinvi;
using Tweetinvi.Parameters;
using Tweetinvi.Models;
using System.Linq;

namespace Twitter_Bot.Modules
{
    public static class BotExecTask
    {
        public static int screenCount;
        public static int currentEpisode = 62;
        public static int currentScreen = 0;
        public static List<string> screenList = new List<string>();

        public static async Task updateScreenList()
        {
            screenList.Clear();
            String screenName = "episode" + currentEpisode.ToString() + "\\";

            foreach (string f in Directory.GetFiles(screenName, "*.*"))
            {
                screenList.Add(f);
            }

            await Task.CompletedTask;
        }

        public static string getCurrentScreenName()
        {
            return screenList.ElementAt(currentScreen);
        }


        public static async Task SendImageTweet()
        {
            await updateScreenCount();

            byte[] tweetinviLogoBinary = File.ReadAllBytes(getCurrentScreenName()); //"episode" + currentEpisode.ToString() + "\\" + screenList[currentScreen].ToString() + ".png");

            IMedia uploadedImage = await Program.client.Upload.UploadTweetImageAsync(tweetinviLogoBinary);
            ITweet tweetWithImage = await Program.client.Tweets.PublishTweetAsync(new PublishTweetParameters() { Medias = { uploadedImage } });

            //var tweet = await userClient.Tweets.PublishTweetAsync("Hello World :)");
            Console.WriteLine("Send screen " + currentScreen + " from episode " + currentEpisode);

            currentScreen++;

            if (currentScreen > screenCount)
            {
                currentEpisode++;
                currentScreen = 0;
            }

            if (currentEpisode > 78)
                currentEpisode = 1;

            await Task.CompletedTask;
        }

        public static async Task updateScreenCount()
        {
            screenCount = (from file in Directory.EnumerateFiles(@"Episode" + currentEpisode.ToString() + "\\ ", "*.png", SearchOption.AllDirectories)
                           select file).Count();

            Console.WriteLine("New screen count is " + screenCount + " From episode " + currentEpisode);
            await Task.CompletedTask;
        }
    }
}
