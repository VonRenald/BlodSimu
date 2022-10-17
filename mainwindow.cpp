#include "mainwindow.h"
#include "generalGL.h"
MainWindow::MainWindow(QWidget *parent)
    : QMainWindow(parent)
{
    this->setFixedSize(1280,720);
//    General *gene = new General(this);
//    gene->setFixedSize(this->size());
    GeneralGL *gene = new GeneralGL(this);
    gene->setFixedSize(this->size());

    QTimer *timer = new QTimer(this);
//    connect(timer, &QTimer::timeout, gene, &General::upd);
    connect(timer, &QTimer::timeout, gene, &GeneralGL::extUpdate);
    timer->start(0);
}

MainWindow::~MainWindow()
{
}

