using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System;

public unsafe class Mp4Encoder {
	private Process ffmpeg;
	private Stream inputStream;
	//private int width;
	private int height;
	//private string outputPath;
	//private int fps;

	public bool Start(int width, int height, string outputPath, int fps) {
		//this.width = width;
		this.height = height;
		//this.outputPath = outputPath;
		//this.fps = fps;

		string ffmpegPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg.exe");

		ffmpeg = new Process {
			StartInfo =
			{
				FileName = ffmpegPath,
				Arguments = $"-y -f rawvideo -pix_fmt bgr24 -s {width}x{height} -r {fps} -i pipe: -c:v libx264 -pix_fmt yuv420p {outputPath}",
				UseShellExecute = false,
				RedirectStandardInput = true,
				RedirectStandardError = true,
				//CreateNoWindow = true
			}
		};
		if (!ffmpeg.Start())
			return false;
		inputStream = ffmpeg.StandardInput.BaseStream;
		return true;
	}

	public bool AddFrame(byte* data, int stride) {
		int length = stride * height;
		byte[] buffer = new byte[length];
		Marshal.Copy((IntPtr)data, buffer, 0, length);

		try {
			inputStream.Write(buffer, 0, length);
			inputStream.Flush();
			return true;
		} catch (IOException ex) {
			Console.WriteLine("IOException: " + ex.Message);
			return false;
		}
	}
	public bool Finish() {
		try {
			inputStream.Flush();
			inputStream.Close();
			ffmpeg.WaitForExit();
			ffmpeg.Dispose();
			return true;
		} catch (IOException ex) {
			Console.WriteLine("IOException: " + ex.Message);
			return false;
		}
	}
}

