using System;
using System.Linq;
using NUnit.Framework;
using System.Collections.Generic;
using OrigoDB.Core.Compression;
using System.Diagnostics;
using System.Text;

namespace OrigoDB.Core.Test
{
    [TestFixture]
    public class CompressorTests
    {
        List<string> results = new List<string>();
        
        [TestFixtureTearDown]
        public void PrintResults()
        {
            foreach (var result in results)
            {
                Console.WriteLine(result);
            }
        }

        [Test, TestCaseSource("TestCases")]
        public void Compress_decompression_yields_original_data(ICompressor compressor)
        {
            var randomBytes = new byte[2000];
            new Randomizer().NextBytes(randomBytes);
            var textBytes = Encoding.UTF8.GetBytes(testData);
            foreach (var array in new[]{randomBytes,textBytes})
            {
                var resurrected = compressor.Decompress(compressor.Compress(array));
                CollectionAssert.AreEqual(array, resurrected);
            }
        }

        [Test, Ignore, TestCaseSource("TestCases")]
        public void Performance(ICompressor compressor)
        {
            results.Add("---------" + compressor.GetType() + "---------");
            byte[] indata = Encoding.UTF8.GetBytes(testData);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            byte[] result = null;
            foreach (var i in Enumerable.Range(0,100))
            {
                result = compressor.Compress(indata);
            }
            stopwatch.Stop();
            long ticks = stopwatch.ElapsedTicks;
            results.Add("Compress: " +  ticks);

            stopwatch.Reset();
            stopwatch.Start();

            byte[] resurrected;
            foreach (var i in Enumerable.Range(0, 100))
            {
                resurrected = compressor.Decompress(result);
            }
            stopwatch.Stop();
            ticks = stopwatch.ElapsedTicks;
            results.Add("Decompress: " + ticks);
            results.Add("Compression: " + result.Length / (1.0 * indata.Length));
        }


        public IEnumerable<ICompressor> TestCases()
        {
            yield return new DeflateStreamCompressor();
            yield return new GzipCompressor();
            //yield return new LzfCompressionAdapter();
        }

        private const string testData = @"Now that there is the Tec-9, a crappy spray gun from South Miami. This gun is advertised as the most popular gun in American crime. Do you believe that shit? It actually says that in the little book that comes with it: the most popular gun in American crime. Like they're actually proud of that shit. 

Well, the way they make shows is, they make one show. That show's called a pilot. Then they show that show to the people who make shows, and on the strength of that one show they decide if they're going to make more shows. Some pilots get picked and become television programs. Some don't, become nothing. She starred in one of the ones that became nothing.

Look, just because I don't be givin' no man a foot massage don't make it right for Marsellus to throw Antwone into a glass motherfuckin' house, fuckin' up the way the nigger talks. Motherfucker do that shit to me, he better paralyze my ass, 'cause I'll kill the motherfucker, know what I'm sayin'?

The path of the righteous man is beset on all sides by the iniquities of the selfish and the tyranny of evil men. Blessed is he who, in the name of charity and good will, shepherds the weak through the valley of darkness, for he is truly his brother's keeper and the finder of lost children. And I will strike down upon thee with great vengeance and furious anger those who would attempt to poison and destroy My brothers. And you will know My name is the Lord when I lay My vengeance upon thee.

Look, just because I don't be givin' no man a foot massage don't make it right for Marsellus to throw Antwone into a glass motherfuckin' house, fuckin' up the way the nigger talks. Motherfucker do that shit to me, he better paralyze my ass, 'cause I'll kill the motherfucker, know what I'm sayin'?

Now that we know who you are, I know who I am. I'm not a mistake! It all makes sense! In a comic, you know how you can tell who the arch-villain's going to be? He's the exact opposite of the hero. And most times they're friends, like you and me! I should've known way back when... You know why, David? Because of the kids. They called me Mr Glass.

Look, just because I don't be givin' no man a foot massage don't make it right for Marsellus to throw Antwone into a glass motherfuckin' house, fuckin' up the way the nigger talks. Motherfucker do that shit to me, he better paralyze my ass, 'cause I'll kill the motherfucker, know what I'm sayin'?

Well, the way they make shows is, they make one show. That show's called a pilot. Then they show that show to the people who make shows, and on the strength of that one show they decide if they're going to make more shows. Some pilots get picked and become television programs. Some don't, become nothing. She starred in one of the ones that became nothing.

Normally, both your asses would be dead as fucking fried chicken, but you happen to pull this shit while I'm in a transitional period so I don't wanna kill you, I wanna help you. But I can't give you this case, it don't belong to me. Besides, I've already been through too much shit this morning over this case to hand it over to your dumb ass.

Now that there is the Tec-9, a crappy spray gun from South Miami. This gun is advertised as the most popular gun in American crime. Do you believe that shit? It actually says that in the little book that comes with it: the most popular gun in American crime. Like they're actually proud of that shit. 
";

    }
}