using System;
using System.IO;
using System.Reflection;
class CoreTests {
 static void Check(bool value, string label) { if (!value) throw new Exception(label); Console.WriteLine("PASS " + label); }
 static int Main() {
  try {
   string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Core.dll");
   Check(File.Exists(path), "Core implementation exists");
   Assembly a = Assembly.LoadFrom(path);
   dynamic timer = Activator.CreateInstance(a.GetType("PigClock.Countdown"));
   DateTime now = new DateTime(2026,9,14,12,0,0,DateTimeKind.Utc);
   timer.Reset(60); timer.Start(now);
   Check(timer.Remaining(now.AddSeconds(17)) == 43, "elapsed deadline");
   timer.Pause(now.AddSeconds(17));
   Check(timer.Remaining(now.AddHours(1)) == 43, "paused duration preserved");
   timer.Start(now.AddHours(1));
   Check(!timer.Tick(now.AddHours(1).AddSeconds(42)), "not early");
   Check(timer.Tick(now.AddHours(1).AddSeconds(43)), "completion");
   Check(!timer.Tick(now.AddHours(2)), "completion fires once");
   timer.Reset(300); Check(timer.Remaining(now)==300 && !timer.Running, "reset");
   string dir=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"test-data",Guid.NewGuid().ToString());
   dynamic repo=Activator.CreateInstance(a.GetType("PigClock.Repository"),new object[]{dir});
   dynamic state=repo.Load();
   dynamic task=Activator.CreateInstance(a.GetType("PigClock.TaskItem"));
   task.Date="2026-09-14"; task.Text="中文任务"; task.Done=true; state.Tasks.Add(task);
   repo.Save(state); state.FocusMinutes=30; repo.Save(state);
   dynamic restored=repo.Load();
   Check(restored.Tasks.Count==1 && restored.Tasks[0].Text=="中文任务" && restored.Tasks[0].Done, "task persistence");
   Check(restored.FocusMinutes==30, "settings persistence");
   File.WriteAllText(Path.Combine(dir,"data.xml"),"broken");
   restored=repo.Load(); Check(restored.Tasks.Count==1 && repo.Recovered, "backup recovery");
   Console.WriteLine("All core tests passed"); return 0;
  } catch(Exception e) {Console.Error.WriteLine("FAIL " + e.ToString()); return 1;}
 }
}
