using System;
using Tweetinvi;
using Tweetinvi.Models;
using System.Threading.Tasks;
using System.IO;
using Tweetinvi.Parameters;
using System.Linq;

namespace Twitter_Bot
{
    class Program
    {
        public static TwitterClient client;
        private static int connectionFailCount = 0;

        string[] tokens;

        static void Main(string[] args) => new Program().RunBotMain().GetAwaiter().GetResult();

        public async Task RunBotMain()
        {
            try
            {
                using (var sr = new StreamReader("token.txt"))
                {
                    Console.WriteLine("Reading Twitter token information...");
                    tokens = File.ReadAllLines("token.txt");
                    sr.Close();
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("Error, couldn't read the file.");
                Console.WriteLine(e.Message);

                await Task.Delay(1000);
                Environment.Exit(0);
            }

            client = new TwitterClient(tokens[0], tokens[1], tokens[2], tokens[3]);

            Console.WriteLine("Connected!");

            await Twitter_Bot.Modules.BotExecTask.updateScreenList();


            System.Timers.Timer timer = new System.Timers.Timer()
            {
                AutoReset = true,
                Interval = 60000, //1 min
            };

            timer.Elapsed += CheckSendTweet_Loop;
            timer.Start();

            /*byte[] tweetinviLogoBinary = File.ReadAllBytes("Episode62/vlcsnap-2020-09-01-11h48m01s983.png");

            IMedia uploadedImage = await  client.Upload.UploadTweetImageAsync(tweetinviLogoBinary);
            ITweet tweetWithImage = await client.Tweets.PublishTweetAsync(new PublishTweetParameters() { Medias = { uploadedImage } });

            //var tweet = await userClient.Tweets.PublishTweetAsync("Hello World :)");
            Console.WriteLine("You published the tweet ^^");*/

            await Task.Delay(-1);
        }

        private static async void CheckSendTweet_Loop(object sender, System.Timers.ElapsedEventArgs e)
        {
            var date = DateTime.Now;

            if (date.Minute % 10 == 0 || date.Minute % 10 == 5 || connectionFailCount > 0 && connectionFailCount < 4) //we try again 3 times if the connection failed (one try every minute.)
            {
                await isConnectionAllowed(); 

                if (connectionFailCount > 0) //We don't want to try to send the tweet if the connection failed.
                    return;

                await Twitter_Bot.Modules.BotExecTask.SendImageTweet();
            }
        }


        public static async Task isConnectionAllowed()
        {
            try
            {
                var user = await client.Users.GetAuthenticatedUserAsync();
                connectionFailCount = 0;
                await Task.CompletedTask;
                return;
            }
            catch (Tweetinvi.Exceptions.TwitterAuthAbortedException e)
            {
                Console.WriteLine(e.ToString());
                connectionFailCount++;
                return;
            }
            catch (Tweetinvi.Exceptions.TwitterException e)
            {
                Console.WriteLine(e.ToString());
                connectionFailCount++;
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                connectionFailCount++;
                return;
            }
        }

    }
}
