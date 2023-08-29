// Convertit des radians en degrés
function radiansToDegrees(radians) {
    return radians * (180 / Math.PI);
}

// Convertit des degrés en radians
function degreesToRadians(degrees) {
    return degrees * (Math.PI / 180);
}

class Vector2
{
    constructor(x,y){
        this.x = x;
        this.y = y;
    }
    adde(i){
        return new Vector2(this.x+i,this.y+i);
    }
    add(v){
        return new Vector2(this.x+v.x,this.y+v.y);
    }
    mule(i){
        return new Vector2(this.x*i,this.y*i);
    }
    mul(v){
        return new Vector2(this.x*v.x,this.y*v.y);
    }
}

class Agent
{
    constructor(size_,position_,angle_=0.0,moveSpeed_=2.0,angleSpeed_=1.0,sensorOffsetDist_=3.0,sensorSize_=3.0,sensorAngleOffset_=1.0){
        this.size=size_;//vector2
        this.position=position_;//vector2
        this.angle=angle_;//float
        this.moveSpeed=moveSpeed_;//float
        this.angleSpeed=angleSpeed_;//float
        this.sensorOffsetDist=sensorOffsetDist_;//float
        this.sensorSize=sensorSize_;
        this.sensorAngleOffset=sensorAngleOffset_;//float
    }
    sens(sensorAngleOffset_,matrix){
        const sensorAngle = degreesToRadians( this.angle+sensorAngleOffset_);
        const sensorDir = new Vector2(Math.cos(sensorAngle),Math.sin(sensorAngle));
        const sensorCenter = this.position.add(sensorDir.mule(this.sensorOffsetDist));
        
        let sum = 0;
        for(let offsetX= Math.floor(-this.sensorSize); offsetX < Math.floor(this.sensorSize); offsetX++){
            for(let offsetY= -this.sensorSize; offsetY < this.sensorSize; offsetY++){
                const pos = sensorCenter.add(new Vector2(offsetX,offsetY));
                // console.log("x:"+pos.x+" y:"+pos.y);
                if(pos.x >= 0 && pos.x < this.size.x && pos.y >= 0 && pos.y < this.size.y){
                    sum += matrix[Math.floor(Math.floor(pos.x) + Math.floor(pos.y)*this.size.x)] & 0xFF;
                    // console.log(matrix[Math.floor(pos.x) + Math.floor(pos.y)*this.size.x]& 0xFF)
                }
            }
        }
        return sum;
    }
    Update(matrix)
    {
        let rad = degreesToRadians(this.angle);
        let angleDir = new Vector2(Math.cos(rad),Math.sin(rad));
        let newPosition = this.position.add(angleDir.mule(this.moveSpeed));
        // console.log("pos :"+ this.position.x+" new pos:"+newPosition.x );
        let loop=0;
        while(newPosition.x<0 || newPosition.x>=this.size.x || newPosition.y<0 || newPosition.y>=this.size.y)
        {
            if (newPosition.x<=0 || newPosition.x >=this.size.x-1)
            {
                angleDir.x = -angleDir.x;
            }
            if (newPosition.y<=0 || newPosition.y >=this.size.y-1)
            {
                angleDir.y = -angleDir.y;
            }
            this.angle = radiansToDegrees(Math.atan2(angleDir.y,angleDir.x));
            newPosition = this.position.add(angleDir.mule(this.moveSpeed));
            // console.log("new pos : "+ newPosition.x);
            loop++;
            if (loop>10){newPosition = new Vector2(Math.random()*this.size.x,Math.random()*this.size.y);}
        }
        this.position = newPosition;
        

        const left = this.sens(this.sensorAngleOffset, matrix);
        const right = this.sens(-this.sensorAngleOffset, matrix);

        let vir = (this.angleSpeed * Math.random()*2)%360;
        // console.log("L"+left+" R"+right+" vir:"+vir);

        if(right > left){
            this.angle = (this.angle-vir)%360;
            // console.log("helo");

        }else  if(right < left){
            this.angle = (this.angle+vir)%360;
        }
    }
    newAngleSpeed(angleSpeed_)
    {
        this.angleSpeed = angleSpeed_;
    }
}

function initAgent(){
    let agents = [];
    for(let i=0; i<10000;i++){
        const x = Math.floor((Math.random()*(textureSize/2))+(textureSize/4));
        const y = Math.floor((Math.random()*(textureSize/2))+(textureSize/4));
        const r = Math.floor((Math.random()*360));
        agents.push(new Agent(new Vector2(textureSize,textureSize),new Vector2(x,y),r,1.0,8.0,4.0,2.0,20.0));
    }
    return agents;
}

