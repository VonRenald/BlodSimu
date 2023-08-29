RWStructuredBuffer<uint> Result;

[numthreads(32, 1, 1)]
void CSMain(uint3 id : SV_DispatchThreadID)
{
    int Width = 320;//640;//1280;
    int Height = 180;//360;//780;

    uint val = (Result[id.x]);
    uint Oval = val;
    if(id.x > 0)
        val +=Result[id.x-1];
    if(id.x < Width*Height-1)
        val +=Result[id.x+1];
    if(id.x+Width < Width*Height ){
        val += Result[id.x+Width];
        val += Result[id.x+Width-1];
        if(id.x+Width+1 < Width*Height ){
            val += Result[id.x+Width+1];
        }
    }
    if(id.x-Width >= 0 ){
        val += Result[id.x-Width];
        val += Result[id.x-Width+1];
        if(id.x-Width-1 >= 0 ){
            val += Result[id.x-Width-1];
        }
    }
    val = val/9;
    // if(Oval < 0){
    //     val = (Oval-val)*0.01f+val;
    // }
    val = (Oval+val)/2;
    // val = Result[id.x] - (Result[id.x]-val)*0.01f;

    uint minus = 5;
    Result[id.x] = ((val-minus>255)? 0: val-minus);
    // Result[id.x] = Result[id.x] + (Result[id.x]<<7) + (Result[id.x]<<15);
}
