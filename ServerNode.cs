using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ViewerIntegration
{
    public class ServerNode
    {
        public readonly string aeTitle;
        public readonly string ipAddress;
        public readonly int port;
        public readonly bool isStreaming;
        public readonly bool isLocal;

        // Constructor initializing Pacs with all its fields
        public ServerNode(string aeTitlePar, string ipAddressPar, int portPar, bool isStreamingPar, bool isLocalPar)
        {
            aeTitle = aeTitlePar;
            ipAddress = ipAddressPar;
            port = portPar;
            isLocal = isLocalPar;
        }
    }
}
