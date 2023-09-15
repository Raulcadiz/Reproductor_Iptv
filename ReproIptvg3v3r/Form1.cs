using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace ReproIptvg3v3r
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        List<ChannelData> Channels = new List<ChannelData>();


       


        public void ProcessLine(string line1, string line2)
        {
            string processingLine = line1;
            
            ChannelData data = new ChannelData();


            var regxID = new Regex("tvg-id=\"(.*?)\"(.*?)", RegexOptions.Singleline);
            data.ID = regxID.Match(processingLine).Groups[1].Value;

            var regxName = new Regex("tvg-name=\"(.*?)\"", RegexOptions.Singleline);
            data.Name = regxName.Match(processingLine).Groups[1].Value;


            var regxLogoURL = new Regex("tvg-logo=\"(.*?)\"", RegexOptions.Singleline);
            data.LogoURL = regxLogoURL.Match(processingLine).Groups[1].Value;

            var regxGroupTitle = new Regex("group-title=\"(.*?)\"", RegexOptions.Singleline);
            data.GroupTitle = regxGroupTitle.Match(processingLine).Groups[1].Value;

            var splitted = processingLine.Split(",");
            data.VisibleName = splitted[splitted.Length - 1];



            data.StreamUrl = line2;
            
            Channels.Add(data);
        }





        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]

        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);

        private void botonurl_Click(object sender, EventArgs e)
        {
            Uri uri = new Uri(this.url.Text);
            WebClient webClient = new WebClient();
            WebClient myWebClient = webClient;
            string nombreFichero = "ficheroUrl.m3u";
            myWebClient.DownloadFile(uri, nombreFichero);



            string curFile = nombreFichero;

            Channels.Clear();
            string[] lines = System.IO.File.ReadAllLines(curFile);

            for (int i = 0; i < lines.Count() - 1; i++)
            {
                // Detect Start, ignore first line
                if (lines[i].StartsWith("#EXTM3U"))
                {
                    // Do nothing

                }
                if (lines[i].StartsWith("#EXTINF:-1"))
                {
                    ProcessLine(lines[i], lines[i + 1]);
                    i = i + 1;
                }
            }

            // Fill groups
            
            CBGroups.Items.AddRange((from chn in Channels select chn.GroupTitle).Distinct<String>().ToArray<Object>());
        }

        private void botonm3u_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.AddExtension = true;
            ofd.Filter = "M3U Playlist (*.m3u)|*.m3u|All files (*.*)|*.*";
            ofd.ShowDialog();
            string curFile = ofd.FileName;
            CBGroups.Items.Clear();
            lbChannels.Items.Clear();
            Channels.Clear();
            string[] lines = System.IO.File.ReadAllLines(curFile);

            for (int i = 0; i < lines.Count() - 1; i++)
            {
                // Detect Start, ignore first line
                if (lines[i].StartsWith("#EXTM3U"))
                {
                    // Do nothing
                }
                if (lines[i].StartsWith("#EXTINF:-1"))
                {
                    ProcessLine(lines[i], lines[i + 1]);
                    i = i + 1;
                }
            }

            // Fill groups

            CBGroups.Items.AddRange((from chn in Channels select chn.GroupTitle).Distinct<String>().ToArray<Object>());
        }
        public bool IsProcessOpen(string name, ref Process proc)
        {
            //here we're going to get a list of all running processes on
            //the computer
            foreach (Process clsProcess in Process.GetProcesses())
            {
                //now we're going to see if any of the running processes
                //match the currently running processes. Be sure to not
                //add the .exe to the name you provide, i.e: NOTEPAD,
                //not NOTEPAD.EXE or false is always returned even if
                //notepad is running.
                //Remember, if you have the process running more than once, 
                //say IE open 4 times the loop thr way it is now will close all 4,
                //if you want it to just close the first one it finds
                //then add a return; after the Kill
                if (clsProcess.ProcessName.Contains(name))
                {
                    //if the process is found to be running then we
                    //return a true
                    proc = clsProcess;
                    return true;
                }
            }
            //otherwise we return a false
            return false;
        }

        private void lbChannels_SelectedIndexChanged(object sender, EventArgs e)
        {
            
            ChannelData selectedChannel = (ChannelData)lbChannels.SelectedItem;

            Process p = null;
            if (IsProcessOpen("vlc", ref p))
            {
                p.Kill();
            }
            p = new Process();
            p.StartInfo.FileName = @"C:\Program Files\VideoLAN\VLC\vlc.exe";
            p.StartInfo.Arguments = selectedChannel.StreamUrl;
            p.Start();
        }

        private void CBGroups_SelectedIndexChanged(object sender, EventArgs e)
        {
             lbChannels.Items.Clear();
            lbChannels.Items.AddRange((from chn in Channels where chn.GroupTitle == CBGroups.Text select chn).ToArray<Object>());
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void panel2_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void pictureBox6_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }
    }
    public class ChannelData
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string LogoURL { get; set; }
        public string GroupTitle { get; set; }
        public string VisibleName { get; set; }
        public string StreamUrl { get; set; }


        public override string ToString()
        {
            return VisibleName;
        }
    }

}