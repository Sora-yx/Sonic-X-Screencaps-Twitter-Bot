using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Tweetinvi.Parameters;
using Tweetinvi.Models;
using System.Linq;
using System.Text.Json;

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
            String screenName = "episode" + currentEpisode.ToString();

            if (Directory.Exists(screenName))
            {
                screenList.Clear();
                screenName += "\\";

                foreach (string f in Directory.GetFiles(screenName, "*.*"))
                {
                    screenList.Add(f);
                }
            }
            else
            {
                Console.WriteLine("Fatal Error, I couldn't get the screencap in the folder. I tried Episode " + currentEpisode + " and screencap number " + currentScreen);
                return;
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

            if (screenCount <= 0)
            {
                Console.WriteLine("Fatal Error, I couldn't get the screencap in the folder. I tried Episode " + currentEpisode + " and screencap number " + currentScreen);
                return;
            }

            byte[] tweetinviLogoBinary = File.ReadAllBytes(getCurrentScreenName()); //"episode" + currentEpisode.ToString() + "\\" + screenList[currentScreen].ToString() + ".png");

            if (tweetinviLogoBinary != null)
            {
                IMedia uploadedImage = await Program.client.Upload.UploadTweetImageAsync(tweetinviLogoBinary);
                ITweet tweetWithImage = await Program.client.Tweets.PublishTweetAsync(new PublishTweetParameters() { Medias = { uploadedImage } });
            }
            else
            {
                Console.WriteLine("Fatal Error, I couldn't get the screencap in the folder. I was on Episode " + currentEpisode + " and screencap number " + currentScreen);
                return;
            }

            //var tweet = await userClient.Tweets.PublishTweetAsync("Hello World :)");
            Console.WriteLine("Send screen " + currentScreen + " from episode " + currentEpisode);

            currentScreen++;

            if (currentScreen > screenCount)
            {
                Console.WriteLine("I finished episode " + currentEpisode + " Moving to next episode...");
                currentEpisode++;
                if (currentEpisode < 64)
                    await updateScreenList();
                currentScreen = 0;
            }

            if (currentEpisode > 63)
            {
                Console.WriteLine("I finished my very long first round. It's time to back to episode 1.");
                currentScreen = 0;
                currentEpisode = 1;
                await updateScreenList();
            }

            Program.backupProgress.Clear();
            Program.backupProgress.Add(currentEpisode);
            Program.backupProgress.Add(currentScreen);


           var json = JsonSerializer.Serialize(Program.backupProgress);
            File.WriteAllText("Backup.json", json + Environment.NewLine);
            Console.WriteLine("Updated json file with the current screenshot and episode.");

            await Task.CompletedTask;
        }

        public static async Task updateScreenCount()
        {
            string folder = "Episode" + currentEpisode.ToString();

            if (Directory.Exists(folder))
            {
                screenCount = (from file in Directory.EnumerateFiles(@"Episode" + currentEpisode.ToString() + "\\ ", "*.png", SearchOption.AllDirectories)
                               select file).Count() - 1;
                Console.WriteLine("New screen count is " + screenCount + " From episode " + currentEpisode);
            }
            else
            {
                Console.WriteLine("Couldn't get the repertory from episode " + currentEpisode);
            }

            await Task.CompletedTask;
        }
    }
}
