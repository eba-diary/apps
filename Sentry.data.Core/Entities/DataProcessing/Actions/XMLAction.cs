using Sentry.data.Core.Interfaces.DataProcessing;

namespace Sentry.data.Core.Entities.DataProcessing
{
    public class XMLAction : BaseAction
    {
        public XMLAction() { }
        public XMLAction(IXMLAction xmlAction) : base(xmlAction) { }
    }
}
