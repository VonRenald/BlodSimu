using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

using System.IO;
using System.Reflection;


using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.D3DCompiler;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;
using MapFlags = SharpDX.Direct3D11.MapFlags;

namespace ant
{
    public class ImgAnt
    {
        private UInt32[] matrix;
        public UInt32[] byteMatrix;
        private int Width;
        private int Height;
        private Size size;

        int groupSize = 16; //Needs to match what is written in the shader
        int totalSize; //Needs to be a multiple of groupSize, or else the shader will try to either change part of the array past its length, or not change the last parts of the array
        int elementByteSize = 4;

        //shader
        Device device;
        DeviceContext context;
        ShaderBytecode shaderBytecode;
        ComputeShader computeShader;
        Buffer inputBuffer;
        Buffer outputBuffer;
        ShaderResourceView inputView;
        UnorderedAccessView outputView;
        Buffer paramBuffer;
        BufferDescription inputDesc;
        Buffer buffer;
        ShaderResourceViewDescription srvDesc;
        ShaderResourceView srvs;
        UnorderedAccessViewDescription uavDesc;
        UnorderedAccessView uavs;

        private int M2T(int x, int y){
            return y*Width+x;
        }

        // private void InitShader(){
        //     device = new Device(DriverType.Hardware, DeviceCreationFlags.None);
        //     context = device.ImmediateContext;

        //     //compile shader
        //     shaderBytecode = ShaderBytecode.CompileFromFile("blur.hlsl", "CSMain", "cs_5_0");
        //     computeShader = new ComputeShader(device, shaderBytecode);
        //     context.ComputeShader.Set(computeShader);

        //     var bufferDescription = new BufferDescription()
        //     {
        //         BindFlags = BindFlags.ShaderResource | BindFlags.UnorderedAccess,
        //         OptionFlags = ResourceOptionFlags.BufferStructured,
        //         StructureByteStride = sizeof(float),
        //         SizeInBytes = matrix.Length * sizeof(float),
        //         Usage = ResourceUsage.Default
        //     };
        //     var outputBufferDescription = new BufferDescription()
        //     {
        //         BindFlags = BindFlags.UnorderedAccess | BindFlags.ShaderResource,
        //         OptionFlags = ResourceOptionFlags.BufferStructured,
        //         StructureByteStride = sizeof(float),  // taille de chaque élément
        //         SizeInBytes = matrix.Length * sizeof(float),
        //         Usage = ResourceUsage.Default,
        //         CpuAccessFlags  = CpuAccessFlags.Read
        //     };
        //     //Creation des buffer
        //     inputBuffer = Buffer.Create(device, matrix, bufferDescription);
        //     outputBuffer = new Buffer(device, outputBufferDescription);

            
        //     // Ensuite, créez la ShaderResourceView avec une description
        //     var srvDescription = new ShaderResourceViewDescription()
        //     {
        //         Format = Format.Unknown,
        //         Dimension = ShaderResourceViewDimension.Buffer,
        //         Buffer = new ShaderResourceViewDescription.BufferResource()
        //         {
        //             FirstElement = 0,
        //             ElementCount = matrix.Length
        //         }
        //     };
        //     // Passez le buffer en entrée au shader
        //     inputView = new ShaderResourceView(device, inputBuffer, srvDescription);
        //     context.ComputeShader.SetShaderResource(0, inputView);


        //     var uavDescription = new UnorderedAccessViewDescription()
        //     {
        //         Format = Format.Unknown,  // Pour un StructuredBuffer
        //         Dimension = UnorderedAccessViewDimension.Buffer,
        //         Buffer = new UnorderedAccessViewDescription.BufferResource()
        //         {
        //             FirstElement = 0,
        //             ElementCount = matrix.Length,
        //             Flags = UnorderedAccessViewBufferFlags.None
        //         }
        //     };
        //     // Passez le buffer de sortie au shader
        //     outputView = new UnorderedAccessView(device, outputBuffer, uavDescription);
        //     context.ComputeShader.SetUnorderedAccessView(0, outputView);

        //     // Paramètres (largeur, hauteur et diffusion)
        //     var bufferSize = Utilities.SizeOf<int>() * 2 +  Utilities.SizeOf<float>() * 2;
        //     // bufferSize = (bufferSize + 15) & ~15;  // Alignez la taille sur 16 octets

