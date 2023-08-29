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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace ant
{
    public class Agent
    {
        public Vector2 position;
        private float angle;
        private float sensorOffsetDist;
        private float sensorSize;
        private float sensorAngleOffset;
        private float moveSpeed;
        private float angleSpeed;
        public Size size;
        private Random rand;
        public Agent(Size s,Vector2 p, float a = 0f, float ms = 2f,float ans =1f, float sod = 3f, float ss = 3f, float sao = 1f)
        {
            size = s;
            position = p;
            angle = a;
            moveSpeed = ms;
            angleSpeed = ans;
            sensorOffsetDist = sod;
            sensorSize = ss;
            sensorAngleOffset = sao;
            rand = new Random();
        }
        public int sense(float sensorAngleOffset_, uint[] matrix)
        {
            float sensorAngle = angle+sensorAngleOffset_;
            Vector2 sensorDir = new Vector2(MathF.Cos(sensorAngle),MathF.Sin(sensorAngle));
            Vector2 sensorCenter = position+(sensorDir*sensorOffsetDist);
            int sum = 0;
            // Console.WriteLine("SensorP {0},{1}",sensorCenter.X,sensorCenter.Y);
            for(int offsetX = (int)-sensorSize; offsetX <= sensorSize; offsetX ++){
                for(int offsetY = (int)-sensorSize; offsetY <= sensorSize; offsetY ++){
                    Vector2 pos =  sensorCenter + new Vector2(offsetX,offsetY);
                    pos.X = (int) pos.X;
                    pos.Y = (int) pos.Y;

                    if (pos.X >=0 && pos.X < size.Width && pos.Y >= 0 && pos.Y < size.Height-1){
                        sum +=  (int) matrix[(int)(pos.X+pos.Y*size.Width)];
                    }
                }
            }
            return sum;
        }
        public void Update(uint[] matrix)
        {
            Vector2 angleDir = new Vector2(MathF.Cos(angle),MathF.Sin(angle));
            Vector2 newPosition = position + angleDir*moveSpeed;

            while(newPosition.X < 0 || newPosition.X >= size.Width || newPosition.Y < 0 || newPosition.Y >= size.Height)
            {
                if (newPosition.X<0 || newPosition.X>=size.Width){
                    angleDir.X = -angleDir.X;
                }
                if (newPosition.Y<0 || newPosition.Y>=size.Height){
                    angleDir.Y = -angleDir.Y;
                }
                angle = MathF.Atan2(angleDir.Y,angleDir.X);
                newPosition = position + angleDir*moveSpeed;
            }
            
            position = newPosition;


            // int front = this.sense(0, matrix);
            int left = this.sense(sensorAngleOffset, matrix);
            int right = this.sense(-sensorAngleOffset, matrix);
            // float sum = front+left+right+1;

            // angle += angleSpeed * (left/sum) %360; 
            // angle -= angleSpeed * (right/sum) %360; 
            if(right>left){
                angle -= angleSpeed * (rand.Next(0,101)/100f) % 360;
            }else {
                angle += angleSpeed * (rand.Next(0,101)/100f) % 360;
            }
        }
        public void NewAngleSpeed(float angleSpeed_){
            angleSpeed = angleSpeed_;
        }
    }
}