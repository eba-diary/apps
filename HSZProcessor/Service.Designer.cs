using System;
using System.ServiceProcess;

namespace HSZGOLDENEYE
{
    partial class Service : ServiceBase
    {

        //UserService overrides dispose to clean up the component list.
        [System.Diagnostics.DebuggerNonUserCode()]
        protected override void Dispose(Boolean disposing)
        {
            try
            {
                if (disposing && components != null)
                {
                    components.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        //Required by the Component Designer
        private System.ComponentModel.IContainer components;

        // NOTE: The following procedure is required by the Component Designer
        // It can be modified using the Component Designer.  Do not modify it
        // using the code editor.
        [System.Diagnostics.DebuggerStepThrough()]
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            this.ServiceName = "WindowsService";
        }

    }
}