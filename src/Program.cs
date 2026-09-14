using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace PigClock {
 static class Program {
  [STAThread]static void Main(){
   bool created;
   using(var mutex=new Mutex(true,"Local\\PigLiftClock.Desktop",out created)){
    Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);
    if(!created){MessageBox.Show("小猪举铁钟已经运行，请从任务栏或右下角托盘打开。","小猪举铁钟");return;}
    try{string dir=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"PigLiftClock");Application.Run(new MainForm(new Repository(dir)));}
    catch(Exception e){MessageBox.Show("小猪举铁钟无法启动。\n"+e.Message+"\n\n数据位置：%LOCALAPPDATA%\\PigLiftClock","启动失败",MessageBoxButtons.OK,MessageBoxIcon.Error);}
   }
  }
 }
}
