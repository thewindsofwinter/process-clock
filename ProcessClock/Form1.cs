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

        DateTime start;
        DateTime end;
        DateTime empty;
        
        String currprocess = null;
        String path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        String[] colors = { "#CC1414", "#B3B312", "#12B312", "#0F9999", "#1414CC", "#B312B3" };

        public void LoadData(String dir)
        {

            // Read user-defined mapping options: create option file if it does not exist
            if (!System.IO.File.Exists(dir + "\\options.txt"))
            {
                System.IO.File.Create(dir + "\\options.txt");
            }
            else
            {
                try
                {
                    using (StreamReader sr = new StreamReader(dir + "\\options.txt"))
                    {
                        String s;
                        while ((s = sr.ReadLine()) != null)
                        {
                            // Delimit based on mapping
                            String[] arr = s.Replace(" maps to ", "~").Split('~');
                            mapping.Add(arr[0], arr[1]);
                            // Log.Text += "Loaded data: Name " + arr[0] + " mapped to " + arr[1] + "\r\n";
                        }
                    }
                }
                catch (IOException e)
                {
                    // Log.Text += "LOAD DATA FAILED\r\n";
                    Console.WriteLine("The file could not be read:");
                    Console.WriteLine(e.Message);
                }
            }

            // Log.Text += subPath + "\\" + curr.Month + "-" + curr.Day + ".txt" + "\r\n";
            // Log.Text += (System.IO.File.Exists(subPath + "\\" + curr.Month + "-" + curr.Day + ".txt")) + "\r\n";

            // Check if there is already data for the current day: if so, intake data
            if (System.IO.File.Exists(dir + "\\" + curr.Month + "-" + curr.Day + ".txt"))
            {
                try
                {   // Open the text file using a stream reader.
                    using (StreamReader sr = new StreamReader(dir + "\\" + curr.Month + "-" + curr.Day + ".txt"))
                    {
                        // Read the heading first
                        sr.ReadLine();

                        String s;
                        while ((s = sr.ReadLine()) != null)
                        {
                            // Split based on special delimiter
                            String[] arr = s.Replace(": ", "~").Split('~');
                            TimeSpan val = TimeSpan.Parse(arr[1]);
                            if (arr[0] != "Total")
                            {
                                dict.Add(arr[0], val);
                            }

                            if (mapping.ContainsKey(arr[0]))
                                arr[0] = mapping[arr[0]];

                            // Log.Text += "Loaded data: Application " + arr[0] + " used for " + arr[1] + "\r\n";
                        }
                    }
                }
                catch (IOException e)
                {
                    // Log.Text += "LOAD DATA FAILED\r\n";
                    Console.WriteLine("The file could not be read:");
                    Console.WriteLine(e.Message);
                }
            }
        }

        public void WriteData()
        {
            DateTime now = DateTime.Now;
            TimeSpan total = TimeSpan.Zero;

            System.IO.File.WriteAllText(path + "\\ProcessClock\\" + now.Month + "-" + now.Day + ".txt", String.Empty);
            using (System.IO.StreamWriter file =
        new System.IO.StreamWriter(path + "\\ProcessClock\\" + now.Month + "-" + now.Day + ".txt"))
            {
                file.WriteLine("Time spent on processes:");
                foreach (String process in dict.Keys)
                {
                    file.WriteLine(process + ": " + dict[process]);
                    total = total.Add(dict[process]);
                }
                file.WriteLine("Total: " + total);
            }

            // Log.Text += "Successfully wrote data to file!\r\n";
        }

        public void WriteDataPast()
        {
            // For ensuring that writing of data at midnight works out
            DateTime past = DateTime.Now.AddMinutes(-4);
            TimeSpan total = TimeSpan.Zero;

            System.IO.File.WriteAllText(path + "\\ProcessClock\\" + past.Month + "-" + past.Day + ".txt", String.Empty);
            using (System.IO.StreamWriter file =
        new System.IO.StreamWriter(path + "\\ProcessClock\\" + past.Month + "-" + past.Day + ".txt"))
            {
                file.WriteLine("Time spent on processes:");
                foreach (String process in dict.Keys)
                {
                    file.WriteLine(process + ": " + dict[process]);
                    total = total.Add(dict[process]);
                }
                file.WriteLine("Total: " + total);
            }

            // Log.Text += "Successfully wrote data to file!\r\n";
        }

        // Schedule actions during certain times -- for switching between files at midnight
        public void ScheduleAction(Action action, DateTime ExecutionTime)
        {
            Task WaitTask = Task.Delay((int)ExecutionTime.Subtract(DateTime.Now).TotalMilliseconds);
            WaitTask.ContinueWith(_ => action);
        }


        public Form1()
        {
            // Create a folder to store ProcessClock files if it doesn't already exist
            string subPath = path + "\\ProcessClock"; 
            this.Icon = new Icon(path + "\\source\\repos\\ProcessClock\\icon\\icon.ico");


            bool exists = System.IO.Directory.Exists(subPath);

            if (!exists)
                System.IO.Directory.CreateDirectory(subPath);


            // Initialize components and variables, make components redraw on resize
            InitializeComponent();
            ResizeRedraw = true;
            dict = new Dictionary<String, TimeSpan>();
            mapping = new Dictionary<String, String>();

            // Set maximum date on options
            DateTime now = DateTime.Now;
            startDateTime.MaxDate = now;
            endDateTime.MaxDate = now;
            InfoPanel.Invalidate(true);

            LoadData(subPath);

            // Set current process
            currprocess = "ProcessClock";
            dele = new WinEventDelegate(WinEventProc);
            IntPtr m_hhook = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, dele, 0, 0, WINEVENT_OUTOFCONTEXT);

            // Make sure that stuff after midnight doesn't spill over
            Action switchFilesAtMidnight = () => recordWindowSwitch(true);
            DateTime nextMidnight = DateTime.Today.AddDays(1);

            ScheduleAction(switchFilesAtMidnight, nextMidnight);
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

        public void recordWindowSwitch(Boolean makeNewFile)
        {
            String s = GetActiveProcessName();

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

            // Log.Text += "Successfully added duration " + diff + " to process " + displayname + "\r\n";

            // If this is not a "midnight" change, only write data if ProcessClock works
            if (!makeNewFile)
            {
                // If one opens the ProcessClock window, the program will automatically write all data to the day's file
                if (s.Equals("ProcessClock"))
                {
                    WriteData();

                    // Redraw static data graph
                    DrawPanel.Invalidate(true);
                }
            }
            else
            {
                // Create a folder to store ProcessClock files if it doesn't already exist
                string subPath = path + "\\ProcessClock";

                // Write all remaining data
                WriteDataPast();

                // Clear current memory
                dict = new Dictionary<String, TimeSpan>();
                mapping = new Dictionary<String, String>();

                // Prepare subpath
                LoadData(subPath);

                // Schedule next action for next midnight
                Action switchFilesAtMidnight = () => recordWindowSwitch(true);
                DateTime nextMidnight = DateTime.Today.AddDays(1);

                ScheduleAction(switchFilesAtMidnight, nextMidnight);
            }

            currprocess = s;
            curr = DateTime.Now;
        }

        public void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            recordWindowSwitch(false);
        }

        private void InfoPanel_Paint(object sender, PaintEventArgs e)
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
            int width = InfoPanel.Width;
            int height = InfoPanel.Height;
            Font titleFont = new Font("Garamond", 24);
            Font labelFont = new Font("Tahoma", 10);
            SolidBrush graphBrush = new SolidBrush(Color.Black);
            Rectangle panelArea = new Rectangle(0, 0, width, height / 6);


            // Format for title
            StringFormat titleFormat = new StringFormat();
            titleFormat.Alignment = StringAlignment.Center;
            titleFormat.LineAlignment = StringAlignment.Center;

            // Get the total amount of time spent on applications
            TimeSpan total = TimeSpan.Zero;


            // Store current y-values for graph and legend
            int y = height / 6;
            int legend = height / 6;

            // Range of y-values for graph and iterator
            int all = height * 5 / 6 - 15;

            // Check if there is any data to paint
            if (start.Equals(empty) && end.Equals(empty))
            {
                // Probably won't be needed much
                graph.DrawString("Historical Data", titleFont, graphBrush, panelArea, titleFormat);

                graph.DrawString("No Data Requested", labelFont, graphBrush, 20, y);
            }
            else
            {

                if (start.CompareTo(end) > 0)
                {
                    graph.DrawString("Historical Data", labelFont, graphBrush, panelArea, titleFormat);
                }

                

            }
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
            Font labelFont = new Font("Garamond", 24);
            SolidBrush graphBrush = new SolidBrush(Color.Black);
            Rectangle panelArea = new Rectangle(0, 0, width, height/6);

            graph.DrawLine(new Pen(graphBrush), 1, 0, 1, height);

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

            List<KeyValuePair<String, TimeSpan>> entries = dict.ToList();
            entries.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));
            
            // If there is no data recorded or total data is less than FromMinutes, note that
            if (dict.Count == 0 || total.CompareTo(TimeSpan.FromMinutes(2)) < 0)
            {
                // Probably won't be needed much
                graph.DrawString("No significant data", labelFont, graphBrush, panelArea, titleFormat);
            }
            else
            {

                graph.DrawString("Process Usage Data", labelFont, graphBrush, panelArea, titleFormat);

                // Store current y-values for graph and legend
                int y = height / 6;
                int legend = height / 6;

                // Range of y-values for graph and iterator
                int all = height * 5 / 6 - 15;
                int iter = 0;

                foreach(KeyValuePair<String, TimeSpan> p in entries) {
                    // Get percentages for time spent
                    double frac =  p.Value.TotalMilliseconds / total.TotalMilliseconds;

                    // Height of current part of bar
                    int end = (int)Math.Round(all * frac);

                    // Get color for graph
                    // Log.Text += colors[iter];

                    // Parse hex into C# color object
                    if (iter < 6)
                    {
                        graphBrush = new SolidBrush(Color.FromArgb(int.Parse(colors[iter].Substring(1, 2), System.Globalization.NumberStyles.HexNumber),
                            int.Parse(colors[iter].Substring(3, 2), System.Globalization.NumberStyles.HexNumber),
                            int.Parse(colors[iter].Substring(5, 2), System.Globalization.NumberStyles.HexNumber)));

                        // Draw part of bar and legend (small gap between bar elements)
                        graph.FillRectangle(graphBrush, new Rectangle(width / 16, y, width / 8, end - 3));
                        graph.FillRectangle(graphBrush, width / 4, legend, 10, 10);
                    }
                    else
                    {
                        graphBrush = new SolidBrush(Color.FromArgb(100, 100, 100));

                        // Draw part of bar and legend (no gap between bar elements)
                        graph.FillRectangle(graphBrush, new Rectangle(width / 16, y, width / 8, end));
                        graph.FillRectangle(graphBrush, width / 4, legend, 10, 10);
                    }


                    // Write label for graph
                    labelFont = new Font("Tahoma", 10);
                    graphBrush = new SolidBrush(Color.Black);

                    String info = p.Value.ToString();
                    String displayname = p.Key;
                    String hhmmss = info.Substring(0, 2) + " h " + info.Substring(3, 2) + " m "
                        + info.Substring(6, 2) + " s " + info.Substring(9, 3) + " ms";

                    // If the app name has been mapped, change it
                    if (mapping.ContainsKey(p.Key))
                    {
                        displayname = mapping[p.Key];
                    }

                    graph.DrawString(displayname + ": " + hhmmss, labelFont, graphBrush, width / 4 + 20, legend);

                    // Increase y-values and iterator
                    legend += 15;
                    y += end;
                    iter++;
                }

                String t = total.ToString();
                String time = t.Substring(0, 2) + " h " + t.Substring(3, 2) + " m "
                    + t.Substring(6, 2) + " s " + t.Substring(9, 3) + " ms";

                graph.DrawString("Total: " + time, labelFont, graphBrush, width / 4 + 20, legend);

            }

            graphBrush.Dispose();
            graph.Dispose();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
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
            WriteData();
        }

        private void queryButton_Click(object sender, EventArgs e)
        {
            // Assign new start and end date time values and invalidate InfoPanel
            start = startDateTime.Value;
            end = endDateTime.Value;
            InfoPanel.Invalidate(true);
        }
    }
}
