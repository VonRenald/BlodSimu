using System;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.D3DCompiler;
// using SharpDX.Windows;

using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;
using MapFlags = SharpDX.Direct3D11.MapFlags;

namespace ant
    {
    public class GPU_test
    {
        int Width = 1280;
        int Height = 720;
        public GPU_test(){
            int groupSize = 16; //Needs to match what is written in the shader
            int totalSize = Width*Height; //Needs to be a multiple of groupSize, or else the shader will try to either change part of the array past its length, or not change the last parts of the array
            int elementByteSize = 4; //The size of a single element of the input-data in bytes (An int is made of 4 bytes)
            //Create device
            Device device = new Device(DriverType.Hardware, DeviceCreationFlags.SingleThreaded);
            //Create compute shader
            CompilationResult bytecode = ShaderBytecode.CompileFromFile("Shader.hlsl", "CSMain", "cs_5_0"); //(Gotta have the shader-file Shader.hlsl be copied to the output directory for this to work)
            ComputeShader cs = new ComputeShader(device, bytecode);
            bytecode.Dispose();
            //Create input data (0,1,2,3)
            uint[] inputData = new uint[totalSize];
            for (uint i = 0; i < inputData.Length; i++)
            {
                inputData[i] = i%256;
            }
            // for (int i = 0; i < inputData.Length; i++)
            // {
            //     Console.WriteLine(inputData[i]);
            // }
            Console.WriteLine("Init end max val :{0}",Width*Height -1);
            //Create input buffer that has the input data
            BufferDescription inputDesc = new BufferDescription()
            {
                SizeInBytes = elementByteSize * totalSize, //Size of the buffer in bytes
                Usage = ResourceUsage.Default, //Lets the buffer be both written and read by the GPU
                BindFlags = BindFlags.ShaderResource | BindFlags.UnorderedAccess,
                OptionFlags = ResourceOptionFlags.BufferStructured,
                StructureByteStride = elementByteSize, //The size of each element in bytes
                CpuAccessFlags = CpuAccessFlags.Read //Lets the CPU read this buffer
            };
            SharpDX.Direct3D11.Buffer buffer = SharpDX.Direct3D11.Buffer.Create(device, inputData, inputDesc);
            //Create resource view (Seems to just be needed for the buffer)
            ShaderResourceViewDescription srvDesc = new ShaderResourceViewDescription()
            {
                Format = SharpDX.DXGI.Format.Unknown,
                Dimension = ShaderResourceViewDimension.Buffer,
                Buffer = new ShaderResourceViewDescription.BufferResource()
                {
                    ElementWidth = elementByteSize
                }
            };
            ShaderResourceView srvs = new ShaderResourceView(device, buffer, srvDesc);
            //Create access view (Seems to just be needed for the buffer)
            UnorderedAccessViewDescription uavDesc = new UnorderedAccessViewDescription()
            {
                Format = SharpDX.DXGI.Format.Unknown,
                Dimension = UnorderedAccessViewDimension.Buffer,
                Buffer = new UnorderedAccessViewDescription.BufferResource()
                {
                    ElementCount = totalSize
                }
            };
            UnorderedAccessView uavs = new UnorderedAccessView(device, buffer, uavDesc);
            
            //Set up shader
            DeviceContext context = device.ImmediateContext;
            context.ComputeShader.Set(cs);
            //Set up shader's buffer
            // context.ComputeShader.SetConstantBuffer(0, buffer);
            context.ComputeShader.SetShaderResource(0, srvs);
            context.ComputeShader.SetUnorderedAccessView(0, uavs);
            for(int i=0;i<100;i++)
            {
                //Execute shader
                int threadGroupCount = (totalSize + groupSize - 1) / groupSize; // +groupSize-1 to round up
                context.Dispatch(threadGroupCount, 1, 1);
                //Set an array "outputData" equal to the buffer's values
                DataStream ds;
                context.MapSubresource(buffer, MapMode.Read, MapFlags.None, out ds);
                uint[] outputData = ds.ReadRange<uint>(totalSize);
                Console.WriteLine("in data[640,320] : {0}, out data[640,320] : {1}",inputData[320*Width+640], outputData[320*Width+640]);
                // Array.Copy(outputData, inputData, inputData.Length);
                // inputData[640+320*Width] = outputData[640+320*Width];
                context.UnmapSubresource(buffer, 0);
                // Update inputData with outputData
                Array.Copy(outputData, inputData, inputData.Length);
                
                // Détacher le buffer avant la mise à jour
                context.ComputeShader.SetUnorderedAccessView(0, null);
                context.ComputeShader.SetShaderResource(0, null);
                // Update buffer with new inputData for next loop
                context.UpdateSubresource(inputData, buffer);
                // Reliez à nouveau le buffer
                context.ComputeShader.SetShaderResource(0, srvs);
                context.ComputeShader.SetUnorderedAccessView(0, uavs);
                
            }
            //Dispose stuff
            context.ClearState();
            Utilities.Dispose(ref srvs);
            Utilities.Dispose(ref uavs);
            Utilities.Dispose(ref buffer);
            Utilities.Dispose(ref cs);
            Utilities.Dispose(ref device);
            //Print values
            
            // for (int i = 0; i < outputData.Length; i++)
            // {
            //     Console.WriteLine(outputData[i]);
            // }
            // Console.WriteLine("first+1 in {0}, first+1 out {1}", inputData[1], outputData[1]);
            // Console.WriteLine("last-1 in {0}, last-1 out {1}",inputData[Width*Height -2], outputData[Width*Height -2]);
        }
    }

    public class GPU_test2
    {
        int Width = 1280;
        int Height = 720;
        
        public GPU_test2(){
            int totalSize = Width*Height;
            int groupSize = 16;
            int threadGroupCount = (totalSize + groupSize - 1) / groupSize;

            uint[] inputData = new uint[totalSize];
            for (uint i = 0; i < inputData.Length; i++)
            {
                inputData[i] = i%256;
            }

            var device = new Device(DriverType.Hardware, DeviceCreationFlags.None);
            // Compile the shader.
            CompilationResult computeShaderCode = ShaderBytecode.CompileFromFile("Shader.hlsl", "CSMain", "cs_5_0");
            // var computeShaderCode = ShaderBytecode.Compile(DCShaderSource, "VectorAdd", "cs_5_0", ShaderFlags.None, EffectFlags.None);
            var computeShader = new ComputeShader(device, computeShaderCode);
            DeviceContext context = device.ImmediateContext;
            context.ComputeShader.Set(computeShader);

            // description for input buffers
            var inputBufferDescription = new BufferDescription
            {
                BindFlags = BindFlags.ShaderResource,
                OptionFlags = ResourceOptionFlags.BufferStructured,
                Usage = ResourceUsage.Dynamic,
                CpuAccessFlags = CpuAccessFlags.Write,
                SizeInBytes = inputData.Length * sizeof(uint),
                StructureByteStride = sizeof(uint)
            };

            // Description for the output buffer itself, and the view required to bind it to the pipeline.
            var outputBufferDescription = new BufferDescription
            {
                BindFlags = BindFlags.UnorderedAccess,
                OptionFlags = ResourceOptionFlags.BufferStructured,
                Usage = ResourceUsage.Default,
                CpuAccessFlags = CpuAccessFlags.None,
                SizeInBytes = inputData.Length * sizeof(uint),
                StructureByteStride = sizeof(uint)
            };

            var stagingBufferDescription = new BufferDescription
            {
                BindFlags = BindFlags.None,
                CpuAccessFlags = CpuAccessFlags.Read,
                OptionFlags = ResourceOptionFlags.BufferStructured,
                SizeInBytes = inputData.Length * sizeof(uint),
                StructureByteStride = sizeof(uint),
                Usage = ResourceUsage.Staging,
            };

            var stagingBuffer = new Buffer(device, stagingBufferDescription);
            var outputBuffer = new Buffer(device, outputBufferDescription);


            var outputViewDescription = new UnorderedAccessViewDescription()
            {
                Buffer = new UnorderedAccessViewDescription.BufferResource() { FirstElement = 0, Flags = UnorderedAccessViewBufferFlags.None, ElementCount = inputData.Length },
                Format = SharpDX.DXGI.Format.Unknown,
                Dimension = UnorderedAccessViewDimension.Buffer
            };
            var outputView = new UnorderedAccessView(device, outputBuffer, outputViewDescription);

            Buffer inputBuffer = Buffer.Create(device, inputData, inputBufferDescription);
            var inputView = new ShaderResourceView(device, inputBuffer);

            DataBox output;
            device.ImmediateContext.ComputeShader.SetUnorderedAccessView(0, outputView);
            device.ImmediateContext.ComputeShader.SetShaderResource(0, inputView);
            for (int i = 0; i < 5; i++)
            {
                context.Dispatch(threadGroupCount, 1, 1);
                device.ImmediateContext.CopyResource(outputBuffer, stagingBuffer);
                DataStream result;
                output = device.ImmediateContext.MapSubresource(stagingBuffer, MapMode.Read, MapFlags.None, out result);
                uint[] outputData = result.ReadRange<uint>(totalSize);
                Console.WriteLine("in data[640,320] : {0}, out data[640,320] : {1}",inputData[320*Width+640], outputData[320*Width+640]);
                device.ImmediateContext.UnmapSubresource(stagingBuffer, 0);
            }
        }
    } 

}