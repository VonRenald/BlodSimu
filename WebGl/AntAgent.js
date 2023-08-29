class Vector2
{
    constructor(x,y){
        this.x = x;
        this.y = y;
    }
    add(i){
        return Vector2(this.x+i,this.y+i);
    }
    add(v){
        return Vector2(this.x+v.x,this.y+v.y);
    }
    mul(i){
        return Vector2(this.x*i,this.y*i);
    }
    mul(v){
        return Vector2(this.x*v.x,this.y*v.y);
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
        const sensorAngle = this.angle+sensorAngleOffset_;
        const sensorDir = Vector2(Math.cos(sensorAngle),Math.sin(sensorAngle));
        const sensorCenter = this.position.add(sensorDir.mul(sensorOffsetDist));
        let sum = 0;
        for(let offsetX= -this.sensorSize; offsetX < this.sensorSize; offsetX++){
            for(let offsetY= -this.sensorSize; offsetY < this.sensorSize; offsetY++){
                const pos = sensorCenter.add(Vector2(offsetX,offsetY));
                if(pos.x >= 0 && pos.x < this.size.x && pos.y >= 0 && pos.y < this.size.y){
                    sum += matrix[Math.floor(pos.x + pos.y*this.size.x)] && 0xFF;
                }
            }
        }
        return sum;
    }
    Update(matrix)
    {
        let angleDir = Vector2(Math.cos(this.angle),Math.sin(this.angle));
        let newPosition = this.position.add(angleDir.mul(this.moveSpeed));
        while(newPosition.x<0 || newPosition.x>=this.size.x || newPosition.y<0 || newPosition>=this.size.y)
        {
            if (newPosition.x<0 || newPosition.x >=this.size.x)
            {
                angleDir.x = -angleDir.x;
            }
            if (newPosition.y<0 || newPosition.y >=this.size.y)
            {
                angleDir.y = -angleDir.y;
            }
            this.angle = Math.atan2(angleDir.y,angleDir.x);
            newPosition = this.position.add(angleDir.mul(this.moveSpeed));
        }
        this.position = newPosition;

        const left = this.sens(this.sensorAngleOffset, matrix);
        const right = this.sens(-this.sensorAngleOffset, matrix);

        if(right > left){
            this.angle -= (this.angleSpeed * Math.random())%360;
        }else{
            this.angle += (this.angleSpeed * Math.random())%360;
        }
    }
}

