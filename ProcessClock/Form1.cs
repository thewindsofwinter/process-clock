using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProcessClock
{
    public partial class Form1 : Form
    {


        WinEventDelegate dele = null;
        DateTime curr = DateTime.Now;
        Dictionary<String, TimeSpan> dict = null;
        String currprocess = null;
        String path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        String[] colors = { "E62817", "E67E17", "E6D417", "A1E617", "4BE617", "17E639", "17E68F",
            "17E6E6", "178FE6", "1739E6", "4B17E6" };


        public Form1()
        {
            string subPath = path + "\\ProcessClock";

            bool exists = System.IO.Directory.Exists(subPath);

            if (!exists)
                System.IO.Directory.CreateDirectory(subPath);

            InitializeComponent();
            dict = new Dictionary<String, TimeSpan>();

            Log.Text += subPath + "\\" + curr.Month + "-" + curr.Day + ".txt" + "\r\n";
            Log.Text += (System.IO.File.Exists(subPath + "\\" + curr.Month + "-" + curr.Day + ".txt")) + "\r\n";
            if(System.IO.File.Exists(subPath + "\\" + curr.Month + "-" + curr.Day + ".txt"))
            {
                try
                {   // Open the text file using a stream reader.
                    using (StreamReader sr = new StreamReader(subPath + "\\" + curr.Month + "-" + curr.Day + ".txt"))
                    {
                        // Read the heading first
                        sr.ReadLine();

                        String s;
                        while((s = sr.ReadLine()) != null)
                        {
                            // Split based on special delimiter
                            String[] arr = s.Replace(": ", "~").Split('~');
                            TimeSpan val = TimeSpan.Parse(arr[1]);
                            dict.Add(arr[0], val);
                            Log.Text += "Loaded data: Application " + arr[0] + " used for " + arr[1] + "\r\n";
                        }
                    }
                }
                catch (IOException e)
                {
                    Log.Text += "LOAD DATA FAILED\r\n";
                    Console.WriteLine("The file could not be read:");
                    Console.WriteLine(e.Message);
                }
            }

            currprocess = "ProcessClock";
            dele = new WinEventDelegate(WinEventProc);
            IntPtr m_hhook = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, dele, 0, 0, WINEVENT_OUTOFCONTEXT);
        }

        delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        [DllImport("user32.dll")]
        static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        private const uint WINEVENT_OUTOFCONTEXT = 0;
        private const uint EVENT_SYSTEM_FOREGROUND = 3;

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint ProcessId);

        string GetActiveProcessName()
        {
            IntPtr hwnd = GetForegroundWindow();
            uint pid;
            GetWindowThreadProcessId(hwnd, out pid);
            Process p = Process.GetProcessById((int)pid);

            return p.ProcessName;
        }

        public void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            String s = GetActiveProcessName();
            DateTime now = DateTime.Now;

            System.TimeSpan diff = DateTime.Now.Subtract(curr);
            if (dict.ContainsKey(currprocess))
            {
                dict[currprocess] = dict[currprocess].Add(diff);
            }
            else
            {
                dict.Add(currprocess, diff);
            }
            Log.Text += "Successfully added duration " + diff + " to process " + currprocess + "\r\n";

            // If one opens the ProcessClock window, the program will automatically write all data to the day's file
            if (s.Equals("ProcessClock"))
            {
                System.IO.File.WriteAllText(path + "\\ProcessClock\\" + now.Month + "-" + now.Day + ".txt", String.Empty);
                using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(path + "\\ProcessClock\\" + now.Month + "-" + now.Day + ".txt"))
                {
                    file.WriteLine("Time spent on processes:");
                    foreach (String process in dict.Keys)
                    {
                        if (!process.Equals("ProcessClock"))
                        {
                            file.WriteLine(process + ": " + dict[process]);
                        }
                    }
                }
                Log.Text += "Successfully wrote data to file!\r\n";
                this.Invalidate(true);
            }

            currprocess = s;
            curr = now;
        }

        private void DrawPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics graph;

            graph = e.Graphics;
            int width = DrawPanel.Width;
            int height = DrawPanel.Height;
            Font labelFont = new Font("Garamond", 18);
            SolidBrush graphBrush = new SolidBrush(Color.Black);
            Rectangle panelArea = new Rectangle(0, 0, width, height/4);
            Rectangle graphArea = new Rectangle(0, height / 4, width / 2, height * 3 / 4);
            Rectangle labelArea = new Rectangle(width / 2, height / 4, width / 2, height * 3 / 4);

            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;

            TimeSpan total = TimeSpan.Zero;
            foreach (String process in dict.Keys)
            {
                // Log.Text += dict[process] + " ";
                total = total.Add(dict[process]);
            }

            // Log.Text += "TOTAL: " + total + "\r\n";

            // If there is no data recorded or total data is less than FromMinutes
            if (dict.Count == 0 || total.CompareTo(TimeSpan.FromMinutes(2)) < 0)
            {
                // Probably won't be needed much
                graph.DrawString("No significant data", labelFont, graphBrush, panelArea, stringFormat);
            }
            else
            {

                graph.DrawString("Process Usage Data", labelFont, graphBrush, panelArea, stringFormat);


                int y = height / 4;
                int all = height * 3 / 4 - 15;
                int iter = 0;
                int legend = height / 4;

                foreach(String data in dict.Keys) {
                    double frac = dict[data].TotalMilliseconds / total.TotalMilliseconds;
                    int end = (int)Math.Round(all * frac);

                    String currcolor = colors[iter];
                    // Log.Text += colors[iter];
                    
                    graphBrush = new SolidBrush(Color.FromArgb(int.Parse(colors[iter].Substring(0,2), System.Globalization.NumberStyles.HexNumber),
                        int.Parse(colors[iter].Substring(2, 2), System.Globalization.NumberStyles.HexNumber),
                        int.Parse(colors[iter].Substring(4, 2), System.Globalization.NumberStyles.HexNumber)));


                    graph.FillRectangle(graphBrush, new Rectangle(width / 12, y, width / 6, end));
                    graph.FillRectangle(graphBrush, width / 3, legend, 10, 10);

                    labelFont = new Font("Tahoma", 10);
                    graphBrush = new SolidBrush(Color.Black);

                    String info = dict[data].ToString();
                    String hhmmss = info.Substring(0, 2) + " h " + info.Substring(3, 2) + " m "
                        + info.Substring(6, 2) + " s " + info.Substring(9, 3) + " ms";
                    graph.DrawString(data + ": " + hhmmss, labelFont, graphBrush, width / 3 + 20, legend);

                    legend += 15;
                    y += end;
                    iter++;
                }
            }

            graphBrush.Dispose();
            graph.Dispose();
        }
    }
}
