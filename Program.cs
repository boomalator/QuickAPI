using System.Net;
using System.Net.Sockets;
using System.Diagnostics;





[System.Runtime.InteropServices.DllImport("User32.dll")]
static extern bool ShowWindowAsync(System.Runtime.InteropServices.HandleRef hWnd, int nCmdShow);
[System.Runtime.InteropServices.DllImport("User32.dll")]
static extern bool SetForegroundWindow(IntPtr WindowHandle);
const int SW_RESTORE = 9;



// https://superuser.com/a/1668835
/*
 Browse to the key HKEY_CURRENT_USER\Control Panel\Desktop
 On the right hand side locate and double click on the key ForegroundLockTimeout
 Select the button Decimal and then then type 0 (zero) in the value data box.
*/

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

bool FocusWindow(IntPtr hWnd)
{
    if (hWnd != IntPtr.Zero)
    {
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
builder.Services.AddCors();
var app = builder.Build();

app.UseCors(x => x
    .AllowAnyMethod()
    .AllowAnyHeader()
    .SetIsOriginAllowed(origin => true) // allow any origin  
    .AllowCredentials()                 // allow credentials 
    );


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
        return $"Could not find {someProc} at {DateTime.Now:HH:mm:ss}";
    }
}

string FocusWindowResult(string someTitle)
{
    bool success = false;

    IntPtr hwnd = WindowFinder.FindWindow(someTitle);
    if (hwnd != IntPtr.Zero )
    {
        if (FocusWindow(hwnd))
        {
            success = true;
        }
    }
    if (success)
    {
        return $"Called {someTitle} to the foreground at {DateTime.Now:HH:mm:ss}";
    }
    else
    {
        return $"Could not find {someTitle} at {DateTime.Now:HH:mm:ss}";
    }
}



string localIP = LocalIPAddress();
Console.WriteLine($"Running on {localIP}.");

app.Urls.Add("http://" + localIP + ":7140");
app.Urls.Add("https://" + localIP + ":7141");
app.Urls.Add("https://*:7141");

var x = WindowFinder.FindWindow("NDI Studio Monitor");
Console.WriteLine($"NDI: {x}");
x = WindowFinder.FindWindow("ByteHive Playoutbee v2");
Console.WriteLine($"Playoutbee: {x}");


app.MapGet("/", () => $"Hello World! This is {localIP}, where it is {DateTime.Now:HH:mm:ss} on {DateTime.Now:m}.");

app.MapGet("/vlc", () => FocusAppResult("vlc"));
app.MapGet("/vw", () => FocusAppResult("VWServer"));
app.MapGet("/playout", () => FocusWindowResult("ByteHive Playoutbee v2"));
app.MapGet("/ndi", () => FocusAppResult("Application.Network.StudioMonitor.x64"));


app.Run();



public class WindowFinder
{
    // For Windows Mobile, replace user32.dll with coredll.dll
    [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    public static IntPtr FindWindow(string caption)
    {
        return FindWindow(null, caption);
    }
}