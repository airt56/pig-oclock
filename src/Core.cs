using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace PigClock {
 public class Countdown {
  double seconds;
  DateTime deadline;
  public bool Running { get; private set; }
  public void Reset(int duration) { seconds=duration; Running=false; }
  public void Start(DateTime now) { if(Running || seconds<=0) return; deadline=now.AddSeconds(seconds); Running=true; }
  public void Pause(DateTime now) { if(!Running) return; seconds=Math.Max(0,(deadline-now).TotalSeconds); Running=false; }
  public int Remaining(DateTime now) { return (int)Math.Ceiling(Math.Max(0,Running?(deadline-now).TotalSeconds:seconds)); }
  public bool Tick(DateTime now) { if(!Running || now<deadline) return false; seconds=0; Running=false; return true; }
 }
 public class TaskItem {
  public string Id=Guid.NewGuid().ToString();
  public string Date="";
  public string Text="";
  public bool Done;
 }
 public class AppState {
  public int FocusMinutes=25;
  public int ShortMinutes=5;
  public int LongMinutes=15;
  public bool TopMost;
  public bool Sound=true;
  public List<TaskItem> Tasks=new List<TaskItem>();
 }
 public class Repository {
  readonly string directory;
  readonly XmlSerializer serializer=new XmlSerializer(typeof(AppState));
  public bool Recovered {get;private set;}
  public Repository(string directory) {this.directory=directory;}
  AppState Read(string path) {
   using(var stream=File.OpenRead(path)) {
    var s=(AppState)serializer.Deserialize(stream);
    s.FocusMinutes=Math.Max(1,Math.Min(180,s.FocusMinutes));
    s.ShortMinutes=Math.Max(1,Math.Min(180,s.ShortMinutes));
    s.LongMinutes=Math.Max(1,Math.Min(180,s.LongMinutes));
    if(s.Tasks==null)s.Tasks=new List<TaskItem>();
    s.Tasks.RemoveAll(t=>t==null || string.IsNullOrWhiteSpace(t.Text));
    return s;
   }
  }
  public AppState Load() {
   Recovered=false;
   string path=Path.Combine(directory,"data.xml");
   if(!File.Exists(path))return new AppState();
   try{return Read(path);}catch(Exception e) {
    if(!(e is InvalidOperationException || e is IOException))throw;
    string backup=path+".bak";
    if(!File.Exists(backup)) throw new IOException("任务文件无法读取，请保留数据文件以便恢复。",e);
    var s=Read(backup); Recovered=true;return s;
   }
  }
  public void Save(AppState state) {
   Directory.CreateDirectory(directory);
   string path=Path.Combine(directory,"data.xml"), temp=path+".tmp";
   using(var stream=new FileStream(temp,FileMode.Create,FileAccess.Write,FileShare.None)) {serializer.Serialize(stream,state);stream.Flush(true);}
   if(File.Exists(path))File.Replace(temp,path,Recovered?null:path+".bak");
   else File.Move(temp,path);
   Recovered=false;
  }
 }
}
