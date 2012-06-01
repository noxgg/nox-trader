using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using noxiousET.src.data.client;
using noxiousET.src.data.paths;

namespace noxiousET.src.control
{
    class ClientConfigInfoProvider
    {
        Paths paths;
        ClientConfig clientConfig;

        public ClientConfigInfoProvider(Paths paths, ClientConfig clientConfig)
        {
            this.paths = paths;
            this.clientConfig = clientConfig;
        }

        public String[] getPaths()
        {
            String[] result = new String[3];

            result[0] = paths.logPath;
            result[1] = paths.clientPath;
            result[2] = paths.configPath;

            return result;
        }

        public String[] getConfig()
        {
            String[] result = new String[4];

            result[0] = Convert.ToString(clientConfig.timingMultiplier);
            result[1] = Convert.ToString(clientConfig.iterations);
            result[2] = Convert.ToString(clientConfig.xResolution);
            result[3] = Convert.ToString(clientConfig.yResolution);

            return result;
        }
    }
}