        //     BufferDescription bufferDescription_param = new BufferDescription
        //     {
        //         Usage = ResourceUsage.Default,
        //         SizeInBytes = bufferSize,
        //         BindFlags = BindFlags.ConstantBuffer
        //     };
        //     paramBuffer = new Buffer(device, bufferDescription_param);
        //     context.UpdateSubresource(new[] { Width, Height, 0.1f, 0.005f}, paramBuffer);
        //     context.ComputeShader.SetConstantBuffer(0, paramBuffer);
        // }
        private void InitShaderV2(){
            var assembly = Assembly.GetExecutingAssembly();
            using Stream stream = assembly.GetManifestResourceStream("ant.Shader.hlsl");
            using StreamReader reader = new StreamReader(stream);
            string result = reader.ReadToEnd();
            Console.Write(result);


            totalSize = Width*Height;
            //Create device
            device = new Device(DriverType.Hardware, DeviceCreationFlags.SingleThreaded);
            //Create compute shader
            // CompilationResult bytecode = ShaderBytecode.CompileFromFile("Shader.hlsl", "CSMain", "cs_5_0"); //(Gotta have the shader-file Shader.hlsl be copied to the output directory for this to work)
            CompilationResult bytecode = ShaderBytecode.Compile(result,"CSMain", "cs_5_0");//CompileFromFile("Shader.hlsl", "CSMain", "cs_5_0"); //(Gotta have the shader-file Shader.hlsl be copied to the output directory for this to work)
            computeShader = new ComputeShader(device, bytecode);
            bytecode.Dispose();

            // /!\ /!\ matrix doit etre init

            inputDesc = new BufferDescription()
            {
                SizeInBytes = elementByteSize * totalSize, //Size of the buffer in bytes
                Usage = ResourceUsage.Default, //Lets the buffer be both written and read by the GPU
                BindFlags = BindFlags.ShaderResource | BindFlags.UnorderedAccess,
                OptionFlags = ResourceOptionFlags.BufferStructured,
                StructureByteStride = elementByteSize, //The size of each element in bytes
                CpuAccessFlags = CpuAccessFlags.Read //Lets the CPU read this buffer
            };
            buffer = SharpDX.Direct3D11.Buffer.Create(device, byteMatrix, inputDesc);

            //Create resource view (Seems to just be needed for the buffer)
            srvDesc = new ShaderResourceViewDescription()
            {
                Format = SharpDX.DXGI.Format.Unknown,
                Dimension = ShaderResourceViewDimension.Buffer,
                Buffer = new ShaderResourceViewDescription.BufferResource()
                {
                    ElementWidth = elementByteSize
                }
            };
            srvs = new ShaderResourceView(device, buffer, srvDesc);
            
            //Create access view (Seems to just be needed for the buffer)
            uavDesc = new UnorderedAccessViewDescription()
            {
                Format = SharpDX.DXGI.Format.Unknown,
                Dimension = UnorderedAccessViewDimension.Buffer,
                Buffer = new UnorderedAccessViewDescription.BufferResource()
                {
                    ElementCount = totalSize
                }
            };
            uavs = new UnorderedAccessView(device, buffer, uavDesc);
            
             //Set up shader
            context = device.ImmediateContext;
            context.ComputeShader.Set(computeShader);

            //Set up shader's buffer
            context.ComputeShader.SetShaderResource(0, srvs);
            context.ComputeShader.SetUnorderedAccessView(0, uavs);


            }
        // private void CloseShader(){
        //     // Nettoyez les ressources (ne pas oublier de libérer toutes les ressources DirectX)
        //     inputBuffer.Dispose();
        //     outputBuffer.Dispose();
        //     paramBuffer.Dispose();
        //     computeShader.Dispose();
        //     shaderBytecode.Dispose();
        //     context.Dispose();
        //     device.Dispose();
        // }
        private void CloseShaderV2(){
            context.ClearState();
            Utilities.Dispose(ref srvs);
            Utilities.Dispose(ref uavs);
            Utilities.Dispose(ref buffer);
            Utilities.Dispose(ref computeShader);
            Utilities.Dispose(ref device);
        }
        public ImgAnt(int Width, int Height){
            this.Width = Width;
            this.Height = Height;
            this.size = new Size(Width,Height);
            this.matrix = new uint[Width*Height]; 
            this.byteMatrix = new UInt32[Width*Height];


            Parallel.For(0, Width, i =>
            {
                for (int j = 0; j < Height; j++)
                {
                    // this.matrix[i*j+j] = 0.5f;//(j % 256)/255f; // Exemple simple pour générer des données
                    this.byteMatrix[M2T((int)i,j)] = 0;//(UInt32)(i % 256);
                    // this.matrix[M2T((int)i,j)] = (uint)j%256;
                }
            });
            // for(int i=0; i<Width*Height; i++)
            //     this.byteMatrix[i] = 255;
            // InitShader();
            InitShaderV2();
            // new GPU_test();
            
        } 
        ~ImgAnt(){
            // CloseShader();
            CloseShaderV2();
        }
        public Image GetImg(int canvasWidth = 0, int canvasHeight = 0){
            if(canvasWidth == 0 || canvasHeight == 0){
                canvasWidth = Width;
                canvasHeight = Height;
            }
            WriteableBitmap wb = new WriteableBitmap(Width, Height, 96, 96, PixelFormats.Bgr32, null);
            wb.WritePixels(new Int32Rect(0,0,Width,Height),byteMatrix,Width*4,0);
            Image image = new Image
            {
                Width = canvasWidth,
                Height = canvasHeight,
                Source = wb
            };
            return image;
        }
        public void Update(Agent[] agents){

            BlurGPUV2();
            DrawAgents(agents);
            // Update buffer with new inputData for next loop
            context.UpdateSubresource(byteMatrix, buffer);
            // Reliez à nouveau le buffer
            context.ComputeShader.SetShaderResource(0, srvs);
            context.ComputeShader.SetUnorderedAccessView(0, uavs);
        }
        private void BlurGPUV2(){
            //Execute shader
            int threadGroupCount = (totalSize + groupSize - 1) / groupSize; // +groupSize-1 to round up
            context.Dispatch(threadGroupCount, 1, 1);
            //Set an array "outputData" equal to the buffer's values
            DataStream ds;
            context.MapSubresource(buffer, MapMode.Read, MapFlags.None, out ds);
            uint[] outputData = ds.ReadRange<uint>(totalSize);
            context.UnmapSubresource(buffer, 0);
            // Update inputData with outputData
            // Console.WriteLine("in data[640,320] : {0}, out data[640,320] : {1}",byteMatrix[321*Width+640], outputData[320*Width+640]);
            Array.Copy(outputData, byteMatrix, byteMatrix.Length);
            
            // Détacher le buffer avant la mise à jour
            context.ComputeShader.SetUnorderedAccessView(0, null);
            context.ComputeShader.SetShaderResource(0, null);
            
            
            
        }
        public void DrawAgents(Agent[] agents)
        {
            for(int i=0;i<agents.Length;i++){
                int x = (int)agents[i].position.X;
                int y = (int)agents[i].position.Y;

                x = (x<0)? 0: (x>=Width)? Width-1:x;
                y = (y<0)? 0: (y>=Height)? Height-1:y;
                byteMatrix[(int) (y*Width+x)] = 255;
                // for(int xx = x-1;xx<x+1;xx++){
                //     for(int yy = y-1;yy<y+1;yy++){
                //         if(xx>=0 && xx <Width && yy >=0 && yy<Height){
                //             byteMatrix[(int) (yy*Width+xx)] = 255;
                //         }
                //     }
                // }
                // Console.WriteLine("coor Print [{0},{1}]",x,y);
            }
        }
        // private void BlurGPU(){