const canvas = document.getElementById('myCanvas');
const textAngle = document.getElementById('angle');
const gl = canvas.getContext('webgl');


// Vertex Shader
const vertexShaderSource = `
    attribute vec4 aPosition;
    attribute vec2 aTexCoord;
    varying vec2 vTexCoord;
    void main() {
        gl_Position = aPosition;
        vTexCoord = aTexCoord;
    }
`;

// Fragment Shader for Dividing Texture Values
const fragmentDivideShaderSource = `
    precision mediump float;
    varying vec2 vTexCoord;
    uniform sampler2D uTexture;
    void main() {

        float Width = 128.0;
        float Height = 128.0;

        vec4 color = texture2D(uTexture, vTexCoord);
        vec4 Ocolor = color;
        //R G B A
        // 0;0 est en bas a gauche
        vec2 coor = vec2(Width * vTexCoord.x,Height * vTexCoord.y); // coordonee entre 0 et tailleImg;
        vec2 unit = vec2(1.0/Width,1.0/Height); // taille de 1 pixel entre 0 et 1; 
        
        if(coor.y>1.0 && (coor.y<Height-2.0)){
            color  += texture2D(uTexture, vTexCoord+vec2(0.0,-unit.y));
            color  += texture2D(uTexture, vTexCoord+vec2(0.0,unit.y));
            if(coor.x>2.0){
                color += texture2D(uTexture, vTexCoord+vec2(-unit.x,0.0));
                color += texture2D(uTexture, vTexCoord+vec2(-unit.x,-unit.y));
                color += texture2D(uTexture, vTexCoord+vec2(-unit.x,unit.y));
            }
            if(coor.x<Width-2.0){
                color += texture2D(uTexture, vTexCoord+vec2(unit.x,0.0));
                color += texture2D(uTexture, vTexCoord+vec2(unit.x,-unit.y));
                color += texture2D(uTexture, vTexCoord+vec2(unit.x,unit.y));
            }
        }
        color = color/9.0;
        color = (Ocolor+color)/2.0;
        color.x = (color.x<0.0)? 0.0:color.x-4.0/255.0;
        color.y = (color.y<0.0)? 0.0:color.y-4.0/255.0;
        color.z = (color.z<0.0)? 0.0:color.z-4.0/255.0;
        // color -= 1.0/255.0;

        color.w = 1.0;

        gl_FragColor = color;
        
    }
`;

function compileShader(source, type) {
    const shader = gl.createShader(type);
    gl.shaderSource(shader, source);
    gl.compileShader(shader);
    if (!gl.getShaderParameter(shader, gl.COMPILE_STATUS)) {
        console.error('Failed to compile shader:', gl.getShaderInfoLog(shader));
        return null;
    }
    return shader;
}

const vertexShader = compileShader(vertexShaderSource, gl.VERTEX_SHADER);
const fragmentDivideShader = compileShader(fragmentDivideShaderSource, gl.FRAGMENT_SHADER);

const programDivide = gl.createProgram();
gl.attachShader(programDivide, vertexShader);
gl.attachShader(programDivide, fragmentDivideShader);
gl.linkProgram(programDivide);

// Buffer for square vertices
const vertices = new Float32Array([
    -1.0, 1.0, 0.0, 1.0,
    -1.0, -1.0, 0.0, 0.0,
    1.0, 1.0, 1.0, 1.0,
    1.0, -1.0, 1.0, 0.0,
]);

const buffer = gl.createBuffer();
gl.bindBuffer(gl.ARRAY_BUFFER, buffer);
gl.bufferData(gl.ARRAY_BUFFER, vertices, gl.STATIC_DRAW);

function createTexture(data) {
    const tex = gl.createTexture();
    gl.bindTexture(gl.TEXTURE_2D, tex);
    gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, 256, 256, 0, gl.RGBA, gl.UNSIGNED_BYTE, data ? new Uint8Array(data.buffer) : null);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.NEAREST);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.NEAREST);
    return tex;
}

// Texture handling
const textureSize = 128;
const textureData = new Uint32Array(textureSize * textureSize).fill(0xFF000000);


let textureA = createTexture(textureData);
let textureB = createTexture(textureData);

const framebufferA = gl.createFramebuffer();
gl.bindFramebuffer(gl.FRAMEBUFFER, framebufferA);
gl.framebufferTexture2D(gl.FRAMEBUFFER, gl.COLOR_ATTACHMENT0, gl.TEXTURE_2D, textureA, 0);

