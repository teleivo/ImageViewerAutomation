using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ViewerIntegration
{
    public interface IImageViewerAutomationAction
    {
        // think I need to keep this with the action, since Execute() will need this info!
        // but the parsing of the server url part should not be happening in the classes implementing this interface, maybe something for a helper class
        ServerNode ActionOnServer { set;  get; }
        string RegexPatternActionType { get; }
        string RegexPatternActionParameter { get; }

        IImageViewerAutomationAction SetAction(ServerNode actionOnServer, string urlParameters);
        Boolean Execute();
    }
}
