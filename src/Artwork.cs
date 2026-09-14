using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PigClock {
 public static class Theme {
  public static readonly Color Cream=Color.FromArgb(252,248,242), Ink=Color.FromArgb(89,59,48), Pink=Color.FromArgb(228,143,155), Sage=Color.FromArgb(155,176,138), Line=Color.FromArgb(231,220,211);
  public static GraphicsPath Round(RectangleF r,float radius) {
   var p=new GraphicsPath(); float d=radius*2;
   p.AddArc(r.X,r.Y,d,d,180,90);p.AddArc(r.Right-d,r.Y,d,d,270,90);p.AddArc(r.Right-d,r.Bottom-d,d,d,0,90);p.AddArc(r.X,r.Bottom-d,d,d,90,90);p.CloseFigure();return p;
  }
  public static void FillRound(Graphics g,Color c,RectangleF r,float radius) {using(var p=Round(r,radius))using(var b=new SolidBrush(c))g.FillPath(b,p);}
 }
 public class SoftButton:Button {
  public Color Fill=Color.White;
  public bool Selected;
  bool hover;
  public SoftButton(){FlatStyle=FlatStyle.Flat;FlatAppearance.BorderSize=0;Cursor=Cursors.Hand;SetStyle(ControlStyles.UserPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.AllPaintingInWmPaint,true);Font=new Font("Microsoft YaHei UI",10);ForeColor=Theme.Ink;}
  protected override void OnMouseEnter(EventArgs e){hover=true;Invalidate();base.OnMouseEnter(e);}
  protected override void OnMouseLeave(EventArgs e){hover=false;Invalidate();base.OnMouseLeave(e);}
  protected override void OnPaint(PaintEventArgs e){
   e.Graphics.Clear(BackColor);
   e.Graphics.SmoothingMode=SmoothingMode.None;
   Color c=hover?ControlPaint.Light(Fill,.15f):Fill;
   using(var brush=new SolidBrush(c))e.Graphics.FillRectangle(brush,ClientRectangle);
   if(!Selected)using(var pen=new Pen(Theme.Line))e.Graphics.DrawRectangle(pen,0,0,Width-1,Height-1);
   TextRenderer.DrawText(e.Graphics,Text,Font,ClientRectangle,Enabled?ForeColor:Color.Gray,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter);
   if(Focused && ShowFocusCues)using(var pen=new Pen(Selected?Color.White:Theme.Sage,2))e.Graphics.DrawRectangle(pen,4,4,Width-9,Height-9);
  }
 }
 public class Card:Panel {
  public Card(){DoubleBuffered=true;BackColor=Theme.Cream;}
  protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);e.Graphics.Clear(Color.FromArgb(255,253,250));e.Graphics.SmoothingMode=SmoothingMode.None;using(var pen=new Pen(Theme.Line))e.Graphics.DrawRectangle(pen,0,0,Width-1,Height-1);}
 }
 public class PigCanvas:Control {
  public bool Rest;
  public bool Animate;
  public double Phase;
  static readonly Color Outline=Color.FromArgb(158,99,88), Skin=Color.FromArgb(250,187,178), Ear=Color.FromArgb(232,141,141);
  public PigCanvas(){DoubleBuffered=true;BackColor=Theme.Cream;}
  void Oval(Graphics g,Color fill,float x,float y,float w,float h,bool stroke){using(var b=new SolidBrush(fill))g.FillEllipse(b,x,y,w,h);if(stroke)using(var p=new Pen(Outline,2.7f))g.DrawEllipse(p,x,y,w,h);}
  void Line(Graphics g,Color c,float width,params PointF[] points){using(var p=new Pen(c,width)){p.StartCap=LineCap.Round;p.EndCap=LineCap.Round;g.DrawLines(p,points);}}
  void Shape(Graphics g,Color fill,PointF[] pts){using(var p=new GraphicsPath()){p.AddClosedCurve(pts,.45f);using(var b=new SolidBrush(fill))g.FillPath(b,p);using(var pen=new Pen(Outline,2.7f))g.DrawPath(pen,p);}}
  protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);var g=e.Graphics;g.SmoothingMode=SmoothingMode.AntiAlias;float s=Math.Min(Width/340f,Height/225f);g.TranslateTransform((Width-340*s)/2,(Height-225*s)/2);g.ScaleTransform(s,s);float wave=Animate?(float)Math.Sin(Phase):0;
   if(Rest) DrawRest(g,wave);else DrawLift(g,wave);
  }
  void Face(Graphics g,float x,float y,bool sleep){
   Oval(g,Color.FromArgb(243,149,147),x-46,y+8,25,17,false);Oval(g,Color.FromArgb(243,149,147),x+24,y+8,25,17,false);
   if(sleep){using(var p=new Pen(Outline,3)){g.DrawArc(p,x-31,y-7,17,12,0,180);g.DrawArc(p,x+16,y-7,17,12,0,180);}}
   else {Oval(g,Theme.Ink,x-26,y-6,7,9,false);Oval(g,Theme.Ink,x+22,y-6,7,9,false);}
   Oval(g,Color.FromArgb(235,146,141),x-16,y+8,33,23,false);Oval(g,Outline,x-8,y+15,4,8,false);Oval(g,Outline,x+5,y+15,4,8,false);
  }
  void DrawLift(Graphics g,float wave){
   Oval(g,Color.FromArgb(228,225,202),95,205,154,12,false);
   float body=wave*1.8f;g.TranslateTransform(0,body);
   Shape(g,Skin,new[]{new PointF(114,56),new PointF(107,18),new PointF(132,16),new PointF(149,43)});
   Shape(g,Skin,new[]{new PointF(195,44),new PointF(219,16),new PointF(237,19),new PointF(229,62)});
   Oval(g,Ear,116,24,16,25,false);Oval(g,Ear,212,23,15,26,false);
   Shape(g,Skin,new[]{new PointF(108,119),new PointF(107,75),new PointF(128,43),new PointF(177,36),new PointF(221,52),new PointF(239,93),new PointF(231,141),new PointF(226,193),new PointF(208,207),new PointF(193,194),new PointF(146,194),new PointF(132,207),new PointF(114,196)});
   using(var p=new Pen(Color.FromArgb(255,245,227),16))g.DrawArc(p,112,40,118,57,195,152);
   using(var p=new Pen(Outline,2))g.DrawArc(p,110,31,122,58,197,149);
   Face(g,172,87,false);
   float lift=wave*15;
   Line(g,Outline,22,new PointF(119,130),new PointF(100,118-lift));Line(g,Skin,17,new PointF(119,130),new PointF(100,118-lift));
   Line(g,Outline,22,new PointF(224,130),new PointF(246,118-lift));Line(g,Skin,17,new PointF(224,130),new PointF(246,118-lift));
   Dumbbell(g,95,113-lift,-12);Dumbbell(g,248,113-lift,12);
   Oval(g,Skin,91,106-lift,17,20,true);Oval(g,Skin,236,106-lift,17,20,true);
   using(var p=new Pen(Outline,2)){g.DrawArc(p,226,163,17,15,270,270);}
   Line(g,Outline,2,new PointF(122,198),new PointF(131,199));Line(g,Outline,2,new PointF(205,198),new PointF(215,198));
   Line(g,Theme.Pink,2,new PointF(67,75-lift),new PointF(61,67-lift),new PointF(63,60-lift));
   Line(g,Theme.Pink,2,new PointF(273,76-lift),new PointF(280,68-lift),new PointF(277,60-lift));
  }
  void Dumbbell(Graphics g,float x,float y,float angle){var state=g.Save();g.TranslateTransform(x,y);g.RotateTransform(angle);Theme.FillRound(g,Color.FromArgb(94,89,85),new RectangleF(-25,-5,50,10),4);foreach(int side in new[]{-1,1}){Theme.FillRound(g,Color.FromArgb(69,66,64),new RectangleF(side<0?-29:17,-20,14,40),5);Theme.FillRound(g,Color.FromArgb(120,114,106),new RectangleF(side<0?-25:19,-16,7,32),3);}g.Restore(state);}
  void DrawRest(Graphics g,float wave){
   Oval(g,Color.FromArgb(228,225,202),47,193,251,15,false);
   Shape(g,Color.FromArgb(245,239,215),new[]{new PointF(49,101),new PointF(94,64),new PointF(202,88),new PointF(217,174),new PointF(93,192)});
   g.TranslateTransform(0,wave*1.6f);
   Shape(g,Skin,new[]{new PointF(95,114),new PointF(105,68),new PointF(137,60),new PointF(179,48),new PointF(206,71),new PointF(222,113),new PointF(203,147),new PointF(142,161),new PointF(103,142)});
   Shape(g,Skin,new[]{new PointF(104,91),new PointF(96,69),new PointF(109,58),new PointF(123,65)});
   Shape(g,Skin,new[]{new PointF(180,59),new PointF(184,38),new PointF(199,35),new PointF(208,65)});
   var fs=g.Save();g.TranslateTransform(157,101);g.RotateTransform(-16);Face(g,0,0,true);g.Restore(fs);
   Shape(g,Color.FromArgb(174,193,151),new[]{new PointF(127,165),new PointF(173,130),new PointF(225,118),new PointF(269,145),new PointF(295,190),new PointF(271,204),new PointF(169,206),new PointF(103,189)});
   Shape(g,Color.FromArgb(243,239,218),new[]{new PointF(110,177),new PointF(151,144),new PointF(199,125),new PointF(231,125),new PointF(206,134),new PointF(161,156),new PointF(128,193)});
   Oval(g,Skin,102,143,25,21,true);
   foreach(var p in new[]{new PointF(170,181),new PointF(210,151),new PointF(245,178),new PointF(271,196)}) {for(int k=0;k<5;k++){double a=k*Math.PI*2/5;Oval(g,Color.FromArgb(246,241,218),p.X+(float)Math.Cos(a)*5-3,p.Y+(float)Math.Sin(a)*5-3,6,6,false);}Oval(g,Color.FromArgb(230,212,153),p.X-2,p.Y-2,4,4,false);}
   using(var p=new Pen(Color.FromArgb(143,164,126),2)){g.DrawArc(p,182,163,23,43,280,130);g.DrawArc(p,251,172,16,33,190,130);}
   using(var f=new Font("Segoe UI",17,FontStyle.Bold))using(var b=new SolidBrush(Theme.Sage)){g.DrawString("z",f,b,237,65-wave*2);g.DrawString("z",f,b,256,40-wave*3);g.DrawString("Z",f,b,275,13-wave*4);}
  }
 }
}
