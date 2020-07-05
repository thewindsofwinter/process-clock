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

        /**
         * Global variables:
             * curr - current time
             * dict - stores time spent on each application
             * mapping - maps exe names to user-defined names (i.e. one could map "Taskmgr" to "Task Manager")
             * currprocess - stores the process currently in focus
             * path - path to user documents folder
             * colors - colors to be used in graph
         */
        DateTime curr = DateTime.Now;
        Dictionary<String, TimeSpan> dict = null;
        Dictionary<String, String> mapping = null;
        String currprocess = null;
        String path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        String[] colors = { "E62817", "E67E17", "E6D417", "A1E617", "4BE617", "17E639", "17E68F",
            "17E6E6", "178FE6", "1739E6", "4B17E6" };


        public Form1()
        {
            // Create a folder to store ProcessClock files if it doesn't already exist
            string subPath = path + "\\ProcessClock";

            bool exists = System.IO.Directory.Exists(subPath);

            if (!exists)
                System.IO.Directory.CreateDirectory(subPath);


            // Initialize components and variables, make components redraw on resize
            InitializeComponent();
            ResizeRedraw = true;
            dict = new Dictionary<String, TimeSpan>();
            mapping = new Dictionary<String, String>();

            // Read user-defined mapping options: create option file if it does not exist
            if (!System.IO.File.Exists(subPath + "\\options.txt"))
            {
                System.IO.File.Create(subPath + "\\options.txt");
            }
            else
            {
                try
                {
                    using (StreamReader sr = new StreamReader(subPath + "\\options.txt"))
                    {
                        String s;
                        while ((s = sr.ReadLine()) != null)
                        {
                            // Delimit based on mapping
                            String[] arr = s.Replace(" maps to ", "~").Split('~');
                            mapping.Add(arr[0], arr[1]);
                            Log.Text += "Loaded data: Name " + arr[0] + " mapped to " + arr[1] + "\r\n";
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

            // Log.Text += subPath + "\\" + curr.Month + "-" + curr.Day + ".txt" + "\r\n";
            // Log.Text += (System.IO.File.Exists(subPath + "\\" + curr.Month + "-" + curr.Day + ".txt")) + "\r\n";
            
            // Check if there is already data for the current day: if so, intake data
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

                            if (mapping.ContainsKey(arr[0]))
                                arr[0] = mapping[arr[0]];

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

            // Set current process
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

            // Update the amount of time spent on processes
            System.TimeSpan diff = DateTime.Now.Subtract(curr);
            if (dict.ContainsKey(currprocess))
            {
                dict[currprocess] = dict[currprocess].Add(diff);
            }
            else
            {
                dict.Add(currprocess, diff);
            }

            String displayname = currprocess;
            if (mapping.ContainsKey(currprocess))
                displayname = mapping[currprocess];

            Log.Text += "Successfully added duration " + displayname + " to process " + currprocess + "\r\n";

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
                        file.WriteLine(process + ": " + dict[process]);
                    }
                }
                Log.Text += "Successfully wrote data to file!\r\n";

                // Redraw static data graph
                this.Invalidate(true);
            }

            currprocess = s;
            curr = now;
        }

        private void DrawPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics graph;

            graph = e.Graphics;

            /**
             * Variables to draw static graph:
                * width - width of panel
                * height - height of panel
                * labelFont - font for drawing labels, may be modified
                * graphBrush - color used in graph
                * panelArea - area used for graph title
             */
            int width = DrawPanel.Width;
            int height = DrawPanel.Height;
            Font labelFont = new Font("Garamond", 18);
            SolidBrush graphBrush = new SolidBrush(Color.Black);
            Rectangle panelArea = new Rectangle(0, 0, width, height/4);

            // Format for title
            StringFormat titleFormat = new StringFormat();
            titleFormat.Alignment = StringAlignment.Center;
            titleFormat.LineAlignment = StringAlignment.Center;

            // Get the total amount of time spent on applications
            TimeSpan total = TimeSpan.Zero;
            foreach (String process in dict.Keys)
            {
                // Log.Text += dict[process] + " ";
                total = total.Add(dict[process]);
            }

            // Log.Text += "TOTAL: " + total + "\r\n";

            // If there is no data recorded or total data is less than FromMinutes, note that
            if (dict.Count == 0 || total.CompareTo(TimeSpan.FromMinutes(2)) < 0)
            {
                // Probably won't be needed much
                graph.DrawString("No significant data", labelFont, graphBrush, panelArea, titleFormat);
            }
            else
            {

                graph.DrawString("Process Usage Data", labelFont, graphBrush, panelArea, titleFormat);

                // Store current y-values for graph and 
                int y = height / 4;
                int legend = height / 4;

                // Range of y-values for graph and iterator
                int all = height * 3 / 4 - 15;
                int iter = 0;

                foreach(String data in dict.Keys) {
                    // Get percentages for time spent
                    double frac = dict[data].TotalMilliseconds / total.TotalMilliseconds;
                    // Height of current part of bar
                    int end = (int)Math.Round(all * frac);

                    // Get color for graph
                    String currcolor = colors[iter];
                    // Log.Text += colors[iter];
                    
                    // Parse hex into C# color object
                    graphBrush = new SolidBrush(Color.FromArgb(int.Parse(colors[iter].Substring(0,2), System.Globalization.NumberStyles.HexNumber),
                        int.Parse(colors[iter].Substring(2, 2), System.Globalization.NumberStyles.HexNumber),
                        int.Parse(colors[iter].Substring(4, 2), System.Globalization.NumberStyles.HexNumber)));

                    // Draw part of bar and legend
                    graph.FillRectangle(graphBrush, new Rectangle(width / 16, y, width / 8, end));
                    graph.FillRectangle(graphBrush, width / 4, legend, 10, 10);

                    // Write label for graph
                    labelFont = new Font("Tahoma", 10);
                    graphBrush = new SolidBrush(Color.Black);

                    String info = dict[data].ToString();
                    String displayname = data;
                    String hhmmss = info.Substring(0, 2) + " h " + info.Substring(3, 2) + " m "
                        + info.Substring(6, 2) + " s " + info.Substring(9, 3) + " ms";

                    // If the app name has been mapped, change it
                    if (mapping.ContainsKey(data))
                        displayname = mapping[data];

                    graph.DrawString(displayname + ": " + hhmmss, labelFont, graphBrush, width / 4 + 20, legend);

                    // Increase y-values and iterator
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
