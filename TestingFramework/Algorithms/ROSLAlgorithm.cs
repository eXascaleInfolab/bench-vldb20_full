﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using TestingFramework.Testing;

namespace TestingFramework.Algorithms
{
    public partial class ROSLAlgorithm : Algorithm
    {
        private static bool _init = false;
        public ROSLAlgorithm() : base(ref _init)
        { }

        public override string[] EnumerateInputFiles(string dataCode, int tcase)
        {
            return new[] { $"{dataCode}_m{tcase}.txt" };
        }
        
        private static string Style => "linespoints lt 8 dt 2 lw 3 pt 7 lc rgbcolor \"dark-red\" pointsize 3";

        public override IEnumerable<SubAlgorithm> EnumerateSubAlgorithms()
        {
            return new[] { new SubAlgorithm($"{AlgCode}", String.Empty, Style) };
        }

        public override IEnumerable<SubAlgorithm> EnumerateSubAlgorithms(int tcase)
        {
            return new[] { new SubAlgorithm($"{AlgCode}", $"{AlgCode}{tcase}", Style) };
        }
        
        protected override void PrecisionExperiment(ExperimentType et, ExperimentScenario es,
            DataDescription data, int tcase)
        {
            RunROSL(GetROSLProcess(data, tcase));
        }
        
        private Process GetROSLProcess(DataDescription data, int len)
        {
            Process roslproc = new Process();
            
            roslproc.StartInfo.WorkingDirectory = EnvPath;
            roslproc.StartInfo.FileName = EnvPath + "../cmake-build-debug/algoCollection";
            roslproc.StartInfo.CreateNoWindow = true;
            roslproc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            roslproc.StartInfo.UseShellExecute = false;

            roslproc.StartInfo.Arguments = $"-alg rosl -test o -n {data.N} -m {data.M} -k {AlgoPack.TypicalTruncation} " +
                                         $"-in ./{SubFolderDataIn}{data.Code}_m{len}.txt " +
                                         $"-out ./{SubFolderDataOut}{AlgCode}{len}.txt";

            return roslproc;
        }
        
        private Process GetRuntimeROSLProcess(DataDescription data, int len)
        {
            Process roslproc = new Process();
            
            roslproc.StartInfo.WorkingDirectory = EnvPath;
            roslproc.StartInfo.FileName = EnvPath + "../cmake-build-debug/algoCollection";
            roslproc.StartInfo.CreateNoWindow = true;
            roslproc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            roslproc.StartInfo.UseShellExecute = false;

            roslproc.StartInfo.Arguments = $"-alg rosl -test rt -n {data.N} -m {data.M} -k {AlgoPack.TypicalTruncation} " +
                                             $"-in ./{SubFolderDataIn}{data.Code}_m{len}.txt " +
                                             $"-out ./{SubFolderDataOut}{AlgCode}{len}.txt";

            return roslproc;
        }
        private void RunROSL(Process roslproc)
        {
            roslproc.Start();
            roslproc.WaitForExit();
                
            if (roslproc.ExitCode != 0)
            {
                string errText =
                    $"[WARNING] ROSL returned code {roslproc.ExitCode} on exit.{Environment.NewLine}" +
                    $"CLI args: {roslproc.StartInfo.Arguments}";
                
                Console.WriteLine(errText);
                Utils.DelayedWarnings.Enqueue(errText);
            }
        }

        protected override void RuntimeExperiment(ExperimentType et, ExperimentScenario es, DataDescription data,
            int tcase)
        {
            RunROSL(GetRuntimeROSLProcess(data, tcase));
        }

        public override void GenerateData(string sourceFile, string code, int tcase, (int, int, int)[] missingBlocks,
            (int, int) rowRange, (int, int) columnRange)
        {
            sourceFile = DataWorks.FolderData + sourceFile;
            
            (int rFrom, int rTo) = rowRange;
            (int cFrom, int cTo) = columnRange;
            
            double[][] res = DataWorks.GetDataLimited(sourceFile, rTo - rFrom, cTo - cFrom);
            
            int n = rTo > res.Length ? res.Length : rTo;
            int m = cTo > res[0].Length ? res[0].Length : cTo;
            
            var data = new StringBuilder();

            for (int i = rFrom; i < n; i++)
            {
                string line = "";

                for (int j = cFrom; j < m; j++)
                {
                    if (Utils.IsMissing(missingBlocks, i, j))
                    {
                        line += "NaN" + " ";
                    }
                    else
                    {
                        line += res[i][j] + " ";
                    }
                }
                data.Append(line.Trim() + Environment.NewLine);
            }

            string destination = EnvPath + SubFolderDataIn + $"{code}_m{tcase}.txt";
            
            if (File.Exists(destination)) File.Delete(destination);
            File.AppendAllText(destination, data.ToString());
        }
    }
}