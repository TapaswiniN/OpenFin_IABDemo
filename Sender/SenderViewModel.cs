using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Utilities;

namespace Sender
{
    class SenderViewModel : INotifyPropertyChanged
    {
        private MessageChannel dataMessageChannel;
        private String _msg;
        private bool _canSendMsg = false;
        private String _label;
        public String Msg { get { return _msg; } set { _msg = value; this.OnPropertyChanged("Msg"); } }

        public String Label { get { return _label; } set { _label = value; OnPropertyChanged("Label"); } }
        private Timer _timer;
        private ICommand _cmdAction;
        public ICommand CmdAction
        {
            get
            { if (_cmdAction == null) { _cmdAction = new RelayCommand(Operate); } return _cmdAction; } 
        }
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(String str) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(str));

        public SenderViewModel()
        {
            Label = "Start Sending Messages";
            var exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var exeDir = System.IO.Path.GetDirectoryName(exePath);
            var runtimeOptions = new Openfin.Desktop.RuntimeOptions
            {
               UUID="sender-uuid",
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
                dataMessageChannel.CurrentUuid = "sender-uuid";
                dataMessageChannel.MessageReceived += DataMessageChannel_MessageReceived;
            });
        }

        private void DataMessageChannel_MessageReceived(object sender, MessageChannel.MessageReceivedEventArgs e)
        {
            Msg = Msg + "\n message received from: " + e.SourceUuid + " @" + DateTime.Now.ToString("hh.mm.ss.ffffff"); 
        }
        private void Operate()
        {
            if(_canSendMsg)
            { _canSendMsg = false; Label = "Start Sending Messages"; }
            else
            {
                _canSendMsg = true; Label = "Stop Sending Messages";
            }
            if(_timer!=null)
            {
                _timer.Dispose();
            }
            _timer = new Timer(SendMessage, null, 0, 100);
           
        }
        private void SendMessage(object obj)
        {
            if (dataMessageChannel == null) { if (_timer != null) _timer.Dispose(); }
            Msg = Msg+"\n" + "PING message sent to receiver-uuid @"+ DateTime.Now.ToString("hh.mm.ss.ffffff");
            dataMessageChannel.SendData("PING");

        }

    }
}