/*using RgbFractalGenCs.AutoGen;
using RgbFractalGenCs.AutoGen.Abstractions;
using RgbFractalGenCs.AutoGen.Bindings.DynamicallyLoaded;
using System;
//using System.Runtime.InteropServices;

public unsafe class Mp4Encoder {
	private AVFormatContext* formatContext;
	private SwsContext* swsContext = null;
	private AVCodecContext* codecContext;
	private AVStream* stream;
	private int frameIndex = 0;

	public bool Start(int width, int height, string outputPath, int fps) {

		FFmpegBinariesHelper.RegisterFFmpegBinaries();

		DynamicallyLoadedBindings.Initialize();

		try {
			ffmpeg.avformat_network_init();
			ffmpeg.avdevice_register_all();
		} catch (Exception ex) {
			Console.WriteLine($"Exception: {ex.Message}");
			Console.WriteLine($"Inner Exception: {ex.InnerException}");
			Console.WriteLine($"Stack Trace: {ex.StackTrace}");
		}

		// Allocate format context
		AVFormatContext* tempFormatContext = null;
		if (ffmpeg.avformat_alloc_output_context2(&tempFormatContext, null, null, outputPath) < 0) {
			throw new Exception("Failed to allocate format context.");
		}
		// Assign it here
		formatContext = tempFormatContext; 

		// Find H.264 encoder
		AVCodec* codec;
		codec = ffmpeg.avcodec_find_encoder(AVCodecID.AV_CODEC_ID_H264);
		if (codec == null) {
			Console.WriteLine("H.264 encoder not found.");
			return false;
		}

		codecContext = ffmpeg.avcodec_alloc_context3(codec);
		if (codecContext == null) {
			Console.WriteLine("Failed to allocate codec context.");
			return false;
		}

		codecContext->width = width;
		codecContext->height = height;
		codecContext->pix_fmt = AVPixelFormat.AV_PIX_FMT_YUV420P;
		codecContext->time_base = new AVRational { num = 1, den = fps };
		codecContext->framerate = new AVRational { num = fps, den = 1 };
		codecContext->bit_rate = 4000000;
		codecContext->gop_size = 10;
		codecContext->max_b_frames = 1;

		if (ffmpeg.avcodec_open2(codecContext, codec, null) < 0) {
			Console.WriteLine("Failed to open codec.");
			return false;
		}

		// Create output stream
		stream = ffmpeg.avformat_new_stream(formatContext, codec);
		if (stream == null) {
			Console.WriteLine("Failed to create output stream.");
			return false;
		}

		stream->codecpar->codec_id = AVCodecID.AV_CODEC_ID_H264;
		stream->codecpar->codec_type = AVMediaType.AVMEDIA_TYPE_VIDEO;
		stream->time_base = codecContext->time_base;

		if (ffmpeg.avcodec_parameters_from_context(stream->codecpar, codecContext) < 0) {
			Console.WriteLine("Failed to copy codec parameters.");
			return false;
		}

		if (ffmpeg.avio_open(&formatContext->pb, outputPath, ffmpeg.AVIO_FLAG_WRITE) < 0) {
			Console.WriteLine($"Failed to open output file: {outputPath}");
			return false;
		}

		if (ffmpeg.avformat_write_header(formatContext, null) < 0) {
			Console.WriteLine("Failed to write header.");
			return false;
		}

		Console.WriteLine($"MP4 encoding started: {outputPath}");
		return true;
	}

	public void AddFrame(byte* data, int stride) {
		AVFrame* frame;
		frame = ffmpeg.av_frame_alloc();
		if (frame == null) {
			Console.WriteLine("Failed to allocate frame.");
			return;
		}

		frame->format = (int)AVPixelFormat.AV_PIX_FMT_YUV420P;
		frame->width = codecContext->width;
		frame->height = codecContext->height;
		frame->pts = frameIndex++;

		// Allocate frame buffer
		if (ffmpeg.av_frame_get_buffer(frame, 32) < 0) {
			Console.WriteLine("Failed to allocate frame buffer.");
			return;
		}

		// Convert BGR to YUV420 (Assuming you already have ConvertToYUV420 function)
		ConvertToYUV420(data, stride, frame);

		// Encode the frame
		int ret = ffmpeg.avcodec_send_frame(codecContext, frame);
		if (ret < 0) {
			Console.WriteLine($"Error sending frame to encoder: {ret}");
			return;
		}

		AVPacket* pkt;
		pkt = ffmpeg.av_packet_alloc();
		if (pkt == null) {
			Console.WriteLine("Failed to allocate packet.");
			return;
		}

		while (ret >= 0) {
			ret = ffmpeg.avcodec_receive_packet(codecContext, pkt);
			if (ret == ffmpeg.AVERROR(ffmpeg.EAGAIN) || ret == ffmpeg.AVERROR_EOF)
				break;
			else if (ret < 0) {
				Console.WriteLine("Error encoding frame.");
				return;
			}

			pkt->stream_index = stream->index;
			pkt->pts = frameIndex;
			pkt->dts = frameIndex;

			ret = ffmpeg.av_interleaved_write_frame(formatContext, pkt);
			if (ret < 0) {
				Console.WriteLine("Error writing frame.");
				return;
			}
		}

		ffmpeg.av_packet_free(&pkt);
		ffmpeg.av_frame_free(&frame);
	}

	public bool Finish() {
		Console.WriteLine("Flushing encoder...");

		int ret;
		do {
			AVPacket* pkt;
			pkt = ffmpeg.av_packet_alloc();
			ret = ffmpeg.avcodec_receive_packet(codecContext, pkt);
			if (ret == ffmpeg.AVERROR(ffmpeg.EAGAIN) || ret == ffmpeg.AVERROR_EOF)
				break;
			else if (ret < 0) {
				Console.WriteLine("Error flushing encoder.");
				return false;
			}

			ret = ffmpeg.av_interleaved_write_frame(formatContext, pkt);
			ffmpeg.av_packet_free(&pkt);
		} while (ret >= 0);

		if (ffmpeg.av_write_trailer(formatContext) < 0) {
			Console.WriteLine("Error writing trailer.");
			return false;
		}

		if (ffmpeg.avio_close(formatContext->pb) < 0) {
			Console.WriteLine("Error closing file.");
			return false;
		}

		fixed (AVCodecContext** codecContextPtr = &codecContext) {
			ffmpeg.avcodec_free_context(codecContextPtr);
		}

		ffmpeg.avformat_free_context(formatContext);

		if (swsContext != null) {
			ffmpeg.sws_freeContext(swsContext);
			swsContext = null;
		}

		Console.WriteLine("MP4 encoding finished.");
		return true;
	}

	private void ConvertToYUV420(byte* data, int stride, AVFrame* frame) {
		// Initialize SwsContext only once
		if (swsContext == null) {
			swsContext = ffmpeg.sws_getContext(
				frame->width, frame->height, AVPixelFormat.AV_PIX_FMT_BGR24, // Input format: BGR24
				frame->width, frame->height, AVPixelFormat.AV_PIX_FMT_YUV420P, // Output format: YUV420P
				ffmpeg.SWS_BILINEAR, null, null, null
			);

			if (swsContext == null) {
				throw new Exception("Failed to initialize swsContext for format conversion.");
			}
		}

		// Source pointers (BGR input)
		byte*[] srcData = [data, null, null];  // Only the first plane is needed for BGR
		int[] srcStride = [stride, 0, 0];        // Only first stride is needed

		// Destination pointers (Y, U, V planes)
		byte*[] dstData = [frame->data[0], frame->data[1], frame->data[2]];
		int[] dstStride = [frame->linesize[0], frame->linesize[1], frame->linesize[2]];

		// Call sws_scale directly with arrays
		int result = ffmpeg.sws_scale(swsContext, srcData, srcStride, 0, frame->height, dstData, dstStride);
		if (result <= 0) {
			throw new Exception("sws_scale failed during BGR to YUV420P conversion.");
		}
	}*/

