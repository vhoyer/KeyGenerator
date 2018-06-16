using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace KeyGenerator {
	class Program {
		static System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

		static readonly char[] valid = "0123456789abcdefghijklmnopqrstuvxywzABCDEFGHIJKLMNOPQRSTUVXYWZ".ToCharArray();
		static string lastKey = String.Format("{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}", valid[0]);

		static int match = 25000000;
		static double lastIndex = 0;
		static double keysFound = 0;

		static string datpath;

		//
		// runtime variables
		//
		static byte currentUsingValidIndex = 0;

		static void Main(string[] args) {
			if(args.Length == 1) {
				match = int.Parse(args[0]);
			}

			string keypath = Environment.CurrentDirectory + "/keys"+match+".txt";
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
			stopwatch.Start();
			while(lastKey != "ZZZZZZZZZZZZZZZZZZZZ") {
				lastKey = generateNextKey(lastKey);
				Console.WriteLine(lastKey + "\t: "+lastIndex + "\t" + ++keysFound);
				using(StreamWriter sw = File.AppendText(keypath)) {
					sw.Write(lastIndex+"\t"+lastKey+"\n");
				}
			}
		}

		private static string generateNextKey(string lastKey) {
			byte[] i = Array.ConvertAll(lastKey.ToCharArray(), x => (byte)x);
			i = iterateThrough(i);

			Func<int> calc = () => ((i[0] + i[1] + i[2] + i[3]) * (i[4] + i[5] + i[6] + i[7]) + (i[8] + i[9] + i[10] + i[11]) * (i[12] + i[13] + i[14] + i[15])) * (i[16] + i[17] + i[18] + i[19]);
			Func<bool> test = () => calc() == match;

			double j,maxIter = Math.Pow(256, 20) - 1;
			for(j = lastIndex + 1; j < maxIter; j++) {
				if(j % 1000000 == 0) {
					Console.Write("\n{0}\t: {1}\t{2}%\t={3}\t[{4}ms] keys:{5}", compile(i), j, (j / maxIter * 100), calc(), stopwatch.ElapsedMilliseconds, keysFound);
					stopwatch.Restart();
				}
				if(j % 10000000 == 0) {
					double progressJ = j,
						   progressFound = keysFound;
					string progressKey = compile(i);
					new Thread(() => {//saveProgress
						using(StreamWriter sw = new StreamWriter(datpath, false))
							sw.Write(progressJ + "\t" + progressKey + "\t" + progressFound + "\r\n");
						Console.Write("\t✓");
					}).Start();
				}
				if(test()) break;
				i = iterateThrough(i);
			}
			lastIndex = j;
			return compile(i);
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
	}
}
