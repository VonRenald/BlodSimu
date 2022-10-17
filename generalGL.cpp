#include "generalGL.h"
#include <QDebug>




//GeneralGL::GeneralGL(QWidget *parent)
//    : QOpenGLWidget(parent)
//{
//    srand (time(NULL));

//    img = new QImage(512,512,QImage::Format_Grayscale8);
//    initImg(img);

//    for(int i=0; i<1000;i++)
//    {
//        int x = rand()% img->width();
//        int y = rand()% img->height();
//        int random = (int)(rand() * y * img->width() +x)%100;
//        listAgent.push_back(Argent(myVec2(rand()%img->width(),rand()%img->height()),(random/100.0f)*2*3.14f));
//    }
//}

GeneralGL::~GeneralGL()
{
    makeCurrent();
    delete texture;
    delete fbo;
    delete plan;
    doneCurrent();
}

static const char* vertexshadersource =
        "#ifdef GL_ES\n"
        "// Set default precision to medium\n"
        "precision mediump int;\n"
        "precision mediump float;\n"
        "#endif\n"
        "uniform mat4 mvp_matrix;\n"
        "attribute vec4 a_position;\n"
        "attribute vec2 a_texcoord;\n"
        "varying vec2 v_texcoord;\n"
        "void main()\n"
        "{\n"
        "    // Calculate vertex position in screen space\n"
        "    gl_Position = mvp_matrix * a_position;\n"
        "    //gl_Position = a_position;\n"
        "    // Pass texture coordinate to fragment shader\n"
        "    // Value will be automatically interpolated to fragments inside polygon faces\n"
        "    v_texcoord = a_texcoord;\n"
        "}\0";

static const char* fragmentshadersource =
        "#ifdef GL_ES\n"
        "// Set default precision to medium\n"
        "precision mediump int;\n"
        "precision mediump float;\n"
        "#endif\n"
        "uniform sampler2D texture;\n"
        "varying vec2 v_texcoord;\n"
        "void main() {\n"
        "   float diffSpeed = 0.4;\n"
        "   float diffEvap =  0.1;\n"
        "   float xs = 1280.0;\n"
        "   float ys = 780.0;\n"
        "   vec4 col = vec4(0.0,0.0,0.0,0.0);\n"
        "   float xp = 1/1280.0;\n"
        "   float yp = 1/780.0;\n"
        "   \n"
        "   col += texture2D(texture, v_texcoord);\n"
        "   col += texture2D(texture, v_texcoord+vec2(xp,0));\n"
        "   col += texture2D(texture, v_texcoord+vec2(xp,yp));\n"
        "   col += texture2D(texture, v_texcoord+vec2(0,yp));\n"
        "   col += texture2D(texture, v_texcoord+vec2(-xp,yp));\n"
        "   col += texture2D(texture, v_texcoord+vec2(-xp,0));\n"
        "   col += texture2D(texture, v_texcoord+vec2(-xp,-yp));\n"
        "   col += texture2D(texture, v_texcoord+vec2(0,-yp));\n"
        "   col += texture2D(texture, v_texcoord+vec2(xp,-yp));\n"
        "   col = vec4(col.r/9.0,col.g/9.0,col.b/9.0,col.a);\n"
        "   vec4 color = texture2D(texture, v_texcoord);\n"
        "   \n"
        "   vec4 originalVal = texture2D(texture, v_texcoord);"
        "   float diffValue = originalVal.r + ((col.r-originalVal.r)*diffSpeed);\n"
        "   float setval = (diffValue-diffEvap>=0)?diffValue-0.0025:0.0;"
        "   gl_FragColor = vec4(setval,setval,setval,1.0);\n"
        "}\0";

void GeneralGL::initializeGL()
{


    qDebug() << "initializeGL";


    initializeOpenGLFunctions();

    glClearColor(0.2, 0.2, 0.2, 1);

    initShaders();
    initTextures();

    // Enable depth buffer
    glEnable(GL_DEPTH_TEST);

    // Enable back face culling
    glEnable(GL_CULL_FACE);

    glEnable(GL_BLEND);
    glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);

    plan = new Plan;
    fbo =new QOpenGLFramebufferObject(1280,720);
}

void GeneralGL::initShaders()
{
    qDebug() << "initShaders";
    if (!program.addShaderFromSourceCode(QOpenGLShader::Vertex, vertexshadersource))
            close();

    // Compile fragment shader
    if (!program.addShaderFromSourceCode(QOpenGLShader::Fragment, fragmentshadersource))
        close();

    // Link shader pipeline
    if (!program.link())
        close();

    // Bind shader pipeline for use
    if (!program.bind())
        close();
}

