using System;
using noxiousET.src.data.client;
using noxiousET.src.data.paths;

namespace noxiousET.src.control
{
    internal class ClientConfigInfoProvider
    {
        private readonly ClientConfig _clientConfig;
        private readonly Paths _paths;

        public ClientConfigInfoProvider(Paths paths, ClientConfig clientConfig)
        {
            _paths = paths;
            _clientConfig = clientConfig;
        }

        public String[] GetPaths()
        {
            var result = new String[4];

            result[0] = _paths.LogPath;
            result[1] = _paths.ClientPath;
            result[2] = _paths.ConfigPath;
            result[3] = _paths.EveSettingsPath;

            return result;
        }

        public String[] GetConfig()
        {
            var result = new String[4];

            result[0] = Convert.ToString(_clientConfig.TimingMultiplier);
            result[1] = Convert.ToString(_clientConfig.Iterations);
            result[2] = Convert.ToString(_clientConfig.XResolution);
            result[3] = Convert.ToString(_clientConfig.YResolution);

            return result;
        }
    }
}