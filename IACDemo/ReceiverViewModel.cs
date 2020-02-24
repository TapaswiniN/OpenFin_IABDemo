using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace IACDemo
{
    class ReceiverViewModel : INotifyPropertyChanged
    {
        private MessageChannel dataMessageChannel;
        private String _msg;
        public String Msg { get { return _msg; } set { _msg = value; this.OnPropertyChanged("Msg"); } }
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(String str) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(str));

        public ReceiverViewModel()
        {
            var exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var exeDir = System.IO.Path.GetDirectoryName(exePath);
            var runtimeOptions = new Openfin.Desktop.RuntimeOptions
            {
                UUID = "receiver-uuid",
                EnableRemoteDevTools = false,
                RemoteDevToolsPort = 9090,
                AssetsPath = exeDir+"\\Assets"

            };

            var runtime = Openfin.Desktop.Runtime.GetRuntimeInstance(runtimeOptions);

            runtime.Error += (sender, e) =>
            {
                Console.Write(e);
            };

            runtime.Connect(() =>
            {
                // Initialize the communication channel after the runtime has connected
                // but before launching any applications or EmbeddedViews
                dataMessageChannel = new MessageChannel(runtime.InterApplicationBus, "user-data");
                dataMessageChannel.MessageReceived += DataMessageChannel_MessageReceived;
            });
        }

        private void DataMessageChannel_MessageReceived(object sender, MessageChannel.MessageReceivedEventArgs e)
        {
            Msg = Msg + "\n " +e.Data.ToString() +" message received from: " + e.SourceUuid + " @" + DateTime.Now.ToString("hh.mm.ss.ffffff");
        }
    }
}
