using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dnx.Runtime;
using Microsoft.Dnx.Runtime.Infrastructure;
using Microsoft.Framework.Configuration;
using Xunit;
using Action = Hadouken.HadoukenApi.Models.Action;

namespace Hadouken.HadoukenApi.Test
{
    public class Functional
    {
        private readonly byte[] _freeMusicTorrentFile;
        private readonly string _freeMusicTorrentHash;
        private readonly ServerCredential _serverCredential;

        public Functional()
        {
            var appEnv = CallContextServiceLocator.Locator.ServiceProvider.GetService(typeof(IApplicationEnvironment)) as IApplicationEnvironment;
            Debug.Assert(appEnv != null, "appEnv != null");
            var builder = new ConfigurationBuilder(appEnv.ApplicationBasePath);
            builder.AddJsonFile("config.json");
            builder.AddJsonFile("config.private.json", true);
            var configuration = builder.Build();

            var uri = new Uri(configuration["ServerCredentialUri"]);
            var username = configuration["ServerCredentialUsername"];
            var password = configuration["ServerCredentialPassword"];

            _serverCredential = new ServerCredential(uri, username, password);

            _freeMusicTorrentFile =
                new HttpClient().GetByteArrayAsync(
                    new Uri("http://bt.etree.org/download.php/582271/hottuna2015-09-11.flac16.torrent")).Result;
            _freeMusicTorrentHash = "9ecc7229ff971d27552dd399509e188847dbbbf1";

            // Make sure there is no torrents before executing the tests
            var api = new Api(_serverCredential);
            var torrents = api.GetTorrents().Result;
            if (torrents.Any())
            {
                var result = api.Perform(Action.Removedata, torrents.Select(t => t.InfoHash)).Result;
                Assert.True(result);
            }
        }

        [Fact]
        public async Task ScenarioSettings()
        {
            var api = new Api(_serverCredential);

            var systemInfo = await api.GetSystemInfoAsync();
            Assert.False(string.IsNullOrWhiteSpace(systemInfo.Versions.FirstOrDefault().Value));

            var randomPort = new Random().Next(1025, 65534);
            var initialSettings = await api.GetSettings();
            initialSettings.BindPort = randomPort;
            await api.SetSettings(initialSettings);
            Thread.Sleep(1500);
            var settingAfterChange = await api.GetSettings();
            Assert.Equal(randomPort, settingAfterChange.BindPort);

            var result = await api.ListDirectories();
            if (result.Count > 0)
            {
                Assert.False(string.IsNullOrWhiteSpace(result.First().Path));
                Assert.True(result.First().Available > 0);
            }
        }

        [Fact]
        public async Task ScenarioTypicalUse()
        {
            var api = new Api(_serverCredential);

            var resultAddTorrent = await api.AddTorrent(_freeMusicTorrentFile);
            Assert.True(resultAddTorrent);

            var torrents = await api.GetTorrents();
            Assert.True(torrents.Count == 1);
            Assert.Equal(_freeMusicTorrentHash, torrents.First().InfoHash);

            var properties = await api.GetProperties(_freeMusicTorrentHash);

            properties.Label = "free-music";
            var resultSetProperty = await api.SetProperties(_freeMusicTorrentHash, properties);
            Assert.True(resultSetProperty);
            var torrentsAfterLabelChange = await api.GetTorrents();
            Assert.Equal(properties.Label, torrentsAfterLabelChange.First().Label);

            Thread.Sleep(1500);
            var files = await api.GetFiles(_freeMusicTorrentHash);
            var rnd = new Random();
            var randomFileIndex = rnd.Next(files.Count);
            var setFilePriorityResult = await api.SetFilePriority(_freeMusicTorrentHash, randomFileIndex, 0);
            Assert.True(setFilePriorityResult);

            // TODO : Investigate why peers are sometimes unavailable
            //var peers = await api.GetPeers(_freeMusicTorrentHash);
            //Assert.True(peers.Count > 0);

            var deleteResult = await api.Perform(Action.Removedata, _freeMusicTorrentHash);
            Assert.True(deleteResult);
            Assert.True((await api.GetTorrents()).Count == 0);
        }
    }
}