using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Hadouken.HadoukenApi.Models;
using Hadouken.HadoukenApi.Models.Settings;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Action = Hadouken.HadoukenApi.Models.Action;

namespace Hadouken.HadoukenApi
{
    public class Api
    {
        private readonly HttpClient _httpClient;

        public Api(ServerCredential argServerCredential)
        {
            _httpClient = new HttpClient {BaseAddress = argServerCredential.Uri};
            _httpClient.DefaultRequestHeaders.Authorization = CreateBasicHeader(argServerCredential.Username,
                argServerCredential.Password);
        }

        public async Task<bool> AddTorrent(Uri magnet, AddTorrentParams addTorrentParams = null)
        {
            if (addTorrentParams == null)
            {
                addTorrentParams = new AddTorrentParams();
            }

            var argParams = new object[3];
            argParams[0] = "url";
            argParams[1] = magnet.ToString();
            argParams[2] = addTorrentParams;
            var response = await SendJsonrpcRequest("webui.addTorrent", argParams);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> AddTorrent(byte[] file, AddTorrentParams addTorrentParams = null)
        {
            if (addTorrentParams == null)
            {
                addTorrentParams = new AddTorrentParams();
            }

            var argParams = new object[3];
            argParams[0] = "file";
            argParams[1] = Convert.ToBase64String(file);
            argParams[2] = addTorrentParams;
            var response = await SendJsonrpcRequest("webui.addTorrent", argParams);
            return response.IsSuccessStatusCode;
        }

        private static AuthenticationHeaderValue CreateBasicHeader(string username, string password)
        {
            var byteArray = Encoding.UTF8.GetBytes(username + ":" + password);
            return new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        }

        public async Task<IList<File>> GetFiles(string hash)
        {
            var argParams = new object[1];
            argParams[0] = hash;
            var response = await SendJsonrpcRequest("webui.getFiles", argParams);
            var jsonStr = await response.Content.ReadAsStringAsync();
            var jObject = JObject.Parse(jsonStr);
            var filesRaw = jObject["result"]["files"][1];
            var files = new List<File>();
            foreach (var fileRaw in filesRaw)
            {
                var file = new File();
                file.Name = fileRaw[0].ToString();
                file.Size = (long) fileRaw[1];
                file.Downloaded = (long) fileRaw[2];
                // TODO : Map Priority
                file.Priority = (int) fileRaw[3];
                files.Add(file);
            }

            return files;
        }

        public async Task<IList<Peer>> GetPeers(string hash)
        {
            var argParams = new object[1];
            argParams[0] = hash;
            var response = await SendJsonrpcRequest("webui.getPeers", argParams);
            var jsonStr = await response.Content.ReadAsStringAsync();
            var jObject = JObject.Parse(jsonStr);
            var peersRaw = jObject["result"]["peers"][1];
            var peers = new List<Peer>();
            foreach (var peerRaw in peersRaw)
            {
                var peer = new Peer();

                //peer.Country = peerRaw[0].ToString();
                peer.IPAddress = IPAddress.Parse(peerRaw[1].ToString());
                // TODO : Map the rest
                peers.Add(peer);
            }

            return peers;
        }

        public async Task<Properties> GetProperties(string hash)
        {
            var argParams = new object[1];
            argParams[0] = hash;
            var response = await SendJsonrpcRequest("webui.getProperties", argParams);
            var jsonStr = await response.Content.ReadAsStringAsync();
            var jObject = JObject.Parse(jsonStr);
            var propertiesRaw = jObject["result"]["props"][0];
            var properties = new Properties();

            // TODO : Map the rest
            var trackers =
                propertiesRaw["trackers"].ToString()
                    .Split(new[] {"\n", "\r\n"}, StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => new Uri(t))
                    .ToList();
            properties.Trackers = trackers;

            properties.Label = propertiesRaw["label"]?.ToString();

            return properties;
        }

        public async Task<Settings> GetSettings()
        {
            var response = await SendJsonrpcRequest("webui.getSettings");
            var jsonStr = await response.Content.ReadAsStringAsync();
            var jObject = JObject.Parse(jsonStr);
            var settingsRaw = jObject["result"]["settings"];
            // https://github.com/hadouken/hadouken/blob/master/js/rpc/webui_getSettings.js
            var settings = new Settings();

            foreach (var settingRaw in settingsRaw)
            {
                var settingName = (string) settingRaw[0];
                var settingValue = settingRaw[2];

                switch (settingName)
                {
                    case "bind_port":
                        settings.BindPort = (long) settingValue;
                        break;
                    case "conns_globally":
                        settings.ConnectionsLimit = (long) settingValue;
                        break;
                    case "dht":
                        settings.Dht = (bool) settingValue;
                        break;
                    // TODO : Map the rest
                    default:
                        Debug.WriteLine($"{settingName} setting is not mapped");
                        break;
                }
            }

            return settings;
        }

        public async Task<SystemInfo> GetSystemInfoAsync()
        {
            var response = await SendJsonrpcRequest("core.getSystemInfo");
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var jObject = JObject.Parse(jsonResponse);
            return jObject["result"].ToObject<SystemInfo>();
        }

        public async Task<IList<Torrent>> GetTorrents()
        {
            var response = await SendJsonrpcRequest("webui.list");
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var jObject = JObject.Parse(jsonResponse);
            var torrentsRaw = jObject["result"]["torrents"];

            var torrents = new Torrent[torrentsRaw.Count()];

            for (var i = 0; i < torrents.Length; i++)
            {
                var torrentRaw = torrentsRaw[i];
                var torrent = new Torrent();
                torrent.InfoHash = (string) torrentRaw[0];
                // TODO : State mapping
                torrent.State = torrentRaw[1];
                torrent.Name = (string) torrentRaw[2];
                torrent.TotalSize = (long) torrentRaw[3];
                torrent.Progress = (long) torrentRaw[4]/1000f;
                torrent.DownloadedBytesTotal = (long) torrentRaw[5];
                torrent.UploadedBytesTotal = (long) torrentRaw[6];
                torrent.Ratio = (long) torrentRaw[7]/1000f;
                torrent.UploadRate = (long) torrentRaw[8];
                torrent.DownloadRate = (long) torrentRaw[9];
                var eta = (long) torrentRaw[10];
                if (eta >= 0)
                {
                    torrent.Eta = TimeSpan.FromSeconds(eta);
                }

                torrent.Label = (string) torrentRaw[11];
                // TODO : Map the rest
                torrents[i] = torrent;
            }

            return torrents;
        }

        public async Task<IList<Directory>> ListDirectories()
        {
            var response = await SendJsonrpcRequest("webui.listDirectories");
            var jsonStr = await response.Content.ReadAsStringAsync();
            var jObject = JObject.Parse(jsonStr);
            var dirsRaw = jObject["result"]["download-dirs"];
            // https://github.com/hadouken/hadouken/blob/master/js/rpc/webui_listDirectories.js
            var dirs = new List<Directory>();
            foreach (var dirRaw in dirsRaw)
            {
                var dir = new Directory();
                dir.Available = (long) dirRaw["available"];
                dir.Path = (string) dirRaw["path"];
                dirs.Add(dir);
            }
            return dirs;
        }

        public async Task<bool> Perform(Action action, string hash)
        {
            return await Perform(action, new[] {hash});
        }

        public async Task<bool> Perform(Action action, IEnumerable<string> hashes)
        {
            var argParams = new object[2];
            argParams[0] = action.ToString().ToLower();
            argParams[1] = hashes;
            var response = await SendJsonrpcRequest("webui.perform", argParams);
            // https://github.com/hadouken/hadouken/blob/develop/js/rpc/webui_perform.js
            return response.IsSuccessStatusCode;
        }

        private async Task<HttpResponseMessage> SendJsonrpcRequest(string methodName, object[] argParams = null)
        {
            var request = new JsonrpcRequest
            {
                Jsonrpc = "2.0",
                Id = 1,
                Method = methodName,
                Params = argParams
            };

            var content = new StringContent(JsonConvert.SerializeObject(request));
            return await _httpClient.PostAsync("/api", content);
        }

        public async Task<bool> SetFilePriority(string hash, long fileIndex, int priority)
        {
            return await SetFilePriority(hash, new[] {fileIndex}, priority);
        }

        public async Task<bool> SetFilePriority(string hash, long[] filesIndexes, int priority)
        {
            var argParams = new object[3];
            argParams[0] = hash;
            argParams[1] = filesIndexes;
            argParams[2] = priority;
            var response = await SendJsonrpcRequest("webui.setFilePriority", argParams);
            // https://github.com/hadouken/hadouken/blob/develop/js/rpc/webui_setFilePriority.js
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> SetProperties(string hash, Properties properties)
        {
            return await SetProperties(new[] {hash}, properties);
        }

        public async Task<bool> SetProperties(string[] hashes, Properties properties)
        {
            var argParams = new object[2];
            argParams[0] = hashes;
            // TODO : Map the rest of properties
            // https://github.com/hadouken/hadouken/blob/master/js/rpc/webui_setProperties.js
            argParams[1] = properties;
            var response = await SendJsonrpcRequest("webui.setProperties", argParams);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> SetSettings(Settings settings)
        {
            var argParams = new object[1];
            argParams[0] = settings;
            // TODO : Map the rest of settings
            // https://github.com/hadouken/hadouken/blob/master/js/rpc/webui_setSettings.js
            var response = await SendJsonrpcRequest("webui.setSettings", argParams);

            return response.IsSuccessStatusCode;
        }
    }
}