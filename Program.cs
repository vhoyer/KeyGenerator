using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KeyGenerator {
	class Program {
		//
		// Parameters
		//
		static readonly char[] valid = "0123456789abcdefghijklmnopqrstuvxywzABCDEFGHIJKLMNOPQRSTUVXYWZ".ToCharArray();
		static string lastKey = String.Format("{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}", valid[0]);
		static int match = 25000000;
		static string datpath;
		static string keypath;
		static double step = 1000000;

		//
		// runtime variables
		//
		static byte currentUsingValidIndex = 0;
		static double lastIndex = 0;
		static double keysFound = 0;
		static void Main(string[] args) {
			if(args.Length == 1) {
				match = int.Parse(args[0]);
			}

			keypath = Environment.CurrentDirectory + "/keys"+match+".txt";
			if(!File.Exists(keypath))
				File.Create(keypath).Close();

			datpath = Environment.CurrentDirectory + "/keys"+match+".dat";
			if(!File.Exists(datpath))
				File.Create(datpath).Close();
			else
				try {
					string lastLine = File.ReadLines(datpath).Last();
					lastIndex = double.Parse(lastLine.Split('\t')[0]);
					lastKey = lastLine.Split('\t')[1];

					currentUsingValidIndex = getNextIndex((byte)lastKey[0]);
				}
				catch(InvalidOperationException) { }

			Console.WriteLine(datpath);

			generateNextKey(lastKey);
		}

		private async static void generateNextKey(string lastKey) {
			byte[] i = Array.ConvertAll(lastKey.ToCharArray(), x => (byte)x);
			i = iterateThrough(i);

			var stopwatch = new Stopwatch();

			double j,maxIter = Math.Pow(256, 20) - 1;
			for(j = lastIndex + 1; j < maxIter;) {
				stopwatch.Restart();

				var steps = new bool[10];
				for (int b = 0; b < steps.Length; b++) {
					steps[b] = execOneStep(b, i, j);
				}

				j += step * steps.Length;

                if (steps.Contains(true)) {
                    foreach (var item in keysInBatch) {
                        Console.WriteLine(item + "\t: " + ++keysFound);
                        using (StreamWriter sw = File.AppendText(keypath)) {
                            sw.Write(lastKey + "\n");
                        }
                    }
					keysInBatch.Clear();
                }

				Console.Write($"\n{(j / maxIter * 100)}%\t[{stopwatch.ElapsedMilliseconds}ms] keys:{keysFound}");
				stopwatch.Stop();

				//SaveProgress
				using(StreamWriter sw = new StreamWriter(datpath, false))
					sw.Write(j + "\t" + compile(i) + "\t" + keysFound + "\r\n");
				Console.Write("\t✓");
			}
		}

		static List<string> keysInBatch = new List<string>();

        private static bool execOneStep(double mod, byte[] i, double j) {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

			bool result = false;

			j += step*mod;
			double max = j+step;
			for (; j < max; j++) {
                if (calc(i) == match) {
                    keysInBatch.Add(compile(i));
                    result = true;
                }
				i = iterateThrough(i);
			}

            Console.Write($"\n{compile(i)}\t: {j}\t={calc(i)}\t[{stopwatch.ElapsedMilliseconds}ms]");
            stopwatch.Stop();
			return result;
		}

		private static byte[] iterateThrough(byte[] i) {
			i[0] = (byte)valid[currentUsingValidIndex > valid.Length - 1 ? 0 : currentUsingValidIndex++];
			if (i[0] == valid[0]){
				currentUsingValidIndex = 1;
				for(byte s = 1; s < i.Length;) {
					byte n = getNextIndex(i[s]);
					i[s] = (byte)valid[n > valid.Length - 1 ? 0 : n];
					if(i[s] != valid[0]) { break; }
				}
			}
			return i;
		}

		private static byte getNextIndex(byte _char){
			byte i = 0;
			for(; i < valid.Length;){
				if (valid[i++] == _char){
					break;
				}
			}
			return i;
		}

		private static string compile(byte[] i){
			return String.Join("", Array.ConvertAll(i, x => (char)x));
		}

		private static double calc(byte[] i){
			return ((i[0] + i[1] + i[2] + i[3]) * (i[4] + i[5] + i[6] + i[7]) + (i[8] + i[9] + i[10] + i[11]) * (i[12] + i[13] + i[14] + i[15])) * (i[16] + i[17] + i[18] + i[19]);
		} 
	}
}
