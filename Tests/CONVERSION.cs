using System;
using System.Diagnostics;
using System.IO;
using Commons;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class CONVERSION
    {
		private string root = @"E:\Projects\Radinoiu\mxf2dash\Tests\Files\Repo\MXF_Samples\";

		[TestMethod]
		public void FFMPEGFileConversion()
		{
			string input = root + "sample_3840x2160.mxf";
			string output = root + @"\out\FFMPEG.mpd";
			string outputDIR = Path.GetDirectoryName(output);
			string cmd = "ffmpeg -re -i {in} -pix_fmt yuv420p -vsync 1 " +
				 "-map 0:v:0 -map 0:a:0{audio} -c:a aac -c:v libx264 -use_template 1 " +
				 "-use_timeline 1 -init_seg_name  \"init-stream$RepresentationID$-$Bandwidth$.mp4\"" +
				 " -media_seg_name \"chunk-stream$RepresentationID$-$Number%05d$.$ext$\" -b:v 1500k -b:a 128k -ac 2 " +
				 "-profile:v main -level:v 3.0 -s {resolution} -r 25 -vsync passthrough -increment_tc 1 -adaptation_sets " +
				 "\"id=0,streams=v id=1,streams=a\" -g 100 -keyint_min 100 -seg_duration 5 -frag_duration 5 " +
                 "-dash_segment_type auto -f dash {out}";


			Process process = ProcessFactory.CreateProcess(cmd, input, output, outputDIR);
			process.Start();
			process.WaitForExit();
			Assert.AreEqual(0, process.ExitCode);
		}

		[TestMethod]
		public void AudioProbeFalse()
		{
			string input = root + "sample_1920x1080.mxf";
			string result = ProcessFactory.Probe(ProcessFactory.AudioProbeString, input);
			Assert.AreEqual("?", result);
		}

		[TestMethod]
		public void ResolutionProbe1920x1080()
		{
			string input = root + "sample_1920x1080.mxf";

			string result = ProcessFactory.Probe(ProcessFactory.DimensionProbeString, input);

			Assert.AreEqual("1920x1080", result);

		}

		[TestMethod]
		public void ResolutionProbe3840x2160()
		{
			string input = root + "sample_3840x2160.mxf";

			string result = ProcessFactory.Probe(ProcessFactory.DimensionProbeString, input);

			Assert.AreEqual("3840x2160", result);

		}

		[TestMethod]
		public void AudioProbeTrue()
		{
			string input = @"E:\Torrents\The.Walking.Dead.S10.1080p.AMZN.WEB-DL.DD+5.1.H.264-CasStudio";

			string result = ProcessFactory.Probe(ProcessFactory.AudioProbeString, input);

			Assert.AreEqual("", result);
		}
	}
}