//using FFmpeg.AutoGen;
//using System;
//using System.Runtime.CompilerServices;

/*namespace RgbFractalGenCs;
public unsafe class Mp4Encoder {
	public bool Start(int width, int height, string outputPath, int fps) { return false; }
	unsafe public bool AddFrame(byte* data, int stride) { return false; }
	public bool Finish() { return false; }

		/*private readonly AVFormatContext* formatContext;
		private AVCodecContext* codecContext;
		private AVStream* stream;
		private int frameIndex = 0;

		static Mp4Encoder() {
			ffmpeg.RootPath = AppDomain.CurrentDomain.BaseDirectory;
			ffmpeg.avformat_network_init();
			ffmpeg.avdevice_register_all();
		}
		public bool Start(int width, int height, string outputPath, int fps) {
			unsafe {
				fixed (AVFormatContext** formatContextPtr = &formatContext) {
					if (ffmpeg.avformat_alloc_output_context2(formatContextPtr, null, null, outputPath) < 0) {
						throw new Exception("Failed to allocate format context.");
					}
				}
			}
			// Find H.264 encoder
			void* codec = ffmpeg.avcodec_find_encoder(AVCodecID.AV_CODEC_ID_H264);
			codecContext = ffmpeg.avcodec_alloc_context3((AVCodec*)codec);

			codecContext->width = width;
			codecContext->height = height;
			codecContext->pix_fmt = AVPixelFormat.AV_PIX_FMT_YUV420P;
			codecContext->time_base = new AVRational { num = 1, den = fps };
			codecContext->framerate = new AVRational { num = fps, den = 1 };
			codecContext->bit_rate = 4000000;

			if (0 > ffmpeg.avcodec_open2(codecContext, (AVCodec*)codec, null))
				return false;

			// Create stream
			stream = ffmpeg.avformat_new_stream(formatContext, (AVCodec*)codec);
			stream->codecpar->codec_id = AVCodecID.AV_CODEC_ID_H264;
			stream->codecpar->codec_type = AVMediaType.AVMEDIA_TYPE_VIDEO;
			stream->time_base = codecContext->time_base;

			return 0 > ffmpeg.avcodec_parameters_from_context(stream->codecpar, codecContext)
				&& 0 > ffmpeg.avio_open(&formatContext->pb, outputPath, ffmpeg.AVIO_FLAG_WRITE)
				&& 0 <= ffmpeg.avformat_write_header(formatContext, null);
		}
		public void AddFrame(byte* data, int stride) {
			AVFrame* frame;
			frame = ffmpeg.av_frame_alloc();
			frame->format = (int)AVPixelFormat.AV_PIX_FMT_YUV420P;
			frame->width = codecContext->width;
			frame->height = codecContext->height;
			frame->pts = frameIndex++;
			// Convert BGR to YUV420
			ConvertToYUV420(data, stride, *frame);
			// Encode the frame
			ffmpeg.avcodec_send_frame(codecContext, frame);
			AVPacket* pkt;
			pkt = ffmpeg.av_packet_alloc();
			ffmpeg.avcodec_receive_packet(codecContext, pkt);
			ffmpeg.av_interleaved_write_frame(formatContext, pkt);
			ffmpeg.av_packet_free(&pkt);
			ffmpeg.av_frame_free(&frame);
		}
		public bool Finish() {
			if (0 > ffmpeg.av_write_trailer(formatContext) || 0 > ffmpeg.avio_close(formatContext->pb))
				return false;
			unsafe {
				fixed (AVCodecContext** codeContextPtr = &codecContext) {
					ffmpeg.avcodec_free_context(codeContextPtr);
				}
			}
			ffmpeg.avformat_free_context(formatContext);
			return true;
		}
		private unsafe void ConvertToYUV420(byte* src, int stride, AVFrame frame) {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			byte ConvertToY(byte R, byte G, byte B) => (byte)(0.257 * R + 0.504 * G + 0.098 * B + 16);
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			byte ConvertToU(byte R, byte G, byte B) => (byte)(-0.148 * R - 0.291 * G + 0.439 * B + 128);
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			byte ConvertToV(byte R, byte G, byte B) => (byte)(0.439 * R - 0.368 * G - 0.071 * B + 128);
			int width = frame.width;
			int height = frame.height;
			byte* yPlane = frame.data[0];  // Luminance (Y)
			byte* uPlane = frame.data[1];  // Chrominance U
			byte* vPlane = frame.data[2];  // Chrominance V
			int yStride = frame.linesize[0];
			int uStride = frame.linesize[1];
			int vStride = frame.linesize[2];
			for (int y = 0; y < height; y++) {
				byte* srcRow = src + y * stride;  // BGR row
				byte* yRow = yPlane + y * yStride;  // Y row
				if (y % 2 == 0) { // Only process UV on even rows
					byte* uRow = uPlane + (y / 2) * uStride;
					byte* vRow = vPlane + (y / 2) * vStride;
					for (int x = 0; x < width; x += 2) { // UV is subsampled 2x2
						// BGR is stored as BGRBGRBGR...
						int x3 = x * 3, x13 = (x + 1) * 3, y1s = (y + 1) * stride, 
							y1sx3 = y1s + x3, y1sx13 = y1s+x13, y1sx = (y + 1) * yStride + x;
						byte B1 = srcRow[x3];
						byte G1 = srcRow[x3 + 1];
						byte R1 = srcRow[x3 + 2];
						byte B2 = srcRow[x13];
						byte G2 = srcRow[x13 + 1];
						byte R2 = srcRow[x13 + 2];
						byte B3 = src[y1sx3];
						byte G3 = src[y1sx3 + 1];
						byte R3 = src[y1sx3 + 2];
						byte B4 = src[y1sx13];
						byte G4 = src[y1sx13 + 1];
						byte R4 = src[y1sx13 + 2];
						// Convert to YUV
						yRow[x] = ConvertToY(R1, G1, B1);
						yRow[x + 1] = ConvertToY(R2, G2, B2);
						yPlane[y1sx] = ConvertToY(R3, G3, B3);
						yPlane[y1sx + 1] = ConvertToY(R4, G4, B4);
						// Average UV over 2x2 block
						byte U = (byte)((ConvertToU(R1, G1, B1) + ConvertToU(R2, G2, B2) + ConvertToU(R3, G3, B3) + ConvertToU(R4, G4, B4)) / 4);
						byte V = (byte)((ConvertToV(R1, G1, B1) + ConvertToV(R2, G2, B2) + ConvertToV(R3, G3, B3) + ConvertToV(R4, G4, B4)) / 4);
						uRow[x / 2] = U;
						vRow[x / 2] = V;
					}
				} else {
					for (int x = 0; x < width; x++) {
						int x3 = x * 3;
						byte B = srcRow[x3];
						byte G = srcRow[x3 + 1];
						byte R = srcRow[x3 + 2];
						yRow[x] = ConvertToY(R, G, B);
					}
				}
			}
		}
	}*/