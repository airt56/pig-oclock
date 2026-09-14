using System;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Windows.Forms;

namespace PigClock {
 public class MainForm:Form {
  readonly Repository repository;
  AppState state;
  readonly Countdown clock=new Countdown();
  readonly Timer ticker=new Timer();
  readonly PigCanvas pig=new PigCanvas();
  readonly Label time=new Label(),status=new Label(),durations=new Label(),progress=new Label(),heading=new Label();
  readonly SoftButton start=new SoftButton(),reset=new SoftButton(),settings=new SoftButton(),add=new SoftButton();
  readonly SoftButton[] modes={new SoftButton(),new SoftButton(),new SoftButton()};
  readonly CheckBox pin=new CheckBox();
  readonly Card card=new Card();
  readonly FlowLayoutPanel list=new FlowLayoutPanel();
  readonly TextBox input=new TextBox();
  readonly DateTimePicker date=new DateTimePicker();
  readonly Panel bar=new Panel(),barFill=new Panel();
  readonly ToolTip tips=new ToolTip();
  readonly NotifyIcon tray=new NotifyIcon();
  int mode,completed;
  DateTime today=DateTime.Today;
  bool loading,saveFailed;
  string notice="";
  Font regular=new Font("Microsoft YaHei UI",10), small=new Font("Microsoft YaHei UI",9);

