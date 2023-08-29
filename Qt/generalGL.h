#ifndef GENERAL_H
#define GENERAL_H

#include <QImage>
#include <QColor>


#include <QWidget>
#include <QOpenGLWidget>
#include <QOpenGLFunctions>
#include <QMatrix4x4>
#include <QQuaternion>
#include <QVector2D>
#include <QOpenGLShaderProgram>
#include <QOpenGLTexture>
#include <QOpenGLFramebufferObject>
#include <QPainter>
#include <QPaintEvent>

#include <list>
#include <stdlib.h>     /* srand, rand */
#include <time.h>       /* time */

#include "argent.h"
#include "vector.h"
#include "plan.h"

#include <algorithm>

#include <math.h>
#include <stdlib.h>

class GeneralGL : public QOpenGLWidget, public QOpenGLFunctions
{

public:
    //GeneralGL( QWidget *parent);
    using QOpenGLWidget::QOpenGLWidget;
    ~GeneralGL();
    void upd(QImage* img);
    void initExt();
public slots:
    void extUpdate();
protected:
    void initializeGL() override;
    void initShaders();
    void initTextures();
    void resizeGL(int w, int h) override;
    void paintGL() override;
private:
    void initImg(QImage* img);
    float scaleToRange01(int random);
    //void blur();

    Plan* plan = nullptr;
    QOpenGLShaderProgram program;
    QMatrix4x4 projection;
    QOpenGLTexture* texture = nullptr;
    QOpenGLFramebufferObject *fbo = nullptr;
    QImage* img;
    std::list<Argent> listAgent;
};

#endif // GENERAL_H
