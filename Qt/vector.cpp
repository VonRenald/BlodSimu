#include <stdio.h>
#include <math.h>
#include <stdlib.h>
#include <time.h>
#include "vector.h"

Vec2 myVec2(float x, float y){
    Vec2 vec;
    vec.x = x;
    vec.y = y;
    return vec;
}

Vec2 randomCircleVec2(int r ){
    float radValue = (rand()%628)/10.0f;
    Vec2 vec = myVec2(cosf(radValue)* (rand()%r),sinf(radValue)*(rand()%r));
    return vec;
}

Vec2 addVector(Vec2 a, Vec2 b){
    return myVec2(a.x+b.x,a.y+b.y);
}

Vec2 sousVector(Vec2 a, Vec2 b){
    return myVec2(a.x-b.x,a.y-b.y);
}

Vec2 sousVectorConst(Vec2 a, float c){
    return myVec2(a.x-c,a.y-c);
}

Vec2 prodVector(Vec2 a, Vec2 b){
    return myVec2(a.x*b.x,a.y*b.y);
}

Vec2 prodVectorConst(Vec2 a, float c){
    return myVec2(a.x*c,a.y*c);
}

float magnitudeVector(Vec2 a){
    return sqrtf(powf(a.x,2)+powf(a.y,2));
}

Vec2 normalize(Vec2 a){
    float longa = magnitudeVector(a);
    return prodVectorConst(a,1/longa);
}

Vec2 ClampMagnitude(Vec2 a, float c){
    float mag = magnitudeVector(a);
    while(mag>abs(c)){
        a.x = (a.x > 0)? a.x-1 : a.x+1;
        a.y = (a.y > 0)? a.y-1 : a.y+1;
        mag = magnitudeVector(a);
    }
    return a;
}

Vec2 lerp(Vec2 a, Vec2 b, float t){
    return addVector(a,prodVectorConst(sousVector(b,a),t));
}
