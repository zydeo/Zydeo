using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;

namespace ZD.Colloc
{
    public delegate void DoneDelegate();

    /// <summary>
    /// All the meat here.
    /// </summary>
    internal class Colloc
    {
        /// <summary>
        /// Word frequencies from corpus.
        /// </summary>
        private Dictionary<string, int> freqs = new Dictionary<string, int>();

        /// <summary>
        /// Corpuses total word count.
        /// </summary>
        private int corpusN = 0;

        /// <summary>
        /// Co-occurrence count of various words with current query.
        /// </summary>
        private Dictionary<string, int> coCounts = new Dictionary<string, int>();

        /// <summary>
        /// Query's occurrence count.
        /// </summary>
        private int qCount;

        /// <summary>
        /// All observations (# of messages in corpus).
        /// </summary>
        private int eventCount;

        public class Result
        {
            public readonly string Word;
            public readonly double LL;
            public readonly double ChSqCorr;
            public Result(string word, double ll, double chSqCorr)
            {
                Word = word;
                LL = ll;
                ChSqCorr = chSqCorr;
            }
        }

        public readonly List<Result> ResArr = new List<Result>();

        /// <summary>
        /// Loads word frequency data.
        /// </summary>
        /// <param name="done"></param>
        public void LoadFreqs(DoneDelegate done)
        {
            ThreadPool.QueueUserWorkItem(doLoadFreqs, done);
        }

        private void doLoadFreqs(object state)
        {
            DoneDelegate done = (DoneDelegate)state;
            using (StreamReader sr = new StreamReader("words_types.txt"))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    string[] parts = line.Split(',');
                    if (parts.Length != 4) continue;
                    int freq = int.Parse(parts[3].Trim('"'));
                    freqs[parts[1].Trim('"')] = freq;
                    corpusN += freq;
                }
            }
            done();
        }

        /// <summary>
        /// Find co-occurring words with query word.
        /// </summary>
        public void Query(string word, int minFreq, int maxFreq, DoneDelegate done, int wleft, int wright)
        {
            ThreadPool.QueueUserWorkItem(doQuery, new object[] { word, done, minFreq, maxFreq, wleft, wright });
        }

        private void doQuery(object state)
        {
            object[] args = (object[])state;
            DoneDelegate done = (DoneDelegate)args[1];
            string word = (string)args[0];
            int minFreq = (int)args[2];
            if (minFreq <= 0) minFreq = int.MaxValue;
            int maxFreq = (int)args[3];
            if (maxFreq <= 0) maxFreq = int.MaxValue;
            int wleft = (int)args[4];
            if (wleft <= 0) wleft = int.MaxValue;
            int wright = (int)args[5];
            if (wright <= 0) wright = int.MaxValue;

            coCounts.Clear();
            qCount = 0;
            eventCount = 0;
            ResArr.Clear();
            using (StreamReader sr = new StreamReader("segm.txt"))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    ++eventCount;
                    string[] parts = line.Split(' ');
                    int wPos = -1;
                    for (int i = 0; i != parts.Length; ++i)
                        if (parts[i] == word) { wPos = i; break; }
                    if (wPos == -1) continue;
                    ++qCount;

                    for (int i = wPos + 1; i < parts.Length && i - wPos <= wright; ++i)
                    {
                        string str = parts[i];
                        if (str == word) continue;
                        if (!freqs.ContainsKey(str)) continue;
                        int freq = freqs[str];
                        if (freq < minFreq || freq > maxFreq) continue;
                        if (!coCounts.ContainsKey(str)) coCounts[str] = 1;
                        else ++coCounts[str];
                    }

                    for (int i = wPos - 1; i >= 0 && wPos - i >= wleft; --i)
                    {
                        string str = parts[i];
                        if (str == word) continue;
                        if (!freqs.ContainsKey(str)) continue;
                        int freq = freqs[str];
                        if (freq < minFreq || freq > maxFreq) continue;
                        if (!coCounts.ContainsKey(str)) coCounts[str] = 1;
                        else ++coCounts[str];
                    }
                }
            }

            foreach (var x in coCounts)
            {
                Result res = new Result(x.Key, getLL(x.Key, x.Value), getChSqCorr(x.Key, x.Value));
                ResArr.Add(res);
            }
            //Array.Sort(resArr, (x, y) => y.LL.CompareTo(x.LL));
            //Array.Sort(resArr, (x, y) => y.ChSqCorr.CompareTo(x.ChSqCorr));

            done();
        }

        private double getChSqCorr(string word, int coCount)
        {
            int wordFreq = freqs[word];
            double o11 = coCount;
            double o12 = wordFreq - coCount;
            double o21 = qCount - coCount;
            double o22 = eventCount - wordFreq - qCount + coCount;
            double dEventCount = eventCount;
            double e11 = (o11 + o12) * (o11 + o21) / dEventCount;
            double e12 = (o12 + o11) * (o12 + o22) / dEventCount;
            double e21 = (o21 + o11) * (o21 + o22) / dEventCount;
            double e22 = (o22 + o12) * (o22 + o21) / dEventCount;
            double res = dEventCount * Math.Pow(Math.Abs(o11 * o22 - o12 * o21) - dEventCount / 2.0D, 2.0D) /
                ((o11 + o12) * (o21 + o22) * (o11 + o21) * (o12 + o22));
            return res;
        }

        private double getLL(string word, int coCount)
        {
            int wordFreq = freqs[word];
            double o11 = coCount;
            double o12 = wordFreq - coCount;
            double o21 = qCount - coCount;
            double o22 = eventCount - wordFreq - qCount + coCount;
            double dEventCount = eventCount;
            double e11 = (o11 + o12) * (o11 + o21) / dEventCount;
            double e12 = (o12 + o11) * (o12 + o22) / dEventCount;
            double e21 = (o21 + o11) * (o21 + o22) / dEventCount;
            double e22 = (o22 + o12) * (o22 + o21) / dEventCount;
            double ll = 2.0D * (
                o11 * (o11 == 0 ? 1D : Math.Log(o11 / e11)) +
                o12 * (o12 == 0 ? 1D : Math.Log(o12 / e12)) +
                o21 * (o21 == 0 ? 1D : Math.Log(o21 / e21)) +
                o22 * (o22 == 0 ? 1D : Math.Log(o22 / e22)));
            return ll;
        }
    }
}
