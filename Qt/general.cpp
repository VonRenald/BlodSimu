#include "general.h"
#include <QDebug>



General::General(QWidget *parent)
    : QWidget(parent)
{
    srand (time(NULL));

    img = new QImage(512,512,QImage::Format_Grayscale8);
    initImg(img);

    for(int i=0; i<1000;i++)
    {
        int x = rand()% img->width();
        int y = rand()% img->height();
        int random = (int)(rand() * y * img->width() +x)%100;
        listAgent.push_back(Argent(myVec2(rand()%img->width(),rand()%img->height()),(random/100.0f)*2*3.14f));
    }
}

void General::initImg(QImage* img_)
{
    for(int x = 0; x < img_->width(); x++)
    {
        for(int y = 0; y < img_->height(); y++)
        {
            int gray = 0;
            img_->setPixelColor(x,y,QColor(gray,gray,gray));
        }
    }
}

void General::blur(){
    float diffuseSpeed = 0.1f;
    float diffuseEvap = 0.005f;
    QImage* out  = new QImage(img->width(),img->height(),QImage::Format_Grayscale8);
    initImg(out);
    QColor color;
//    #pragma omp parallel num_threads(4)
//    #pragma omp for
    for(int j=0;j<img->height();j++){
        for(int i=0;i<img->width();i++){
            color = img->pixelColor(i,j);
            int originalVal = color.red();

            int r=0;
            for(int offsetj=-1;offsetj<=1;offsetj++){
                for(int offseti=-1; offseti<=1;offseti++){
                    int x = i+offseti;
                    int y =j+offsetj;
                    if(!(x<0||x>img->width()-1||y<0||y>img->height()-1)){
                        color = img->pixelColor(x,y);
                        r+=color.red();
                    }
                }
            }
            QColor sum(r/9,r/9,r/9);
            r= r/9;
            int diffuseValue = originalVal + ((r-originalVal)*diffuseSpeed);
            int setval = (diffuseValue-diffuseEvap>=0)?diffuseValue:0;
            out->setPixelColor(i,j,QColor(setval,setval,setval));

        }
    }
    free(img);
    img = out;

}

void General::upd()
{
    //blur();
    for(auto iter = listAgent.begin();iter != listAgent.end();iter++)
    {
        (*iter).agentUpdate(img->width(),img->height(),img);
        //qDebug() << (*iter).position.x << " " << (*iter).position.y;
        img->setPixelColor(int((*iter).position.x),int((*iter).position.y),QColor(255,255,255));
    }
    update();
}

void General::paintEvent(QPaintEvent *event)
{
    QPainter painter;
    painter.begin(this);
    painter.drawImage(event->rect(),*img);
    painter.end();
}
