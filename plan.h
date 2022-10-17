#ifndef PLAN_H
#define PLAN_H

#include <QOpenGLFunctions>
#include <QOpenGLShaderProgram>
#include <QOpenGLBuffer>
#include <QVector2D>
#include <QVector3D>

class Plan : public QOpenGLFunctions
{
public:
    Plan();
    ~Plan();

    void drawPlanGeometry(QOpenGLShaderProgram* program);
private:
    void initPlanGeometry();

    QOpenGLBuffer arrayBuf;
    QOpenGLBuffer indexBuf;
};

#endif // PLAN_H
