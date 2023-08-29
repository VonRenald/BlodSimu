#ifndef ARGENT_H
#define ARGENT_H

#include "vector.h"
#include <QImage>
#include <QColor>

class Argent
{
public:
    Argent(Vec2 pos, float angle);
    Vec2 position;
    float angle;
    float moveSpeed;
    float scaleToRange01(int random);
    void agentUpdate (int width, int height,QImage* img);
private:


    float min(float a,float b);
    float max(float a,float b);

    float sense(float sensorAngleOffset,int width, int height, QImage* img);

};

#endif // ARGENT_H
