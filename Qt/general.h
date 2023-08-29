#ifndef GENERAL_H
#define GENERAL_H

#include <QImage>
#include <QColor>


#include <QWidget>
//#include <QGLWidget>
#include <QPainter>
#include <QPaintEvent>

#include <list>
#include <stdlib.h>     /* srand, rand */
#include <time.h>       /* time */

#include "argent.h"
#include "vector.h"

class General : public QWidget
{

public:
    General( QWidget *parent);
    void upd();

protected:
    void paintEvent(QPaintEvent *event) override;
private:
    void initImg(QImage* img);
    float scaleToRange01(int random);
    void blur();

    QImage* img;
    std::list<Argent> listAgent;
};

#endif // GENERAL_H
