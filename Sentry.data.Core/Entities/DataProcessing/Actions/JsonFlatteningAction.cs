using Sentry.data.Core.Interfaces.DataProcessing;

namespace Sentry.data.Core.Entities.DataProcessing
{
    public class JsonFlatteningAction : BaseAction
    {
        public JsonFlatteningAction() { }
        public JsonFlatteningAction(IJsonFlatteningAction jsonFlatteningAction) : base(jsonFlatteningAction) { }
    }
}
