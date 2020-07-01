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
            }

            currprocess = s;
            curr = now;
        }
    }
}