const framebufferB = gl.createFramebuffer();
gl.bindFramebuffer(gl.FRAMEBUFFER, framebufferB);
gl.framebufferTexture2D(gl.FRAMEBUFFER, gl.COLOR_ATTACHMENT0, gl.TEXTURE_2D, textureB, 0);

const readFramebuffer = gl.createFramebuffer();

let agents=initAgent();
console.log("nb agents : " +agents.length);
let loopAngle = 0;
let sensorA = 20.0;

let drawToA = true;
function animate() {
    let sourceFB, destFB, sourceTex, destTex;
    if (drawToA) {
        sourceFB = framebufferB;
        destFB = framebufferA;
        sourceTex = textureB;
        destTex = textureA;
        
        
    } else {
        sourceFB = framebufferA;
        destFB = framebufferB;
        sourceTex = textureA;
        destTex = textureB;
        
    }

    gl.bindTexture(gl.TEXTURE_2D, sourceTex);
    gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, textureSize, textureSize, 0, gl.RGBA, gl.UNSIGNED_BYTE, new Uint8Array(textureData.buffer));
    // Set source and destination framebuffers and textures
    gl.bindFramebuffer(gl.FRAMEBUFFER, destFB);
    // Assurez-vous que la taille de la viewport correspond à la taille de la texture/framebuffer
    gl.viewport(0, 0, textureSize, textureSize);

    gl.bindTexture(gl.TEXTURE_2D, sourceTex);
    
    gl.useProgram(programDivide);
    bindBufferAndAttributes(programDivide);
    gl.drawArrays(gl.TRIANGLE_STRIP, 0, 4);

    // Draw to the main canvas
    gl.bindFramebuffer(gl.FRAMEBUFFER, null);
    // Réglez la taille de la viewport pour correspondre à la taille du canvas
    gl.viewport(0, 0, canvas.width, canvas.height);
    gl.bindTexture(gl.TEXTURE_2D, destTex);
    gl.drawArrays(gl.TRIANGLE_STRIP, 0, 4);

    //sauver texture
    gl.bindFramebuffer(gl.FRAMEBUFFER, readFramebuffer);
    gl.framebufferTexture2D(gl.FRAMEBUFFER, gl.COLOR_ATTACHMENT0, gl.TEXTURE_2D, destTex, 0);  // Changez `textureA` par la texture que vous voulez sauvegarder

    if (gl.checkFramebufferStatus(gl.FRAMEBUFFER) === gl.FRAMEBUFFER_COMPLETE) {
        const pixels = new Uint8Array(textureSize * textureSize * 4);
        gl.readPixels(0, 0, textureSize, textureSize, gl.RGBA, gl.UNSIGNED_BYTE, pixels);
        textureData.set(new Uint32Array(pixels.buffer));
    } else {
        console.error("Problème avec le framebuffer. La texture n'a pas été lue correctement.");
    }
    // Détachez le framebuffer
    gl.bindFramebuffer(gl.FRAMEBUFFER, null);

    // console.log(textureData[0]&0xFF);
    // Switch for the next iteration
    // console.log("angle " + agents[0].angle);
    for(let i=0; i<agents.length;i++){
        agents[i].Update(textureData);
        textureData[Math.floor(agents[i].position.x)+ Math.floor(agents[i].position.y)*textureSize] = 0xFF0000FF;
        agents[i].newAngleSpeed(loopAngle);
        agents[i].sensorAngleOffset=sensorA;
        // console.log("textur coor " + agents[i].position.x + " " + agents[i].position.y + " angle " + agents[i].angle);
    }
    textAngle.textContent = loopAngle;
    // textureData.fill(0xFF0000FF);
    drawToA = !drawToA;
    loopAngle+=0.01;
    if (loopAngle>30.0)
        sensorA = (sensorA<0.2)? 0.2 : sensorA-0.01;
    if(loopAngle>50.0 && sensorA <= 0.2){
        loopAngle = 0.0;
        sensorA = 20.0;
    }
    requestAnimationFrame(animate);
    
}

function bindBufferAndAttributes(program) {
    gl.bindBuffer(gl.ARRAY_BUFFER, buffer);
    const aPosition = gl.getAttribLocation(program, "aPosition");
    gl.enableVertexAttribArray(aPosition);
    gl.vertexAttribPointer(aPosition, 2, gl.FLOAT, false, 16, 0);

    const aTexCoord = gl.getAttribLocation(program, "aTexCoord");
    gl.enableVertexAttribArray(aTexCoord);
    gl.vertexAttribPointer(aTexCoord, 2, gl.FLOAT, false, 16, 8);
}




animate();
