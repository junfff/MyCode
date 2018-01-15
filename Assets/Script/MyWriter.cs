
using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Assets.Script
{

    public class MyPtr
    {
        public IntPtr myP;
        public int MyLength;
    }
    public class MyWriter
    {
        [DllImport("MyWriteHandler")]
        private static extern int SaveBuf(IntPtr buf, int length);
        [DllImport("MyWriteHandler")]
        private static extern int GetBuf(out IntPtr unmanaged_ptr);
        [DllImport("MyWriteHandler")]
        private static extern int Add(int x, int y);

        private const string path = @"C:\LJF\MyTestWriter.txt";

        private FileInfo finfo;

        private byte[] content;

        private FileInfo file_info;
        internal void OnStart()
        {
            file_info = new FileInfo(path);

            string str = "今天星期天啊,等下吃饭了今天星期天啊,等下吃饭了今天星期天啊,等下吃饭了 !!";
            content = System.Text.Encoding.UTF8.GetBytes(str);
            OnWrite();
        }

        private int writeNum = 2;
        private int curWrite;

        List<byte[]> list = new List<byte[]>();

        bool finish;
        bool saveFinish;
        internal void OnUpdate()
        {
            //TestAAAA();
            Save4096();
        }

        private void Save4096()
        {
            content = new byte[40960];

            IntPtr unmanaged_data_prt = Marshal.AllocHGlobal(content.Length);// 直接分配100 byte的内存
            Marshal.Copy(content, 0, unmanaged_data_prt, content.Length);

            MyPtr myP = new MyPtr();
            myP.myP = unmanaged_data_prt;
            myP.MyLength = content.Length;
            listPtr.Add(myP);
            //Marshal.FreeHGlobal(unmanaged_data_prt); //使用后销毁非托管内存


            //GCHandle unmanaged_data_handle = GCHandle.Alloc(content, GCHandleType.Pinned); //这里将标记_managed_data暂时不能被gc回收，并且固定对象的地址
            //int ret = SaveBuf(unmanaged_data_handle.AddrOfPinnedObject(), content.Length);//这里将拿到非托管内存的固定地址，传给c++
            //unmanaged_data_handle.Free();//使用完毕后，将其handle free，这样c#可以正常gc这块内存

            curWrite++;
            UnityEngine.Debug.LogFormat("Savebuff curWrite = {0}  ret = {1}", curWrite, listPtr);
        }

        private void TestAAAA()
        {
            if (finish)
            {
                UnityEngine.Debug.LogFormat("save and write done !!");
                return;
            }
            if (curWrite >= 100)
            {
                saveFinish = true;
                while (true)
                {
                    int ret = UpWrite();
                    if (ret == -1)
                    {
                        finish = true;
                        return;
                    }
                }
            }
            if (saveFinish)
            {
                return;
            }
            Savebuff();
        }

        List<MyPtr> listPtr = new List<MyPtr>();
        private void Savebuff()
        {
            //content = new byte[40960];

            string str = "今天星期天啊,等下吃饭了今天星期天啊,等下吃饭了今天星期天啊,等下吃饭了 !!";
            str = string.Format("{0}, num = {1}", str, curWrite);
            content = System.Text.Encoding.UTF8.GetBytes(str);

            IntPtr unmanaged_data_prt = Marshal.AllocHGlobal(content.Length);// 直接分配100 byte的内存
            Marshal.Copy(content, 0, unmanaged_data_prt, content.Length);

            MyPtr myP = new MyPtr();
            myP.myP = unmanaged_data_prt;
            myP.MyLength = content.Length;
            listPtr.Add(myP);
            //Marshal.FreeHGlobal(unmanaged_data_prt); //使用后销毁非托管内存


            //GCHandle unmanaged_data_handle = GCHandle.Alloc(content, GCHandleType.Pinned); //这里将标记_managed_data暂时不能被gc回收，并且固定对象的地址
            //int ret = SaveBuf(unmanaged_data_handle.AddrOfPinnedObject(), content.Length);//这里将拿到非托管内存的固定地址，传给c++
            //unmanaged_data_handle.Free();//使用完毕后，将其handle free，这样c#可以正常gc这块内存

            curWrite++;
            UnityEngine.Debug.LogFormat("Savebuff curWrite = {0}  ret = {1}", curWrite, listPtr);
        }

        private int UpWrite()
        {
            IntPtr unmanaged_ptr = IntPtr.Zero; //定义这个c#中用来接收c++返回数据的指针类型
            int length = GetPtr(ref unmanaged_ptr);//调用c++的函数，使unmanaged_ptr指向c++里分配的内存，注意这里用out ，才能与c++里面的**匹配。
            if (length == -1)
            {
                return -1;
            }
            byte[] managed_data = new byte[length];
            Marshal.Copy(unmanaged_ptr, managed_data, 0, length);//将非托管内存拷贝成托管内存，才能在c#里面使用
            Marshal.FreeHGlobal(unmanaged_ptr);//释放非托管的内存


      
            WriterFile(managed_data);
            curWrite--;
            UnityEngine.Debug.LogFormat("UpWrite curWrite = {0}  retr = {1}", curWrite, length);

            return 0;
        }

        private int GetPtr(ref IntPtr unmanaged_ptr)
        {
            int length = 0;
            if (listPtr.Count <= 0)
            {
                return -1;
            }
            unmanaged_ptr = listPtr[0].myP;
            length = listPtr[0].MyLength;
            listPtr.RemoveAt(0);
            return length;
        }

        private void OnWrite()
        {
            testB();

            //testA();

        }

        private void testA()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();


            while (true)
            {
                if (curWrite >= writeNum)
                {
                    break;
                }

                WriterFile(content);//3394.8485
                curWrite++;
            }


            sw.Stop();
            TimeSpan swTime = sw.Elapsed;

            UnityEngine.Debug.LogFormat("TotalMilliseconds = {0}", swTime.TotalMilliseconds);
        }

        private void testB()
        {
            Stopwatch swB = new Stopwatch();
            swB.Start();


            while (true)
            {
                if (curWrite >= writeNum)
                {
                    break;
                }

                WriterFileB(content);//3394.8485
                curWrite++;
            }


            swB.Stop();
            TimeSpan swTimeB = swB.Elapsed;

            UnityEngine.Debug.LogFormat("TotalMilliseconds BBB = {0}", swTimeB.TotalMilliseconds);
        }

        private void WriterFileB(byte[] content)
        {
            using (FileStream fs = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Write))
            {
                fs.Write(content, 0, content.Length);
            }

            //UnityEngine.Debug.Log("Write done BBB ！");

        }
        private void WriterFile(byte[] content)
        {
            FileStream sw;

            if (!file_info.Exists)
            {
                sw = file_info.Create();//创建一个用于写入 UTF-8 编码的文本  
                //UnityEngine.Debug.Log("文件创建成功！");
            }
            else
            {
                sw = file_info.Open(FileMode.Append, FileAccess.Write, FileShare.Write);//打开现有 UTF-8 编码文本文件以进行读取  
                //UnityEngine.Debug.Log("OpenWrite！");
            }
            sw.Write(content, 0, content.Length);
            sw.Close();
            sw.Dispose();//文件流释放  

            //UnityEngine.Debug.Log("Write done ！");

        }



    }
}
