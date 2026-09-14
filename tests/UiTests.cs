using System;
using System.IO;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
class UiTests {
 [System.Runtime.InteropServices.DllImport("user32.dll",CharSet=System.Runtime.InteropServices.CharSet.Unicode)]static extern IntPtr FindWindow(string cls,string title);
 [System.Runtime.InteropServices.DllImport("user32.dll")]static extern bool PostMessage(IntPtr h,uint msg,IntPtr w,IntPtr l);
 static void Check(bool b,string s){if(!b)throw new Exception(s);Console.WriteLine("PASS "+s);}
 static Control Find(Control c,string n){if(c.Name==n)return c;foreach(Control child in c.Controls){var found=Find(child,n);if(found!=null)return found;}return null;}
 static void Click(Form f,string n){((Button)Find(f,n)).PerformClick();Application.DoEvents();}
 static void Shot(Form f,string p){using(var b=new Bitmap(f.Width,f.Height)){f.DrawToBitmap(b,new Rectangle(0,0,b.Width,b.Height));b.Save(p);}}
 [STAThread] static int Main(string[] args){try{
  string root=Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,".."));
  string exe=Path.Combine(root,args.Length>0?args[0]:"小猪举铁钟.exe");Check(File.Exists(exe),"desktop EXE exists");
  Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);
  var a=Assembly.LoadFrom(exe);
  string dir=Path.Combine(root,"tests","ui-data",Guid.NewGuid().ToString());
  object repo=Activator.CreateInstance(a.GetType("PigClock.Repository"),new object[]{dir});
  using(var f=(Form)Activator.CreateInstance(a.GetType("PigClock.MainForm"),new object[]{repo})){
   f.Show();Application.DoEvents();
   Check(Find(f,"Clock").Text=="25:00","initial time");
   Click(f,"StartPause");Check(Find(f,"StartPause").Text.Contains("暂停"),"start control");
   System.Threading.Thread.Sleep(1150);Application.DoEvents();Check(Find(f,"Clock").Text!="25:00","live timer ticks");
   Click(f,"StartPause");Check(Find(f,"StartPause").Text.Contains("继续"),"pause control");
   Click(f,"Reset");Check(Find(f,"Clock").Text=="25:00","reset control");
   Find(f,"TaskInput").Text="验收测试任务";Click(f,"AddTask");
   Check(Find(f,"TaskCheck")!=null,"task added");
   ((CheckBox)Find(f,"TaskCheck")).Checked=true;Application.DoEvents();
   dynamic loaded=repo.GetType().GetMethod("Load").Invoke(repo,null);
   Check(loaded.Tasks.Count==1 && loaded.Tasks[0].Done,"checkbox persisted");
   var date=(DateTimePicker)Find(f,"TaskDate");date.Value=date.Value.AddDays(-1);Application.DoEvents();Check(Find(f,"TaskCheck")==null,"date isolation");
   date.Value=DateTime.Today;Application.DoEvents();Check(Find(f,"TaskCheck")!=null,"history retained");
   Directory.CreateDirectory(Path.Combine(root,"tests","screenshots"));
   Shot(f,Path.Combine(root,"tests","screenshots","focus.png"));
   Click(f,"Mode1");Check(Find(f,"Clock").Text=="05:00","short rest");
   Shot(f,Path.Combine(root,"tests","screenshots","rest.png"));
   Click(f,"Mode2");Check(Find(f,"Clock").Text=="15:00","long rest");
   Click(f,"DeleteTask");Check(Find(f,"TaskCheck")==null,"delete control");
   bool settingsSeen=false;
   using(var helper=new Timer()){helper.Interval=100;helper.Tick+=delegate{
    Form dialog=Application.OpenForms.CastForms("SettingsForm");
    if(dialog==null)return;
    settingsSeen=true;((NumericUpDown)Find(dialog,"Minutes0")).Value=30;
    ((NumericUpDown)Find(dialog,"Minutes1")).Value=7;
    ((NumericUpDown)Find(dialog,"Minutes2")).Value=20;
    ((CheckBox)Find(dialog,"Sound")).Checked=false;
    Click(dialog,"SaveSettings");
   };helper.Start();Click(f,"Settings");helper.Stop();}
   Check(settingsSeen,"settings dialog");Click(f,"Mode0");Check(Find(f,"Clock").Text=="30:00","custom duration applied");
   loaded=repo.GetType().GetMethod("Load").Invoke(repo,null);Check(loaded.ShortMinutes==7 && loaded.LongMinutes==20 && !loaded.Sound,"settings persisted");
   var clockField=f.GetType().GetField("clock",BindingFlags.Instance|BindingFlags.NonPublic);
   dynamic countdown=clockField.GetValue(f);int alerts=0;
   IntPtr lastAlert=IntPtr.Zero;
   using(var closer=new Timer()){closer.Interval=50;closer.Tick+=delegate{IntPtr h=FindWindow("#32770","时间到");if(h!=IntPtr.Zero){if(h!=lastAlert){alerts++;lastAlert=h;}PostMessage(h,0x111,new IntPtr(1),IntPtr.Zero);}};closer.Start();
    countdown.Reset(1);Click(f,"StartPause");var deadline=DateTime.UtcNow.AddSeconds(4);
    while(DateTime.UtcNow<deadline){Application.DoEvents();System.Threading.Thread.Sleep(20);}
    closer.Stop();
   }
   Console.WriteLine("Alarm diagnostics: count="+alerts+", clock="+Find(f,"Clock").Text+", running="+countdown.Running);
   Check(alerts==1,"alarm shown once");Check(Find(f,"Clock").Text=="07:00" && Find(f,"StartPause").Text.Contains("开始"),"completion prepares rest without auto start");
   Check(Find(f,"Status").Text.Contains("专注完成"),"completion message retained");
   ((CheckBox)Find(f,"Pin")).Checked=true;Check(f.TopMost,"always on top");
   f.ClientSize=new Size(500,665);Application.DoEvents();Shot(f,Path.Combine(root,"tests","screenshots","compact.png"));
   using(var stream=File.Create(Path.Combine(root,"src","app.ico")))f.Icon.Save(stream);
   f.Close();
  }
  Console.WriteLine("All UI tests passed");return 0;
 }catch(Exception e){Console.Error.WriteLine(e);return 1;}}
}
static class FormLookup {public static Form CastForms(this FormCollection forms,string name){foreach(Form f in forms)if(f.Name==name)return f;return null;}}
