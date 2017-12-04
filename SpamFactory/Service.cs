using System.ServiceProcess;
using System.Threading.Tasks;

namespace SpamFactory
{
    public partial class Service
    {

        private Core _myCore;

        protected override void OnStart(string[] args)
        {
            //If your app takes a while to bootstrap, request additional time
            RequestAdditionalTime(30000);

            //Add code here to start your service. This method should set things
            //in motion so your service can do its work.
            _myCore = new Core();
            _myCore.OnStart();

        }

        protected override void OnStop()
        {
            //If your app takes a while to shut down, request additional time
            RequestAdditionalTime(30000);

            //Add code here to perform any tear-down necessary to stop your service.
            _myCore.OnStop();
        }

    }
}