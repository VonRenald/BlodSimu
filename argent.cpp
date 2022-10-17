#include "argent.h"
#include <QDebug>

Argent::Argent(Vec2 pos, float angle)
{
    this->position = pos;
    this->angle = angle;
    this->moveSpeed = 2;
}

float Argent::max(float a, float b)
{
    return (a>b)? a : b;
}

float Argent::min(float a, float b)
{
    return (a<b)? a : b;
}

float Argent::scaleToRange01(int random){
    return (random/100.0f);
}

float Argent::sense(float sensorAngleOffset,int width, int height, QImage* img){
    float sensorOffsetDist = 4.f;
    float sensorSize = 1.5f;

    float sensorAngle = this->angle + sensorAngleOffset;
    Vec2 sensorDir = myVec2(cos(sensorAngle),sin(sensorAngle));
    Vec2 sensorCentre = addVector(this->position, prodVectorConst(sensorDir, sensorOffsetDist));
    float sum = 0;

    for(int offsetX = -sensorSize; offsetX <= sensorSize; offsetX ++){
        for(int offsetY = -sensorSize; offsetY <= sensorSize; offsetY ++){
            Vec2 pos = addVector(sensorCentre,myVec2(offsetX,offsetY));

            if (pos.x >=0 && pos.x < width && pos.y >= 0 && pos.y < height){
                QColor ptr = img->pixelColor(pos.x,pos.y); //getPixelPtr(img, pos.x, pos.y);
                sum+= ptr.red();
            }
        }
    }
    return sum;
}

void Argent::agentUpdate (int width, int height, QImage* img){
    int r = rand()%100;
    float turnSpeed = 1.0f;
    // printf("\nr %d\n",r);
    // int random = (int)(r * a->position.y * width +a->position.x);

    Vec2 direction = myVec2(cos(this->angle),sin(this->angle));
    Vec2 newPos = addVector(this->position,prodVectorConst(direction,this->moveSpeed));
    //qDebug("pos: <%f,%f> new pos: <%f;%f>",this->position.x,this->position.y,newPos.x,newPos.y);
    if(newPos.x < 0 || newPos.x >= width || newPos.y < 0 || newPos.y >= height){
        newPos.x = min(width-0.01f,max(0.f,newPos.x));
        newPos.y = min(height-0.01f,max(0.f,newPos.y));
        this->angle = scaleToRange01(r) * 2 * 3.14f;
        //qDebug("r %d other %f\n",r,this->position.y * width +this->position.x);
        //qDebug() << "r " << r << "other " <<this->position.y * width +this->position.x;
        // printf("\na %f\n",a->angle);

    }
    this->position = newPos;
    float randomSteerStrength = scaleToRange01(r);
    float weightForward = 0;
    float weightLeft = 0;
    float weightRight = 0;
    //qDebug("pos: <%f,%f> new pos: <%f;%f>",this->position.x,this->position.y,newPos.x,newPos.y);
    weightForward = sense(0,width,height,img);
    weightLeft = sense(0.79f,width,height,img);
    weightRight = sense(-0.79f,width,height,img);


    if(weightForward>weightLeft && weightForward>weightRight){
        this->angle += 0;
    }
    else if(weightRight>weightLeft){
        this->angle -= randomSteerStrength * turnSpeed;
    }else if(weightRight<weightLeft){
        this->angle += randomSteerStrength * turnSpeed;
    }

}
