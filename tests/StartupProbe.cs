using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
class StartupProbe {
 delegate bool Callback(IntPtr window,IntPtr p);
 [DllImport("user32.dll")]static extern bool EnumWindows(Callback c,IntPtr p);
 [DllImport("user32.dll")]static extern uint GetWindowThreadProcessId(IntPtr h,out uint id);
 [DllImport("user32.dll",CharSet=CharSet.Unicode)]static extern int GetWindowText(IntPtr h,StringBuilder s,int n);
 [DllImport("user32.dll")]static extern bool PostMessage(IntPtr h,uint m,IntPtr w,IntPtr l);
 static int Main(string[] args){int id=int.Parse(args[0]);IntPtr app=IntPtr.Zero;EnumWindows(delegate(IntPtr h,IntPtr p){uint owner;GetWindowThreadProcessId(h,out owner);if(owner==id){var text=new StringBuilder(256);GetWindowText(h,text,256);Console.WriteLine("Window: "+text);if(text.ToString()=="小猪举铁钟")app=h;}return true;},IntPtr.Zero);
  if(app==IntPtr.Zero){Console.Error.WriteLine("Main app window not found");return 1;}var process=Process.GetProcessById(id);PostMessage(app,0x10,IntPtr.Zero,IntPtr.Zero);if(!process.WaitForExit(5000)){Console.Error.WriteLine("Normal close timed out");return 1;}Console.WriteLine("Final EXE startup and normal exit passed: "+process.ExitCode);return process.ExitCode;
 }
}
