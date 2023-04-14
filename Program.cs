using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TISUnityPluginDevHelper
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"参数数量{args.Length}");
            if (args.Length == 0)
            {
                Console.WriteLine("请使用命令行打开该程序，使用help参数来获取命令列表");
                Console.ReadKey();
                return;
            }

            //懒得做命令支持框架，随便塞了
            if (args[0] == "help")
            {
                Console.WriteLine("moveAll [UnityProjectName] 移动所有已生成的插件到Unity文件夹");
            }

            if (args[0] == "moveAll")
            {
                Console.WriteLine("moveAll 移动所有已生成的插件到Unity文件夹");
                MoveAll();
            }

            Console.ReadKey();
        }


        const string PLUGIN_DEVS = "PluginDevs";
        static void MoveAll()
        {
            var exePath = Environment.CurrentDirectory;
            Console.WriteLine(exePath);
            var directoryInfo = new DirectoryInfo(exePath);
            DirectoryInfo unityRoot;
            DirectoryInfo pluginRoot;
            while (true)
            {
                var subDirectories = directoryInfo.GetDirectories();
                if (subDirectories.Length != 2)
                {
                    if (directoryInfo.Parent != null)
                    {
                        directoryInfo = directoryInfo.Parent;
                    }
                    else
                    {
                        //已经搜索到磁盘根目录
                        Console.WriteLine("错误：未找到目标目录！");
                        return;
                    }
                }
                else
                {
                    pluginRoot = subDirectories.FirstOrDefault((sd) => sd.Name == PLUGIN_DEVS);

                    if (pluginRoot == null)
                    {
                        continue;
                    }

                    unityRoot = subDirectories.FirstOrDefault((sd) => sd.Name != PLUGIN_DEVS);

                    if (unityRoot == null)
                    {
                        Console.WriteLine($"错误：未找到Unity目录！请确定插件文件夹和Unity文件夹在同一级目录下");
                        break;
                    }
                    Console.WriteLine("找到Unity目录和插件目录了！");
                    break;
                }
            }

            var assetDirectories = unityRoot.GetDirectories("Assets");
            if (unityRoot.GetDirectories("Assets").Length == 0)
            {
                Console.WriteLine("错误：未找到Assets目录，可能不是Unity工程！");
                return;
            }

            var unityAssetDirectory = assetDirectories[0];
            var unityPluginDirectories = unityAssetDirectory.GetDirectories("Plugins");
            DirectoryInfo unityPluginDirectory;
            if (unityPluginDirectories.Length == 0)
            {
                Console.WriteLine("未找到Plugins目录，生成中！");
                unityPluginDirectory = unityAssetDirectory.CreateSubdirectory("Plugins");
            }
            else
            {
                unityPluginDirectory = unityPluginDirectories[0];
            }

            foreach (var pluginDir in pluginRoot.GetDirectories())
            {
                Console.WriteLine($"===尝试复制{pluginDir.Name}===");
                CopyPlugin(unityPluginDirectory, pluginDir, pluginDir.Name);
                Console.WriteLine($"===复制结束===");
            }
        }

        static void CopyPlugin(DirectoryInfo unityPluginDirectory, DirectoryInfo pluginRootDirectory, string pluginName)
        {
            var subPath = $"src/{pluginName}/bin/Release/";
            if (!Directory.Exists(Path.Combine(pluginRootDirectory.FullName, subPath)))
            {
                Console.WriteLine($"插件{pluginName}还没有生成");
                return;
            }
            var buildTargets = pluginRootDirectory.GetDirectories(subPath);
            if (buildTargets.Length == 0)
            {
                Console.WriteLine($"未找到插件{pluginName}");
                Console.WriteLine($"请检查路径是否为{PLUGIN_DEVS}/{pluginName}/src/{pluginName}/bin/Release");
                return;
            }

            var standardBuild = buildTargets.FirstOrDefault((bt) => bt.Name == "netstandard2.1");
            if (standardBuild != null)
            {
                Console.WriteLine("找到了.net standard 2.1版本的发布版");
                CopyDirectory(standardBuild.FullName, Path.Combine(unityPluginDirectory.FullName, pluginName));
                return;
            }

            var frameworkBuild = buildTargets.FirstOrDefault((bt) => bt.Name == "netframework4.7.1");
            if (frameworkBuild != null)
            {
                Console.WriteLine("暂时不支持 framework 的移植哦");
                return;
            }

            Console.WriteLine("未找到.net standard2.1或者.net framwork 4.7.1的release版本");
            return;
        }

        static void CopyDirectory(string srcPath, string destPath)
        {
            if (!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
            }
            DirectoryInfo dir = new DirectoryInfo(srcPath);
            FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();
            foreach (FileSystemInfo i in fileinfo)
            {
                if (i is DirectoryInfo)
                {
                    if (!Directory.Exists(destPath + "\\" + i.Name))
                    {
                        Directory.CreateDirectory(destPath + "\\" + i.Name);
                    }
                    CopyDirectory(i.FullName, destPath + "\\" + i.Name);
                }
                else
                {
                    File.Copy(i.FullName, destPath + "\\" + i.Name, true);
                }
            }
        }
    }
}
