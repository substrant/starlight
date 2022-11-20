using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Starlight.Apis;
using Starlight.Apis.JoinGame;
using Starlight.Bootstrap;
using Starlight.Launch;
using static System.Collections.Specialized.BitVector32;

namespace Starlight.Gui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var jobId = await Starlight.App.Games.ThumbFollow.GetServerId(
                long.Parse(UidBox.Text), long.Parse(PlidBox.Text));
            
            if (jobId == null)
            {
                MessageBox.Show("Invalid user id, place id, or user left game.");
                return;
            }

            var joinReq = new JoinRequest
            {
                ReqType = JoinType.Specific,
                JobId = jobId,
                PlaceId = long.Parse(PlidBox.Text),
            };

            using var session = await Session.LoginAsync(Environment.GetEnvironmentVariable("RBX_AUTH"));
            var launchWnd = new LaunchWindow(session, joinReq);
            launchWnd.Show();
        }

        private async void aaa_Click(object sender, RoutedEventArgs e)
        {
            using var session = await Session.LoginAsync(Environment.GetEnvironmentVariable("RBX_AUTH"));
            var info = new LaunchParams
            {
                AuthStr = await session.GetTicketAsync(),
                AuthType = AuthType.Ticket,
                Request = new JoinRequest
                {
                    ReqType = JoinType.Auto,
                    PlaceId = 4483381587,
                    BrowserTrackerId = 284243248
                },
                RobloxLocale = CultureInfo.CurrentUICulture,
                GameLocale = CultureInfo.CurrentCulture
            };
            
            Process.Start(await info.GetLaunchUriAsync());
        }
    }
}