        //     // Passez le buffer en entrée au shader
        //     // var inputView = new ShaderResourceView(device, inputBuffer);
        //     // context.ComputeShader.SetShaderResource(0, inputView);

        //     // Passez le buffer de sortie au shader
        //     // var outputView = new UnorderedAccessView(device, outputBuffer);
        //     // context.ComputeShader.SetUnorderedAccessView(0, outputView);

        //     // Exécutez le shader
        //     context.Dispatch(Width / 16, Height / 16, 1);  // Puisque nous utilisons [numthreads(16, 16, 1)]

        //     // Récupérez les données de sortie
        //     float[] outputData = new float[Width * Height];
        //     context.CopyResource(outputBuffer, inputBuffer);
        
        //     // Lire les données de sortie
        //     var dataStream = context.MapSubresource(outputBuffer, 0, MapMode.Read, MapFlags.None);
        //     Utilities.Read<float>(dataStream.DataPointer, outputData, 0, outputData.Length);
        //     context.UnmapSubresource(outputBuffer, 0);
        //     // for (int i = 0; i < 10; i++)
        //     //     Console.WriteLine(outputData[i]);

        //     Console.WriteLine("out {0}, in {1}",outputData[M2T(0,500)],matrix[M2T(0,500)]);
        //     //met a jour le buffer
        //     // Array.Copy(outputData, matrix, matrix.Length);
        //     // context.UpdateSubresource(matrix, inputBuffer);
        // }
        // public void Blur(float diffuseSpeed = 0.1f, float diffuseEvap = 0.005f){
        //     float[] BluredMatrix = new float[Width*Height];
        //     Parallel.For(0,Height,j => {
        //     // for(int j=0; j<Height; j++){
        //         for(int i=0; i<Width; i++){
                    // float val = 0;
                    // byte n = 0;
                    // for(int ii=(i-1<0)? 0:i-1; ii<=((i+1>Width-1)?Width-1:i+1); ii++){
                    //     for(int jj=(j-1<0)? 0:j-1; jj<=((j+1>Height-1)?Height-1:j+1); jj++){
                    //         val += matrix[M2T(ii,jj)];
                    //         n++;
                    //     }
                    // }
                    // val /= n;
        //             float diffuseValue = matrix[M2T(i,j)] + ((val-matrix[M2T(i,j)])*diffuseSpeed);
        //             if(i==0 && j==0)
        //                 Console.WriteLine("n = {0} val = {1} diffuseValue = {2}",n,val,diffuseValue);
        //             val = (diffuseValue-(int)(diffuseSpeed)<0)?0:diffuseValue-(int)(diffuseSpeed);
        //             BluredMatrix[M2T(i,j)] = val;
        //             byteMatrix[M2T(i,j)] = (byte)(val*255);
        //         }
        //     });
        //     matrix = BluredMatrix;
        // }
    }

}