void GeneralGL::initTextures()
{
    qDebug() << "initTextures";
    img = new QImage(1280,720, QImage::Format_RGB16);
    QImage test(1280,720, QImage::Format_RGB16);
    for(int x = 0; x < img->width(); x++)
    {
        for(int y = 0; y < img->height(); y++)
        {
            int gray = 0;
            img->setPixelColor(x,y,QColor(gray,gray,gray));
            //test.setPixelColor(x,y,QColor(gray,gray,gray));
        }
    }
    srand (time(NULL));
    for(int i=0; i<20000;i++)
    {
        int x = rand()% img->width();
        int y = rand()% img->height();
        //img->setPixelColor(x,y,QColor(255,255,255));
        int random = (int)(rand() * y * img->width() +x)%100;
        Vec2 pos;
        pos = randomCircleVec2(300);
        pos.x += (img->width())/2.0;
        pos.y += (img->height())/2.0;
        //listAgent.push_back(Argent(myVec2(rand()%img->width(),rand()%img->height()),(random/100.0f)*2*3.14f));
        listAgent.push_back(Argent(pos,(random/100.0f)*2*3.14f));
    }

    texture = new QOpenGLTexture(*img);
    // Set nearest filtering mode for texture minification
    texture->setMinificationFilter(QOpenGLTexture::Nearest);

    // Set bilinear filtering mode for texture magnification
    texture->setMagnificationFilter(QOpenGLTexture::Linear);

    // Wrap texture coordinates by repeating
    // f.ex. texture coordinate (1.1, 1.2) is same as (0.1, 0.2)
    texture->setWrapMode(QOpenGLTexture::Repeat);
}

void GeneralGL::resizeGL(int w, int h)
{
//    qDebug() << "resizeGL" << w << h;

    // Reset projection
    projection.setToIdentity();
    // Set ortho projection
    projection.ortho(-1,1,-1,1,-10,10);
    //update();
}

void GeneralGL::paintGL()
{
    //makeCurrent();

    //qDebug("paintGL");
    fbo->bind();
    glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

    texture->bind();
    QMatrix4x4 matrix;
    matrix.translate(0.0, 0.0, -4);
    program.setUniformValue("mvp_matrix", projection * matrix);
    program.setUniformValue("texture", 0);
    plan->drawPlanGeometry(&program);
    fbo->release();
    QImage img2 = fbo -> toImage();
    img2 = img2.mirrored();
    //free(*img);
    //img = &img2;
    upd(&img2);
    texture->setData(img2);
//    texture = new QOpenGLTexture(img2);
//    texture->setMinificationFilter(QOpenGLTexture::Nearest);
//    texture->setMagnificationFilter(QOpenGLTexture::Linear);
//    texture->setWrapMode(QOpenGLTexture::Repeat);
    plan->drawPlanGeometry(&program);
//    doneCurrent();
}

void GeneralGL::initExt()
{
    initializeGL();
    //resizeGL(500,500);
}

void GeneralGL::extUpdate()
{
    //upd();
    update();
}

//void GeneralGL::blur(){
//    float diffuseSpeed = 0.1f;
//    float diffuseEvap = 0.005f;
//    QImage* out  = new QImage(img->width(),img->height(),QImage::Format_Grayscale8);
//    initImg(out);
//    QColor color;
////    #pragma omp parallel num_threads(4)
////    #pragma omp for
//    for(int j=0;j<img->height();j++){
//        for(int i=0;i<img->width();i++){
//            color = img->pixelColor(i,j);
//            int originalVal = color.red();

//            int r=0;
//            for(int offsetj=-1;offsetj<=1;offsetj++){
//                for(int offseti=-1; offseti<=1;offseti++){
//                    int x = i+offseti;
//                    int y =j+offsetj;
//                    if(!(x<0||x>img->width()-1||y<0||y>img->height()-1)){
//                        color = img->pixelColor(x,y);
//                        r+=color.red();
//                    }
//                }
//            }
//            QColor sum(r/9,r/9,r/9);
//            r= r/9;
//            int diffuseValue = originalVal + ((r-originalVal)*diffuseSpeed);
//            int setval = (diffuseValue-diffuseEvap>=0)?diffuseValue:0;
//            out->setPixelColor(i,j,QColor(setval,setval,setval));

//        }
//    }
//    free(img);
//    img = out;

//}

void GeneralGL::upd(QImage* img_)
{
    //blur();
    int i = 0;
    for(auto iter = listAgent.begin();iter != listAgent.end();iter++)
    {
        (*iter).agentUpdate(img_->width(),img_->height(),img_);
        //qDebug() << (*iter).position.x << " " << (*iter).position.y;
        img_->setPixelColor(int((*iter).position.x),int((*iter).position.y),QColor(255,255,255));
        i++;
    }
    //qDebug("i %d\n",i);
    //update();
}

//void GeneralGL::paintEvent(QPaintEvent *event)
//{
//    QPainter painter;
//    painter.begin(this);
//    painter.drawImage(event->rect(),*img);
//    painter.end();
//}
