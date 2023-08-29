#ifndef VECTOR_H
#define VECTOR_H


typedef struct vec2_s{
    float x;
    float y;
}Vec2;

Vec2 myVec2(float x, float y);
Vec2 randomCircleVec2(int r);
Vec2 addVector(Vec2 a, Vec2 b);
Vec2 sousVector(Vec2 a, Vec2 b);
Vec2 sousVectorConst(Vec2 a, float c);
Vec2 prodVector(Vec2 a, Vec2 b);
Vec2 prodVectorConst(Vec2 a, float c);
float magnitudeVector(Vec2 a);
Vec2 normalize(Vec2 a);
Vec2 ClampMagnitude(Vec2 a, float c);
Vec2 lerp(Vec2 a, Vec2 b, float t);

#endif // VECTOR_H
