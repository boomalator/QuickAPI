using System.Net;
using System.Net.Sockets;
using System.Diagnostics;


[System.Runtime.InteropServices.DllImport("User32.dll")]
static extern bool ShowWindowAsync(System.Runtime.InteropServices.HandleRef hWnd, int nCmdShow);
[System.Runtime.InteropServices.DllImport("User32.dll")]
static extern bool SetForegroundWindow(IntPtr WindowHandle);
const int SW_RESTORE = 9;

bool FocusProcess(string procName)
{
    // Process[] objProcesses = System.Diagnostics.Process.GetProcessesByName(procName); 
    Process? thisProc = System.Diagnostics.Process.GetProcessesByName(procName).FirstOrDefault();
    if (thisProc is not null)
    {
        Console.WriteLine(thisProc.ProcessName);
        IntPtr hWnd = IntPtr.Zero;
        hWnd = thisProc.MainWindowHandle;
        ShowWindowAsync(new System.Runtime.InteropServices.HandleRef(null, hWnd), SW_RESTORE);
        SetForegroundWindow(hWnd);
        return true;
    }
    else
    {
        return false;
    }

}


var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();


string LocalIPAddress()
{
    using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
    {
        socket.Connect("8.8.8.8", 65530);
        IPEndPoint? endPoint = socket.LocalEndPoint as IPEndPoint;
        if (endPoint != null)
        {
            return endPoint.Address.ToString();
        }
        else
        {
            return "127.0.0.1";
        }
    }
}

string FocusAppResult(string someProc)
{
    if (FocusProcess(someProc))
    {
        return $"Called {someProc} to the foreground at {DateTime.Now:HH:mm:ss}";
    }
    else
    {
        return $"Could not find {someProc} at {DateTime.Now:HH:MM:ss}";
    }
}

string localIP = LocalIPAddress();
Console.WriteLine($"Running on {localIP}.");

app.Urls.Add("http://" + localIP + ":7140");
app.Urls.Add("https://" + localIP + ":7141");
app.Urls.Add("https://*:7141");

app.MapGet("/", () => $"Hello World! This is {localIP}, where it is {DateTime.Now:HH:mm:ss} on {DateTime.Now:m}.");

app.MapGet("/vlc", () => FocusAppResult("vlc"));
app.MapGet("/vw", () => FocusAppResult("VWServer"));
app.MapGet("/playout", () => FocusAppResult("VWServer"));
app.MapGet("/ndi", () => FocusAppResult("Application.Network.StudioMonitor.x64"));


app.Run();