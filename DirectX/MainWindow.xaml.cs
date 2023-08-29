using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace ant
{
    
    public partial class MainWindow : Window
    {
        private DispatcherTimer _timer;
        private int Width;
        private int Height;
        private long lastImgT;
        private ImgAnt img;
        private Agent[] agents;

        private float angle = 0f;
        public MainWindow()
        {
            // InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Width = 320;//640;//1280;
            Height = 180;//360/2;//720;

            var rand = new Random();

            img = new ImgAnt(Width,Height);
            agents = new Agent[10000];
            for (int i=0;i<agents.Length;i++){
                int x = rand.Next(0,301)-150;
                int y = rand.Next(0,150)-75;
                int r = rand.Next(0,360);
                agents[i] = new Agent(new Size(Width,Height), new Vector2(Width/2+x,Height/2+y),r,1.0f,0.75f,2f,1,0.5f);
            }


            
            canvas.Children.Add(img.GetImg((int)this.RenderSize.Width,(int)this.RenderSize.Width));

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(0); // Mise à jour toutes les 100 millisecondes
            _timer.Tick += Timer_Tick;
            _timer.Start();
            lastImgT = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            Parallel.For(0, agents.Length, i =>
            {
                agents[i].NewAngleSpeed(angle);
                agents[i].Update(img.byteMatrix);
            });
            angle= (angle<7f)? (angle<2f)? angle+0.001f:angle+0.01f : 0;
            // for (int i=0;i<agents.Length;i++){
            //     agents[i].Update(img.byteMatrix);
            //     // Console.WriteLine("Agent [{0},{1}]",agents[i].position.X,agents[i].position.Y);
            // }
            // img.DrawAgents(agents);
            img.Update(agents);
            
            canvas.Children.Clear(); // Supprimer l'ancienne image
            // canvas.Children.Add(image); // Ajouter la nouvelle image
            canvas.Children.Add(img.GetImg(Width*2,Height*2));
            long ImgT = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            Title = "Ant "+(1000/(ImgT - lastImgT)).ToString() + " angle " + angle.ToString();
            lastImgT = ImgT;
        }
    }
}