  public MainForm(Repository repository) {
   this.repository=repository;
   state=repository.Load();
   Text="小猪举铁钟";Name="MainForm";BackColor=Theme.Cream;ForeColor=Theme.Ink;Font=regular;
   AutoScaleMode=AutoScaleMode.None;StartPosition=FormStartPosition.CenterScreen;
   ClientSize=new Size(560,800);MinimumSize=new Size(510,700);
   var area=Screen.PrimaryScreen.WorkingArea;if(Height>area.Height-20)Height=area.Height-20;
   Icon=MakeIcon();DoubleBuffered=true;
   string[] names={"专注","短休息","长休息"};
   for(int i=0;i<3;i++){int index=i;modes[i].Text=names[i];modes[i].Name="Mode"+i;modes[i].Click+=delegate{if(mode==index)return;if(clock.Running && MessageBox.Show(this,"切换模式将重置当前计时，是否继续？","切换模式",MessageBoxButtons.YesNo,MessageBoxIcon.Question)!=DialogResult.Yes)return;SwitchMode(index);};Controls.Add(modes[i]);}
   Controls.Add(pig);
   time.Name="Clock";time.Font=new Font("Segoe UI",49,FontStyle.Bold);time.TextAlign=ContentAlignment.MiddleCenter;Controls.Add(time);
   status.Name="Status";status.Font=new Font("Microsoft YaHei UI",11);status.TextAlign=ContentAlignment.MiddleCenter;Controls.Add(status);
   start.Name="StartPause";start.Selected=true;start.Fill=Theme.Pink;start.ForeColor=Color.White;start.Font=new Font("Microsoft YaHei UI",12,FontStyle.Bold);start.Click+=delegate{Toggle();};Controls.Add(start);
   reset.Name="Reset";reset.Text="↻  重置";reset.Fill=Theme.Cream;reset.Click+=delegate{notice="";clock.Reset(Duration()*60);RefreshClock();};Controls.Add(reset);
   durations.TextAlign=ContentAlignment.MiddleCenter;durations.ForeColor=Color.FromArgb(135,125,117);durations.Font=small;durations.Cursor=Cursors.Hand;durations.Click+=delegate{OpenSettings();};Controls.Add(durations);
   Controls.Add(card);heading.Text="今日任务";heading.Font=new Font("Microsoft YaHei UI",13,FontStyle.Bold);heading.BackColor=Color.FromArgb(255,253,250);card.Controls.Add(heading);
   date.Name="TaskDate";date.Format=DateTimePickerFormat.Custom;date.CustomFormat="M月d日 ddd";date.Font=small;date.ValueChanged+=delegate{RefreshTasks();};card.Controls.Add(date);
   progress.Font=small;progress.ForeColor=Color.FromArgb(130,120,112);progress.BackColor=heading.BackColor;card.Controls.Add(progress);
   bar.BackColor=Theme.Line;barFill.BackColor=Theme.Pink;bar.Controls.Add(barFill);card.Controls.Add(bar);
   list.FlowDirection=FlowDirection.TopDown;list.WrapContents=false;list.AutoScroll=true;list.BackColor=heading.BackColor;card.Controls.Add(list);
   input.Name="TaskInput";input.MaxLength=200;input.Font=regular;input.BorderStyle=BorderStyle.FixedSingle;input.KeyDown+=delegate(object sender,KeyEventArgs e){if(e.KeyCode==Keys.Enter){e.SuppressKeyPress=true;AddTask();}};tips.SetToolTip(input,"输入今日任务，按 Enter 添加（最多 200 字）");card.Controls.Add(input);
   add.Name="AddTask";add.Text="＋ 添加";add.Fill=Theme.Pink;add.ForeColor=Color.White;add.Selected=true;add.Click+=delegate{AddTask();};card.Controls.Add(add);
   pin.Name="Pin";pin.Text="窗口置顶";pin.AutoSize=true;pin.Font=small;pin.Checked=state.TopMost;TopMost=state.TopMost;pin.CheckedChanged+=delegate{TopMost=state.TopMost=pin.Checked;Save();};Controls.Add(pin);
   settings.Text="⚙ 设置";settings.Name="Settings";settings.Fill=Theme.Cream;settings.Click+=delegate{OpenSettings();};Controls.Add(settings);
   tray.Icon=Icon;tray.Text="小猪举铁钟";tray.Visible=true;tray.DoubleClick+=delegate{Restore();};tray.BalloonTipClicked+=delegate{Restore();};
   var menu=new ContextMenuStrip();menu.Items.Add("显示小猪举铁钟",null,delegate{Restore();});menu.Items.Add("退出",null,delegate{Close();});tray.ContextMenuStrip=menu;
   ticker.Interval=40;ticker.Tick+=delegate{Tick();};ticker.Start();
   Resize+=delegate{Arrange();};FormClosing+=HandleClosing;
   SwitchMode(0);Arrange();RefreshTasks();
   Shown+=delegate{if(repository.Recovered)MessageBox.Show(this,"主数据文件损坏，已从上一次备份恢复任务。","数据恢复",MessageBoxButtons.OK,MessageBoxIcon.Information);};
  }
  static Icon MakeIcon(){using(var bitmap=new Bitmap(32,32)){using(var g=Graphics.FromImage(bitmap)){g.SmoothingMode=System.Drawing.Drawing2D.SmoothingMode.AntiAlias;g.Clear(Theme.Cream);using(var b=new SolidBrush(Theme.Pink)){g.FillEllipse(b,4,2,9,14);g.FillEllipse(b,20,2,9,14);g.FillEllipse(b,2,8,28,23);}using(var b=new SolidBrush(Color.FromArgb(255,205,196)))g.FillEllipse(b,10,17,13,8);using(var b=new SolidBrush(Theme.Ink)){g.FillEllipse(b,9,13,3,3);g.FillEllipse(b,22,13,3,3);g.FillEllipse(b,13,20,2,3);g.FillEllipse(b,18,20,2,3);}}IntPtr handle=bitmap.GetHicon();try{return (Icon)Icon.FromHandle(handle).Clone();}finally{DestroyIcon(handle);}}}
  [System.Runtime.InteropServices.DllImport("user32.dll")]static extern bool DestroyIcon(IntPtr handle);
  [System.Runtime.InteropServices.DllImport("dwmapi.dll")]static extern int DwmSetWindowAttribute(IntPtr window,int attribute,ref int value,int size);
  protected override void OnHandleCreated(EventArgs e){base.OnHandleCreated(e);try{int square=1;DwmSetWindowAttribute(Handle,33,ref square,4);}catch(DllNotFoundException){}catch(EntryPointNotFoundException){}}
  int Duration(){return mode==0?state.FocusMinutes:mode==1?state.ShortMinutes:state.LongMinutes;}
  void SwitchMode(int value){notice="";mode=value;clock.Reset(Duration()*60);pig.Rest=mode!=0;for(int i=0;i<3;i++){modes[i].Selected=i==mode;modes[i].Fill=i==mode?(mode==0?Theme.Pink:Theme.Sage):Color.FromArgb(241,235,227);modes[i].ForeColor=i==mode?Color.White:Theme.Ink;modes[i].Invalidate();}RefreshClock();}
  void Toggle(){DateTime now=DateTime.UtcNow;if(clock.Tick(now)){Complete();return;}notice="";if(clock.Running)clock.Pause(now);else clock.Start(now);RefreshClock();}
  void Tick(){
   if(today!=DateTime.Today){bool showingToday=date.Value.Date==today;today=DateTime.Today;if(showingToday)date.Value=today;RefreshTasks();}
   if(clock.Tick(DateTime.UtcNow))Complete();
   if(clock.Running){pig.Phase+=.055;pig.Invalidate();}
   RefreshClock();
  }
  void Complete(){
   int finished=mode;
   if(mode==0)completed++;
   SwitchMode(finished==0?(completed%4==0?2:1):0);
   if(state.Sound)SystemSounds.Exclamation.Play();
   string message=finished==0?"这一轮完成啦！小猪准备盖被休息。":"休息结束啦！和小猪一起开始下一轮吧。";
   notice=finished==0?"专注完成！点击开始休息":"休息结束！点击开始专注";
   RefreshClock();
   MessageBox.Show(this,message+"\n\n下一阶段已准备好，点击“开始”后计时。","时间到",MessageBoxButtons.OK,MessageBoxIcon.None);
  }
  void RefreshClock(){int sec=clock.Remaining(DateTime.UtcNow);time.Text=string.Format("{0:00}:{1:00}",sec/60,sec%60);pig.Animate=clock.Running;
   start.Text=clock.Running?"Ⅱ  暂停":sec<Duration()*60?"▶  继续":"▶  开始";
   status.Text=clock.Running?(mode==0?"专注中 · 再坚持一下":"休息中 · 好好放松"):(sec<Duration()*60?"已暂停 · 随时继续":mode==0?"准备好了吗？和小猪一起加油":"让小猪陪你休息一会儿");
   if(notice.Length>0)status.Text=notice;
   durations.Text=string.Format("专注 {0} 分钟  /  短休息 {1} 分钟  /  长休息 {2} 分钟",state.FocusMinutes,state.ShortMinutes,state.LongMinutes);
   tray.Text="小猪举铁钟 · "+(mode==0?"专注 ":"休息 ")+time.Text;
   pig.Invalidate();
  }
  void Arrange(){
   int w=ClientSize.Width,h=ClientSize.Height,margin=26,inner=w-margin*2;
   int top=18,tab=40;for(int i=0;i<3;i++)modes[i].SetBounds(margin+i*inner/3,top,inner/3-2,tab);
   int pigHeight=Math.Max(145,Math.Min(205,h-580));pig.SetBounds(margin,top+tab+9,inner,pigHeight);
   int y=pig.Bottom;time.SetBounds(margin,y-4,inner,86);status.SetBounds(margin,y+79,inner,30);
   y+=120;start.SetBounds(margin+34,y,(inner-80)/2,48);reset.SetBounds(start.Right+12,y,(inner-80)/2,48);
   durations.SetBounds(margin,y+51,inner,30);
   int cardY=y+88,cardH=h-cardY-54;card.SetBounds(margin,cardY,inner,cardH);
   heading.SetBounds(18,15,130,29);date.SetBounds(inner-160,17,140,25);progress.SetBounds(18,49,126,24);
   bar.SetBounds(148,57,Math.Max(20,inner-168),6);barFill.Height=6;
   list.SetBounds(15,81,inner-30,Math.Max(55,cardH-137));input.SetBounds(19,cardH-43,inner-120,28);add.SetBounds(inner-91,cardH-47,73,35);
   pin.Location=new Point(margin+4,h-36);settings.SetBounds(w-margin-95,h-43,95,34);
   foreach(Control c in list.Controls)c.Width=Math.Max(100,list.ClientSize.Width-22);
   UpdateProgress();
  }
  void AddTask(){string text=input.Text.Trim();if(text.Length==0){input.Focus();return;}state.Tasks.Add(new TaskItem{Date=date.Value.ToString("yyyy-MM-dd"),Text=text});Save();input.Clear();RefreshTasks();input.Focus();}
  void RefreshTasks(){
   if(loading)return;loading=true;list.SuspendLayout();
   while(list.Controls.Count>0){Control old=list.Controls[0];list.Controls.RemoveAt(0);old.Dispose();}
   string key=date.Value.ToString("yyyy-MM-dd");heading.Text=date.Value.Date==DateTime.Today?"今日任务":"每日任务";
   foreach(TaskItem task in state.Tasks.Where(t=>t.Date==key)){
    var row=new Panel{Width=Math.Max(100,list.ClientSize.Width-22),Height=35,Margin=new Padding(0,0,0,3)};
    var check=new CheckBox{Name="TaskCheck",Text=task.Text,Checked=task.Done,AutoEllipsis=true,Cursor=Cursors.Hand,Location=new Point(4,2),Height=30,Width=row.Width-42,Anchor=AnchorStyles.Left|AnchorStyles.Top|AnchorStyles.Right,ForeColor=task.Done?Color.FromArgb(155,149,143):Theme.Ink};
    check.Font=new Font(regular,task.Done?FontStyle.Strikeout:FontStyle.Regular);tips.SetToolTip(check,task.Text);
    check.CheckedChanged+=delegate{task.Done=check.Checked;check.ForeColor=task.Done?Color.FromArgb(155,149,143):Theme.Ink;Font previous=check.Font;check.Font=new Font(regular,task.Done?FontStyle.Strikeout:FontStyle.Regular);previous.Dispose();Save();UpdateProgress();};
    var del=new Button{Name="DeleteTask",Text="×",FlatStyle=FlatStyle.Flat,Size=new Size(30,29),Location=new Point(row.Width-32,2),Anchor=AnchorStyles.Right|AnchorStyles.Top,Cursor=Cursors.Hand,ForeColor=Color.FromArgb(167,137,126)};del.FlatAppearance.BorderSize=0;tips.SetToolTip(del,"删除任务");del.Click+=delegate{state.Tasks.Remove(task);Save();RefreshTasks();};
    row.Controls.Add(check);row.Controls.Add(del);list.Controls.Add(row);
   }
   if(list.Controls.Count==0)list.Controls.Add(new Label{Text="今天也从一个小目标开始吧",ForeColor=Color.FromArgb(162,150,138),Width=list.Width-25,Height=43,TextAlign=ContentAlignment.MiddleCenter,Font=small});
   list.ResumeLayout();loading=false;UpdateProgress();
  }
  void UpdateProgress(){if(state==null)return;string key=date.Value.ToString("yyyy-MM-dd");var tasks=state.Tasks.Where(t=>t.Date==key).ToList();int done=tasks.Count(t=>t.Done);progress.Text=done+" / "+tasks.Count+" 已完成";barFill.Width=tasks.Count==0?0:bar.Width*done/tasks.Count;}
  bool Save(){try{repository.Save(state);saveFailed=false;return true;}catch(Exception e){if(!saveFailed)MessageBox.Show(this,"保存失败，当前任务仍留在窗口中。请检查磁盘空间或目录权限。\n"+e.Message,"无法保存",MessageBoxButtons.OK,MessageBoxIcon.Error);saveFailed=true;return false;}}
  void OpenSettings(){using(var dialog=new SettingsForm(state)){if(dialog.ShowDialog(this)==DialogResult.OK){state.FocusMinutes=dialog.FocusMinutes;state.ShortMinutes=dialog.ShortMinutes;state.LongMinutes=dialog.LongMinutes;state.Sound=dialog.Sound;Save();notice="";clock.Reset(Duration()*60);RefreshClock();}}}
  void Restore(){Show();WindowState=FormWindowState.Normal;Activate();}
  void HandleClosing(object sender,FormClosingEventArgs e){if(!Save() && MessageBox.Show(this,"任务尚未保存，仍要退出吗？","退出",MessageBoxButtons.YesNo,MessageBoxIcon.Warning)!=DialogResult.Yes){e.Cancel=true;return;}ticker.Stop();tray.Visible=false;}
  protected override void Dispose(bool disposing){if(disposing){ticker.Dispose();tray.Dispose();tips.Dispose();}base.Dispose(disposing);}
 }
 public class SettingsForm:Form {
  readonly NumericUpDown focus=new NumericUpDown(),shortRest=new NumericUpDown(),longRest=new NumericUpDown();
  readonly CheckBox sound=new CheckBox();
  public int FocusMinutes{get{return (int)focus.Value;}}public int ShortMinutes{get{return (int)shortRest.Value;}}public int LongMinutes{get{return (int)longRest.Value;}}public bool Sound{get{return sound.Checked;}}
  public SettingsForm(AppState state){Text="计时设置";Name="SettingsForm";ClientSize=new Size(365,322);FormBorderStyle=FormBorderStyle.FixedDialog;MaximizeBox=false;MinimizeBox=false;StartPosition=FormStartPosition.CenterParent;BackColor=Theme.Cream;ForeColor=Theme.Ink;Font=new Font("Microsoft YaHei UI",10);
   string[] labels={"专注时长（分钟）","短休息（分钟）","长休息（分钟）"};NumericUpDown[] nums={focus,shortRest,longRest};int[] values={state.FocusMinutes,state.ShortMinutes,state.LongMinutes};
   for(int i=0;i<3;i++){Controls.Add(new Label{Text=labels[i],Location=new Point(26,28+i*46),Size=new Size(175,26)});nums[i].Name="Minutes"+i;nums[i].Minimum=1;nums[i].Maximum=180;nums[i].Value=values[i];nums[i].SetBounds(231,25+i*46,103,30);Controls.Add(nums[i]);}
   sound.Text="到点播放提示音";sound.Name="Sound";sound.Checked=state.Sound;sound.SetBounds(26,169,290,26);Controls.Add(sound);
   Controls.Add(new Label{Text="保存后重置当前计时。每完成 4 次专注，\n下一阶段为长休息；到点后手动开始。",Font=new Font("Microsoft YaHei UI",9),ForeColor=Color.Gray,Location=new Point(26,205),Size=new Size(315,43)});
   var save=new SoftButton{Name="SaveSettings",Text="保存设置",Fill=Theme.Pink,ForeColor=Color.White,Selected=true,DialogResult=DialogResult.OK};save.SetBounds(183,264,151,39);Controls.Add(save);AcceptButton=save;
   var cancel=new SoftButton{Text="取消",Fill=Theme.Cream,DialogResult=DialogResult.Cancel};cancel.SetBounds(26,264,141,39);Controls.Add(cancel);CancelButton=cancel;
  }
 }
}
