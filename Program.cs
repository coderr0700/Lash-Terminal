using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Linq;
using System.Net.Http;
using System.ComponentModel;
using System.Collections;
using System.Linq.Expressions;
using System.Threading;
using System.Security.Cryptography;
using System.Text;
// Program
class Program
{
    static void Line()
    {
        Console.WriteLine();
    }
    static void Help()
    {
        Console.WriteLine("Lash Terminal - Commands");
        Line();
        // basic commands
        Console.WriteLine("Basic:");
        Console.WriteLine("  help                         Shows this help menu");
        Console.WriteLine("  clr                          Clears the screen");
        Console.WriteLine("  exit                         Exits Lash");
        Console.WriteLine("  ln                           Prints a blank line");
        Console.WriteLine("  out <text>                   Prints text");
        Console.WriteLine("  in                           Reads user input");
        Console.WriteLine("  read <text>                  Checks if input matches text");
        // system commands
        Line();
        Console.WriteLine("System Info:");
        Console.WriteLine("  machinename                  Shows computer name");
        Console.WriteLine("  username                     Shows username");
        Console.WriteLine("  userPath                     Shows user path");
        Line();
        // timestamps commands
        Console.WriteLine("Timestamps:");
        Console.WriteLine("  localtime                    Shows detailed local time");
        Console.WriteLine("  datetime                     Shows date and time");
        Console.WriteLine("  date                         Shows date");
        Console.WriteLine("  time                         Shows time");
        Console.WriteLine("  uptime                       Shows system uptime");
        Console.WriteLine("  bootingtime                  Shows estimated boot time");
        // file co
        // mmands
        Line();
        Console.WriteLine("Files / Paths:");
        Console.WriteLine("  cd <path>                    Changes directory");
        Console.WriteLine("  explorer <path>              Opens a folder");
        Console.WriteLine("  begin <file>                 Runs a file after warning");
        Console.WriteLine("  del <path>                   Deletes a file");
        // dev commands
        Line();
        Console.WriteLine("Developer:");
        Console.WriteLine("  dotnet <command>             Runs dotnet command");
        Console.WriteLine("  git <command>                Runs git command");
        Console.WriteLine("  pyc                          Opens Python");
        Console.WriteLine("  pwsh <command>               Runs PowerShell command");
        // installer commands
        Line();
        Console.WriteLine("Installers:");
        Console.WriteLine("  winget <package>             Installs package on Windows");
        Console.WriteLine("  linuget <package>            Installs package on Linux");
        Console.WriteLine("  macget <package>             Installs package on macOS");
        // admin commands
        Line();
        Console.WriteLine("Admin / Power:");
        Console.WriteLine("  adminc                       Reopens Lash as admin");
        Console.WriteLine("  restart                      Restarts computer");
        Console.WriteLine("  shutdown                     Shuts down computer");
        // fun commands
        Line();
        Console.WriteLine("Fun:");
        Console.WriteLine("  wheremostaccidentshappen     Joke crash command");
        Console.WriteLine("  abcdefghijklmnopqrstuvwxyz   ABC joke");
        Console.WriteLine("  qwertyuiop                   Keyboard mash joke");
        Console.WriteLine("  youareanidiot                Opposite day joke");
        Console.WriteLine("  youarethebest                Opens The Handies channel");
        // commands for looks
        Line();
        Console.WriteLine("Looks:");
        Console.WriteLine("  path h                        Hides current path");
        Console.WriteLine("  path s                        Shows current path");
        Console.WriteLine("  bgclr                         Colors the background");
        Console.WriteLine("  txtclr                        Colors the text");
        // binary commands
        Line();
        Console.WriteLine("Binary:");
        Console.WriteLine("  binaryt <text>                Converts text to binary");
        Console.WriteLine("  binaryf <binary>              Converts binary to text");
        Console.WriteLine("  binarytc <text>               Converts text to binary and copies to clipboard");
        Console.WriteLine("  binaryfc <binary>             Converts binary to text and copies to clipboard");
    }
    static bool IsWindows() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    static bool IsLinux() => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    static bool IsMacOS() => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
    static bool IsAdmin()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        return Environment.UserName == "root";
    }
    static void RunAsAdmin(string file, string arguments)
    {
        ProcessStartInfo psi;
        if (IsWindows())
        {
            psi = new ProcessStartInfo()
            {
                FileName = file,
                Arguments = arguments,
                Verb = "runas",
                UseShellExecute = true,
            };
        }
        else if (IsLinux() || IsMacOS())
        {
            psi = new ProcessStartInfo()
            {
                FileName = "sudo",
                Arguments = $"{file} {arguments}",
                UseShellExecute = true,
            };
        }
        else
        {
            Console.WriteLine("Unsupported operating system");
            return;
        }
        try
        {
            Process.Start(psi);
        }
        catch
        {
            Console.WriteLine("Failed to run as admin");
        }
    }
    static string DetectLinuxDistro()
    {
        try
        {
            if (!File.Exists("/etc/os-release"))
                return "unknown";

            var lines = File.ReadAllLines("/etc/os-release");
            foreach (var line in lines)
            {
                if (line.StartsWith("ID="))
                {
                    return line.Substring(3).Trim().Trim('"').ToLower();
                }
            }
        }
        catch { }

        return "unknown";
    }
    static string DetectPackageManager()
    {
        string id = DetectLinuxDistro();

        return id switch
        {
            // Debian Family
            "debian" or "ubuntu" or "linuxmint" or "pop" or "zorin" or "elementary" or "kali" or "mx" 
                => "apt",

            // Fedora / RHEL Family
            "fedora" or "rhel" or "centos" or "rocky" or "almalinux"
                => "dnf",

            // Arch Family
            "arch" or "manjaro" or "endeavouros" or "garuda"
                => "pacman",

            // openSUSE
            "opensuse" or "opensuse-tumbleweed" or "opensuse-leap"
                => "zypper",

            // Alpine
            "alpine" => "apk",

            // Void
            "void" => "xbps",

            // Gentoo
            "gentoo" => "emerge",

            // Solus
            "solus" => "eopkg",

            // NixOS
            "nixos" => "nix-env",

            _ => "unknown"
        };
    }
    static (string file, string insArgs) BuildInstallCommand(string pkg)
    {
        string pm = DetectPackageManager();

        return pm switch
        {
            "apt"    => ("bash", $"-c \"sudo apt install -y {pkg}\""),
            "dnf"    => ("bash", $"-c \"sudo dnf install -y {pkg}\""),
            "pacman" => ("bash", $"-c \"sudo pacman -S --noconfirm {pkg}\""),
            "zypper" => ("bash", $"-c \"sudo zypper install -y {pkg}\""),
            "apk"    => ("bash", $"-c \"sudo apk add {pkg}\""),
            "xbps"   => ("bash", $"-c \"sudo xbps-install -S {pkg}\""),
            "emerge" => ("bash", $"-c \"sudo emerge {pkg}\""),
            "eopkg"  => ("bash", $"-c \"sudo eopkg install {pkg}\""),
            "nix-env"=> ("bash", $"-c \"nix-env -i {pkg}\""),

            _ => ("unknown", "")
        };
    }
    static void Main()
    {
        string user = Environment.UserName;
        Environment.CurrentDirectory = @$"C:\Users\{user}";
        var rand = new Random();
        var backgroundDefaultColor = Console.BackgroundColor;
        var TextDefaultColor = Console.ForegroundColor;
        Console.Title = "Lash Terminal";
        Console.WriteLine("Lash 1.0.0");
        Console.WriteLine($"Welcome, {user}");
        Line();
        Thread.Sleep(3000);
        bool pathHide = false;
        while (true)
        {
            string path = Environment.CurrentDirectory;
            string cmdtext = $"{path} >/> ".Replace("\\", "/");
            if (pathHide)
            {
                cmdtext = ">/> ";
            }
            Console.Write(cmdtext);
            string? cmd = Console.ReadLine();
            // switch and cases
            switch (cmd)
            {
                case null: case "": break;
                case var s when s.StartsWith("cd "):
                {
                    string[] cdArgs = s.Split(' ',2);
                    if (cdArgs.Length <= 1)
                    {
                        Console.WriteLine("cd must have a path as an argument");
                        break;
                    }
                    string inputPath = cdArgs[1].Trim().Trim('"');
                    if (inputPath.ToLower() == ".dsk")
                    {
                        inputPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    }
                    else if (inputPath == "~")
                    {
                        inputPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                    }
                    else if (inputPath.StartsWith("~/"))
                    {
                        string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                        inputPath = Path.Combine(home, inputPath.Substring(2));
                    }
                    string fullPath = Path.GetFullPath(inputPath);

                    if (!Directory.Exists(fullPath))
                    {
                        Console.WriteLine("Directory does not exist");
                        break;
                    }
                    Directory.SetCurrentDirectory(fullPath);
                    break;
                }
                case var s when s.StartsWith("out "):
                    string output = s.Substring(4).Trim().Trim('"');
                    Console.WriteLine(output);
                    break;
                case "ln":
                    Line();
                    break;
                case "in":
                    Console.ReadLine();
                    break;
                case var s when s.StartsWith("read "):
                    string read = s.Substring(5);
                    string? line = Console.ReadLine();
                    if (line == null)
                    {
                        Console.WriteLine("UserInputIsNull");
                        break;
                    }
                    else if (line == read)
                    {
                        Console.WriteLine("UserInputIsEqual");
                    }
                    else
                    {
                        Console.WriteLine("UserInputIsOther");
                    }
                    break;
                case "clr":
                    Console.Clear();
                    break;
                case "exit":
                    Environment.Exit(0);
                    break;
                case "help":
                    Help();
                    break;
                case "machinename":
                    Console.WriteLine(Environment.MachineName);
                    break;
                case "username":
                    Console.WriteLine(user);
                    break;
                case "userPath":
                    Console.WriteLine(path);
                    break;
                case "localtime":
                    Console.WriteLine($@"Date:
    Year: {DateTime.Now.Year}
    Month: {DateTime.Now.Month}
    Day: {DateTime.Now.Day}
    Hour: {DateTime.Now.Hour}
    Minute: {DateTime.Now.Minute}
    Second: {DateTime.Now.Second}
    Millisecond: {DateTime.Now.Millisecond}");
                    break;
                case var s when s.StartsWith("explorer "):
                    {
                        string target = s.Substring(9).Trim('"');
                        target = target switch
                        {
                            "~" => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                            "~~" => Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                            _ when target.StartsWith("~/") => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), target.Substring(2)),
                            _ when IsWindows() && target == "!." => @"C:\Windows\System32",
                            _ => target
                        };
                        if (!Directory.Exists(target))
                        {
                            Console.WriteLine($"Directory \"{target}\" does not exist");
                            break;
                        }
                        string? cmdl =
                            IsWindows() ? "explorer.exe":
                            IsLinux() ? "xdg-open":
                            IsMacOS() ? "open":
                            null;
                        if (cmdl == null)
                        {
                            Console.WriteLine("Unsupported operating system");
                            break;
                        }
                        Process.Start(cmdl, target);
                        break;
                    }
                case "pyc":
                    Process.Start(new ProcessStartInfo()
                    {
                        FileName = "python",
                        CreateNoWindow = false,
                        UseShellExecute = true,
                    });
                    break;
                case "restart":
                    if (!IsAdmin())
                    {
                        Console.WriteLine("You need to run this command as administrator");
                        break;
                    }
                    if (IsWindows())
                        Process.Start("shutdown", "/r /t 0");
                    else if (IsLinux() || IsMacOS())
                        Process.Start("shutdown", "-r now");
                    else
                        Console.WriteLine("Unsupported operating system");
                    break;
                case "shutdown":
                    if (!IsAdmin())
                    {
                        Console.WriteLine("You need to run this command as administrator");
                        break;
                    }
                    if (IsWindows())
                        Process.Start("shutdown", "/s /t 0");
                    else if (IsLinux() || IsMacOS())
                        Process.Start("shutdown", "-h now");
                    else
                        Console.WriteLine("Unsupported operating system");
                    break;
                case var s when s.StartsWith("git "):
                {
                    string gitArgs = s.Substring(4).Trim();
                    var p = new Process();
                    p.StartInfo.FileName = "git";
                    p.StartInfo.Arguments = gitArgs;
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.RedirectStandardError = true;
                    p.Start();
                    string stdout = p.StandardOutput.ReadToEnd();
                    string stderr = p.StandardError.ReadToEnd();
                    p.WaitForExit();
                    Console.Write(stdout);
                    Console.Write(stderr);
                    break;
                }
                case var s when s.StartsWith("dotnet "):
                {
                    string dotnetArgs = s.Substring(7).Trim();
                    var p = new Process();
                    p.StartInfo.FileName = "dotnet";
                    p.StartInfo.Arguments = dotnetArgs;
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.RedirectStandardError = true;
                    p.Start();
                    string stdout = p.StandardOutput.ReadToEnd();
                    string stderr = p.StandardError.ReadToEnd();
                    p.WaitForExit();
                    Console.Write(stdout);
                    Console.Write(stderr);
                    break;
                }
                case "pwcrash":
                    if (!IsAdmin())
                    {
                        Console.WriteLine("You need to run this command as administrator");
                        break;
                    }
                    Console.Write("Are you sure you want to crash the system? (y/n): ");
                    string? input = Console.ReadLine();
                    if (input?.ToLower() == "y")
                    {
                        var psi = new ProcessStartInfo()
                        {
                            FileName = "pwsh",
                            Arguments = "-Command \"for ($i = 0; $i -lt 10; $i++) { wt }\"",
                            UseShellExecute = true,
                        };
                        Process.Start(psi);
                        break;
                    }
                    else if (input?.ToLower() == "n")
                    {
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Please enter y or n");
                        break;
                    }
                case "wheremostaccidentshappen":
                    Console.WriteLine("terminal, and then the terminal crashed");
                    Environment.Exit(1337);
                    break;
                case var s when s.StartsWith("begin "):
                    {
                        Console.WriteLine(@"DISCLAIMER:
This app does what you tell it to do.
If you use it to run malware, delete files, crash your PC, or break your system,
that is YOUR responsibility, not mine.
Do not run random files unless you know what they are.");
                        Console.Write("Are you sure you want to run this file? (y/n): ");
                        string? inp = Console.ReadLine();
                        if (inp == null)
                        {
                            Console.WriteLine("YOU PICKED NONE");
                            break;
                        }
                        else if (!inp.Equals("y", StringComparison.OrdinalIgnoreCase))
                        {
                            break;
                        }
                        string file = s.Substring(6).Trim().Trim('"');
                        if (!File.Exists(file))
                        {
                            Console.WriteLine($"File \"{file}\" does not exist");
                            break;
                        }
                        try
                        {
                            Process.Start(new ProcessStartInfo()
                            {
                                FileName = file,
                                UseShellExecute = true,
                            });
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error: " + ex.Message);
                        }
                        break;
                    }
                case var s when s.StartsWith("pwsh "):
                    {
                        Process.Start("powershell.exe", s.Substring(5));
                        break;
                    }
                case "adminc":
                    string exeName = Path.GetFileName(Environment.ProcessPath ?? "");
                    Process.Start(new ProcessStartInfo()
                    {
                        FileName = "powershell.exe",
                        Arguments = $"-Command \"Start-Process {exeName} -Verb RunAs\"",
                        UseShellExecute = true,
                    });
                    break;
                case var s when s.StartsWith("macget "):
                    {
                        if (!IsMacOS())
                        {
                            Console.WriteLine("This command is only supported on macOS");
                            break;
                        }
                        if (!IsAdmin())
                        {
                            Console.WriteLine("You need to run this command as administrator");
                            break;
                        }
                        string macgetArgs = s.Substring(6).Trim().Trim('"');
                        Process.Start(new ProcessStartInfo()
                        {
                            FileName = "brew",
                            Arguments = $"install {macgetArgs}",
                        });
                        break;
                    }
                case var s when s.StartsWith("winget "):
                    {                
                        if (!IsWindows())
                        {
                            Console.WriteLine("This command is only supported on Windows");
                            break;
                        }
                        if (!IsAdmin())
                        {
                            Console.WriteLine("You need to run this command as administrator");
                            break;
                        }
                        string wingetArgs = s.Substring(6).Trim().Trim('"');
                        Process.Start(new ProcessStartInfo()
                        {
                            FileName = "winget",
                            Arguments = $"install {wingetArgs}",
                        });
                        break;
                    }
                case var s when s.StartsWith("linuget "):
                    {
                        if (!IsLinux())
                        {
                            Console.WriteLine("This command is only supported on Linux.");
                            break;
                        }
                        if (!IsAdmin())
                        {
                            Console.WriteLine("You need to run this command as administrator");
                            break;
                        }
                        string pkg = s.Substring(8).Trim().Trim('"');
                        var (file, linuArgs) = BuildInstallCommand(pkg);
                        if (file == "unknown")
                        {
                            Console.WriteLine("Could not detect your Linux package manager.");
                            break;
                        }
                        Process.Start(new ProcessStartInfo()
                        {
                            FileName = file,
                            Arguments = linuArgs,
                            UseShellExecute = false
                        });
                        Console.WriteLine($"Installing {pkg}...");
                        break;
                    }
                case "uptime":
                    var uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
                    Console.WriteLine("Uptime:");
                    Console.WriteLine($"  Years: {uptime.Days / 365}");
                    Console.WriteLine($"  Months: {uptime.Days / 30}");
                    Console.WriteLine($"  Weeks: {uptime.Days / 7}");
                    Console.WriteLine($"  Days: {uptime.Days}");
                    Console.WriteLine($"  Hours: {uptime.Hours}");
                    Console.WriteLine($"  Minutes: {uptime.Minutes}");
                    Console.WriteLine($"  Seconds: {uptime.Seconds}");
                    Console.WriteLine($"  Milliseconds: {uptime.Milliseconds}");
                    break;
                case "bootingtime":
                    var Now = DateTime.Now;
                    var Uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);;
                    var BootTime = Now - Uptime;
                    Console.WriteLine($"Boot Time: {BootTime}");
                    break;
                case "datetime":
                    var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss tt");
                    Console.WriteLine($"Current Clock And Date: {now}");
                    break;
                case "date":
                    var today = DateTime.Today.ToString("yyyy-MM-dd");
                    Console.WriteLine($"Date: {today}");
                    break;
                case "time":
                    var time = DateTime.Now.ToString("HH:mm:ss tt");
                    Console.WriteLine($"Time: {time}");
                    break;
                case "abcdefghijklmnopqrstuvwxyz":
                    Console.WriteLine("Now i know my ABC's, Next time won't you sing with me?");
                    break;
                case "qwertyuiop":
                    Console.WriteLine("qwertyuiopqwertyuiopqwertyuip MASHING qwertyuiopqwertyuiopqwertyuiop");
                    break;
                case "youareanidiot":
                    Console.WriteLine("But today is oppiside day! also why are you blinking black to white?");
                    break;
                case "youarethebest":
                    Console.WriteLine("ok let me show you my yt channel");
                    Thread.Sleep(2000);
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "powershell",
                        Arguments = "start https://www.youtube.com/@THEHANDIES",
                        UseShellExecute = true,
                        CreateNoWindow = true,
                    });
                    break;
                case "path h":
                    pathHide = true;
                    break;
                case "path s":
                    pathHide = false;
                    break;
                case var s when s.StartsWith("del "):
                {
                    string[] delArgs = s.Split(' ', 2);

                    if (delArgs.Length <= 1)
                    {
                        Console.WriteLine("del must have a file name");
                        break;
                    }
                    string filePath = delArgs[1];
                    string fullPath = Path.GetFullPath(filePath);
                    if (!File.Exists(fullPath))
                    {
                        Console.WriteLine("File does not exist");
                        break;
                    }
                    try
                    {
                        File.Delete(fullPath);
                        Console.WriteLine("Deleted: " + Path.GetFileName(fullPath));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error deleting file: " + ex.Message);
                    }
                    break;
                }
                case var s when s.StartsWith("txtclr "):
                    string[] txtclrArgs = s.Split(' ');
                    if (txtclrArgs.Length <= 1)
                    {
                        Console.WriteLine("txtclr must have a color as an argument");
                        break;
                    }
                    Console.ForegroundColor = txtclrArgs[1].ToLower() switch
                    {
                        "black" => ConsoleColor.Black,
                        "red" => ConsoleColor.Red,
                        "green" => ConsoleColor.Green,
                        "yellow" => ConsoleColor.Yellow,
                        "blue" => ConsoleColor.Blue,
                        "magenta" => ConsoleColor.Magenta,
                        "cyan" => ConsoleColor.Cyan,
                        "white" => ConsoleColor.White,
                        "gray" => ConsoleColor.Gray,
                        "darkgray" => ConsoleColor.DarkGray,
                        "darkred" => ConsoleColor.DarkRed,
                        "darkgreen" => ConsoleColor.DarkGreen,
                        "darkyellow" => ConsoleColor.DarkYellow,
                        "darkblue" => ConsoleColor.DarkBlue,
                        "darkmagenta" => ConsoleColor.DarkMagenta,
                        "darkcyan" => ConsoleColor.DarkCyan,
                        // numbers
                        "1" => ConsoleColor.Black,
                        "2" => ConsoleColor.Red,
                        "3" => ConsoleColor.Green,
                        "4" => ConsoleColor.Yellow,
                        "5" => ConsoleColor.Blue,
                        "6" => ConsoleColor.Magenta,
                        "7" => ConsoleColor.Cyan,
                        "8" => ConsoleColor.White,
                        "9" => ConsoleColor.Gray,
                        "10" => ConsoleColor.DarkGray,
                        "11" => ConsoleColor.DarkRed,
                        "12" => ConsoleColor.DarkGreen,
                        "13" => ConsoleColor.DarkYellow,
                        "14" => ConsoleColor.DarkBlue,
                        "15" => ConsoleColor.DarkMagenta,
                        "16" => ConsoleColor.DarkCyan,
                        _ => TextDefaultColor
                    };
                    break;
                case var s when s.StartsWith("bgclr "):
                    string[] bgclrArgs = s.Split(' ');
                    if (bgclrArgs.Length <= 1)
                    {
                        Console.WriteLine("bgclr must have a color as an argument");
                        break;
                    }
                    Console.BackgroundColor = bgclrArgs[1].ToLower() switch
                    {
                        "black" => ConsoleColor.Black,
                        "red" => ConsoleColor.Red,
                        "green" => ConsoleColor.Green,
                        "yellow" => ConsoleColor.Yellow,
                        "blue" => ConsoleColor.Blue,
                        "magenta" => ConsoleColor.Magenta,
                        "cyan" => ConsoleColor.Cyan,
                        "white" => ConsoleColor.White,
                        "gray" => ConsoleColor.Gray,
                        "darkgray" => ConsoleColor.DarkGray,
                        "darkred" => ConsoleColor.DarkRed,
                        "darkgreen" => ConsoleColor.DarkGreen,
                        "darkyellow" => ConsoleColor.DarkYellow,
                        "darkblue" => ConsoleColor.DarkBlue,
                        "darkmagenta" => ConsoleColor.DarkMagenta,
                        "darkcyan" => ConsoleColor.DarkCyan,
                        // numbers
                        "1" => ConsoleColor.Black,
                        "2" => ConsoleColor.Red,
                        "3" => ConsoleColor.Green,
                        "4" => ConsoleColor.Yellow,
                        "5" => ConsoleColor.Blue,
                        "6" => ConsoleColor.Magenta,
                        "7" => ConsoleColor.Cyan,
                        "8" => ConsoleColor.White,
                        "9" => ConsoleColor.Gray,
                        "10" => ConsoleColor.DarkGray,
                        "11" => ConsoleColor.DarkRed,
                        "12" => ConsoleColor.DarkGreen,
                        "13" => ConsoleColor.DarkYellow,
                        "14" => ConsoleColor.DarkBlue,
                        "15" => ConsoleColor.DarkMagenta,
                        "16" => ConsoleColor.DarkCyan,
                        _ => backgroundDefaultColor
                    };
                    break;
                #pragma warning disable CA1416
                case var s when s.StartsWith("encrypt "):
                {
                    if (!IsWindows())
                    {
                        Console.WriteLine("This command is only supported on Windows");
                        break;
                    }
                    string filepath = s.Substring(8).Trim();
                    if (!File.Exists(filepath))
                    {
                        Console.WriteLine("File does not exist");
                        break;
                    }
                    byte[] data = File.ReadAllBytes(filepath);
                    byte[] encrypted = ProtectedData.Protect(
                        data,
                        null,
                        DataProtectionScope.CurrentUser
                    );
                    string result = Convert.ToBase64String(encrypted);
                    File.WriteAllText(filepath, result);
                    Console.WriteLine("Encrypted file:");
                    Console.WriteLine(filepath);
                    break;
                }
                case var s when s.StartsWith("decrypt "):
                {
                    if (!IsWindows())
                    {
                        Console.WriteLine("This command is only supported on Windows");
                        break;
                    }
                    string decryptpath = s.Substring(8).Trim();
                    decryptpath = Path.GetFullPath(decryptpath);
                    if (!File.Exists(decryptpath))
                    {
                        Console.WriteLine("File does not exist");
                        break;
                    }
                    try
                    {
                        string base64 = File.ReadAllText(decryptpath).Trim();
                        byte[] encrypted = Convert.FromBase64String(base64);

                        byte[] decrypted = ProtectedData.Unprotect(
                            encrypted,
                            null,
                            DataProtectionScope.CurrentUser
                        );

                        File.WriteAllBytes(decryptpath, decrypted);

                        Console.WriteLine("Decrypted file:");
                        Console.WriteLine(decryptpath);
                    }
                    catch
                    {
                        Console.WriteLine("Invalid encrypted data");
                    }
                    break;
                }
                #pragma warning restore CA1416
                case "iamnotrickroll":
                    Process.Start(new ProcessStartInfo()
                    {
                        FileName = "powershell",
                        Arguments = "start https://www.youtube.com/watch?v=dQw4w9WgXcQ&list=RDdQw4w9WgXcQ&start_radio=1&pp=ygUIcmlja3JvbGygBwHSBwkJ3woBhyohjO8%3D",
                        UseShellExecute = true,
                    });
                    break;
                case var s when s.StartsWith("binaryt "):
                    {
                        string text = s.Substring(8);

                        string result = "";

                        foreach (char c in text)
                        {
                            string binary = Convert.ToString(c, 2).PadLeft(8, '0');
                            result += binary;
                        }
                        Console.WriteLine(result);
                        break;
                    }
                case var s when s.StartsWith("binaryf "):
                    {
                        string bin = s.Substring(8).Replace(" ", "");
                        if (bin.Length % 8 != 0)
                        {
                            Console.WriteLine("Invalid binary");
                            break;
                        }
                        if (bin.Length == 0)
                        {
                            Console.WriteLine("Empty binary");
                            break;
                        }
                        bool isBinary = bin.All(c => c == '0' || c == '1');
                        if (!isBinary)
                        {
                            Console.WriteLine("Is not binary");
                            break;
                        }
                        string result = "";
                        for (int i = 0; i < bin.Length; i += 8)
                        {
                            string chunk = bin.Substring(i, 8);
                            int value = Convert.ToInt32(chunk, 2);
                            result += (char)value;
                        }
                        Console.WriteLine(result);
                        break;
                    }
                case var s when s.StartsWith("binarytc "):
                    {
                        string text = s.Substring(9);

                        string result = "";

                        foreach (char c in text)
                        {
                            string binary = Convert.ToString(c, 2).PadLeft(8, '0');
                            result += binary;
                        }
                        Console.WriteLine(result);
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = "powershell",
                            Arguments = $"-NoProfile -Command \"Set-Clipboard -Value '{result}'\"",
                            UseShellExecute = false,
                            CreateNoWindow = true
                        });
                        break;
                    }
                case var s when s.StartsWith("binaryfc "):
                    {
                        string bin = s.Substring(9).Replace(" ", "");
                        if (bin.Length % 8 != 0)
                        {
                            Console.WriteLine("Invalid binary");
                            break;
                        }
                        if (bin.Length == 0)
                        {
                            Console.WriteLine("Empty binary");
                            break;
                        }
                        bool isBinary = bin.All(c => c == '0' || c == '1');
                        if (!isBinary)
                        {
                            Console.WriteLine("Is not binary");
                            break;
                        }
                        string result = "";
                        for (int i = 0; i < bin.Length; i += 8)
                        {
                            string chunk = bin.Substring(i, 8);
                            int value = Convert.ToInt32(chunk, 2);
                            result += (char)value;
                        }
                        Console.WriteLine(result);
                        string copy = result.Replace("'", "''");
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = "powershell",
                            Arguments = $"-NoProfile -Command \"Set-Clipboard -Value '{copy}'\"",
                            UseShellExecute = false,
                            CreateNoWindow = true
                        });
                        break;
                    }
                default:
                    string[] comd = cmd.Split(" ");
                    string cmdname = comd[0];
                    Console.WriteLine($"{cmdname} : The command {cmdname} is an invalid command, Please enter a valid command");
                    break;
            }
        }
    }
}