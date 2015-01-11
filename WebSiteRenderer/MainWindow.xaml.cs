using MahApps.Metro.Controls;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace WebSiteRenderer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        string DateTimeStr = "";
        System.Windows.Threading.DispatcherTimer dispatcherTimerJob = new System.Windows.Threading.DispatcherTimer();
        public MainWindow()
        {
            InitializeComponent();
            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();



            
            dispatcherTimerJob.Tick += new EventHandler(dispatcherTimerJob_Tick);
            dispatcherTimerJob.Interval = new TimeSpan(0, 0, 10);
            dispatcherTimerJob.Start();
        }
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            lblTime.Content = DateTime.Now.ToString();
        }
        private void dispatcherTimerJob_Tick(object sender, EventArgs e)
        {
            int StartMinute = int.Parse(ConfigurationSettings.AppSettings["TimeScheduleMinute"].ToString().Trim());
            if (DateTime.Now.Minute >= StartMinute && DateTime.Now.Minute <= StartMinute + 3)
            {
                dispatcherTimerJob.Stop();
                Button_Click(null, null);
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Deleter();
            prWaiting.IsActive = true;
            btnStart.Content = "Started";
            SnapShot();

        }
        protected void SnapShot()
        {
            Process procSnap = new Process();
            procSnap.StartInfo.FileName = "\"" + ConfigurationSettings.AppSettings["SnapEngine"].ToString().Trim() + "\"";
            procSnap.StartInfo.Arguments = " --url=" + "\"" + ConfigurationSettings.AppSettings["WebUrl"].ToString().Trim() + "\"" +
                                       " --min-width=1652  --min-height=2898 --delay=2000 " +
                                       " --out=" + "\"" + ConfigurationSettings.AppSettings["Thumbnail"].ToString().Trim() + "\"";
            procSnap.StartInfo.RedirectStandardError = true;
            procSnap.StartInfo.UseShellExecute = false;
            procSnap.StartInfo.CreateNoWindow = true;
            procSnap.EnableRaisingEvents = true;
            procSnap.StartInfo.RedirectStandardOutput = true;
            procSnap.StartInfo.RedirectStandardError = true;
            procSnap.Exited += procSnap_Exited;

            if (!procSnap.Start())
            {
                return;
            }
        }
        private void procSnap_Exited(object sender, EventArgs e)
        {
            string img = ConfigurationSettings.AppSettings["Thumbnail"].ToString().Trim();
            File.Copy(img, img.Replace("01", "02"), true);
            File.Copy(img, img.Replace("01", "03"), true);
            Renderer();

        }
        protected void Renderer()
        {
            Process proc = new Process();
            proc.StartInfo.FileName = "\"" + ConfigurationSettings.AppSettings["AeRenderPath"].ToString().Trim() + "\"";
            DateTimeStr = string.Format("{0:0000}", DateTime.Now.Year) + "-" + string.Format("{0:00}", DateTime.Now.Month) + "-" + string.Format("{0:00}", DateTime.Now.Day) + "_" + string.Format("{0:00}", DateTime.Now.Hour) + "-" + string.Format("{0:00}", DateTime.Now.Minute) + "-" + string.Format("{0:00}", DateTime.Now.Second);

            DirectoryInfo Dir = new DirectoryInfo(ConfigurationSettings.AppSettings["OutputPath"].ToString().Trim());

            if (!Dir.Exists)
            {
                Dir.Create();
            }

            proc.StartInfo.Arguments = " -project " + "\"" + ConfigurationSettings.AppSettings["AeProjectFile"].ToString().Trim() + "\"" + "   -comp   \"" + ConfigurationSettings.AppSettings["Composition"].ToString().Trim() + "\" -output " + "\"" + ConfigurationSettings.AppSettings["OutputPath"].ToString().Trim() + ConfigurationSettings.AppSettings["OutPutFileName"].ToString().Trim() + "_" + DateTimeStr + ".mp4" + "\"";
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.CreateNoWindow = true;
            proc.EnableRaisingEvents = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.Exited += proc_Exited;

            if (!proc.Start())
            {
                return;
            }
            StreamReader reader = proc.StandardOutput;
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                rtxtLog.Dispatcher.Invoke(
                   new UpdateTextCallback(this.UpdateText),
                   new object[] { line }
               );
            }
            proc.Close();

        }
        void proc_Exited(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(
                   new HideWaitingCallback(this.HideWaiting), new object());
        }
        
        public delegate void UpdateTextCallback(string message);
        private void UpdateText(string message)
        {
            rtxtLog.Document.Blocks.Clear();
            rtxtLog.AppendText(message);
        }
        public delegate void HideWaitingCallback();
        private void HideWaiting()
        {
            prWaiting.IsActive = false;
            rtxtLog.Document.Blocks.Clear();
            rtxtLog.AppendText("Done:" + DateTime.Now.ToString());
            btnStart.Content = "START";
            try
            {
                string StaticDestFileName = ConfigurationSettings.AppSettings["ScheduleDestFileName"].ToString().Trim();
                File.Copy(ConfigurationSettings.AppSettings["OutputPath"].ToString().Trim() + ConfigurationSettings.AppSettings["OutPutFileName"].ToString().Trim() + "_" + DateTimeStr + ".mp4", StaticDestFileName, true);

            }
            catch
            {

            }
            dispatcherTimerJob.Start();
        }
        protected void Deleter()
        {
            string[] FilesList = Directory.GetFiles(ConfigurationSettings.AppSettings["OutputPath"].ToString().Trim());
            foreach (string item in FilesList)
            {
                try
                {
                    if (File.GetLastAccessTime(item) < DateTime.Now.AddHours(-48))
                    {
                        File.Delete(item);
                    }
                }
                catch (Exception Exp)
                {

                }

            }
        }

    }
}