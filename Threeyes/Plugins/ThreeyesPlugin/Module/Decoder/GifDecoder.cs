using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;
using System;
using Threeyes.Core;
#if Threeyes_UnityGifDecoder
using ThreeDISevenZeroR.UnityGifDecoder;
#endif
namespace Threeyes.Decoder
{
    /// <summary>
    /// Warning：
    /// 1.因为贴图是临时生成，因此需要手动调用Dispose清除！
    /// </summary>
    public class GifDecoder : DecoderBase<List<GifFrameData>>
	{
		#region Interface
		public override DecodeResult<List<GifFrameData>> DecodeEx(byte[] data, IDecodeOption option = null)
		{
			return DecodeEx(data, (IDecodeOption)(option as DecodeOption));//PS:如果option不匹配，那就变为null参数
		}

		#endregion

		public static Texture2D DecodeFirstFrame(byte[] data, DecodeOption option = null)
		{
			//ToAdd:
			Texture2D texture2DResult = Texture2D.grayTexture;
			DecodeResult decodeResult = new DecodeResult();
			if (option == null)
				option = DecodeOption.Default;
#if Threeyes_UnityGifDecoder
			if (data != null && data.Length > 0)
			{
				try
				{
					var frameDelays = new List<float>();
					using (var gifStream = new GifStream(data))
					{
						while (gifStream.HasMoreData)
						{
							switch (gifStream.CurrentToken)
							{
								case GifStream.Token.Image:
									GifFrameData gifFrameData = GetTexture(option, gifStream);
									texture2DResult = gifFrameData.texture;
									return texture2DResult;//仅返回第一帧
								case GifStream.Token.Comment:
									var commentText = gifStream.ReadComment();
									//Debug.Log(commentText);
									break;

								default:
									gifStream.SkipToken(); // Other tokens
									break;
							}
						}
					}
				}
				catch (System.Exception e)
				{
					decodeResult.errorInfo = "Try Convert to gif error: " + e;
				}
			}
			else
			{
				Debug.LogError("The data is empty!");
				return texture2DResult;
			}
#else
            Debug.LogError("Relate Define not active!");
#endif
			return texture2DResult;
		}
		public static DecodeResult DecodeEx(byte[] data, DecodeOption option = null)
		{
			DecodeResult decodeResult = new DecodeResult();
			if (option == null)
				option = DecodeOption.Default;

#if Threeyes_UnityGifDecoder
			if (data != null && data.Length > 0)
			{
				try
				{
					var frameDelays = new List<float>();
					using (var gifStream = new GifStream(data))
					{
						while (gifStream.HasMoreData)
						{
							switch (gifStream.CurrentToken)
							{
								case GifStream.Token.Image:
									GifFrameData gifFrameData = GetTexture(option, gifStream);
									decodeResult.value.Add(gifFrameData);
									break;

								case GifStream.Token.Comment:
									var commentText = gifStream.ReadComment();
									//Debug.Log(commentText);
									break;

								default:
									gifStream.SkipToken(); // Other tokens
									break;
							}
						}
					}
				}
				catch (System.Exception e)
				{
					decodeResult.errorInfo = "Try Convert to gif error: " + e;
				}
			}
			else
			{
				Debug.LogError("The data is empty!");
				return decodeResult;
			}
#else
            Debug.LogError("Relate Define not active!");
#endif

			return decodeResult;
		}

		/// <summary>
		/// (每加载一张图片等待一帧）
		/// </summary>
		/// <param name="data"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static async Task<DecodeResult> DecodeExAsync(byte[] data, CancellationToken cancellationToken, DecodeOption option = null)
		{
			DecodeResult decodeResult = new DecodeResult();
			if (option == null)
				option = DecodeOption.Default;

#if Threeyes_UnityGifDecoder
			if (data != null && data.Length > 0)
			{
				try
				{
					var frameDelays = new List<float>();
					using (var gifStream = new GifStream(data))
					{
						while (gifStream.HasMoreData)
						{
							if (cancellationToken.IsCancellationRequested)//Canceled
							{
								//临时取消
								return decodeResult;
							}

							switch (gifStream.CurrentToken)
							{
								case GifStream.Token.Image:
									GifFrameData gifFrameData = GetTexture(option, gifStream);
									decodeResult.value.Add(gifFrameData);
									await Task.Yield();//每次成功读图，就等待一帧
									break;

								case GifStream.Token.Comment:
									var commentText = gifStream.ReadComment();
									//Debug.Log(commentText);
									break;

								default:
									gifStream.SkipToken(); // Other tokens
									break;
							}
						}
					}
				}
				catch (System.Exception e)
				{
					Debug.LogError("Try Convert to gif error:" + "r\n" + e);
				}
			}
			else
			{
				decodeResult.errorInfo = "The data is empty!";
			}
#else
            Debug.LogError("Relate Define not active!");
            await Task.Yield();
#endif

			return decodeResult;
		}
#if Threeyes_UnityGifDecoder
		static GifFrameData GetTexture(DecodeOption option, GifStream gifStream)
		{
			GifFrameData gifFrameData = new GifFrameData();
			var image = gifStream.ReadImage();
			var textureFrame = new Texture2D(gifStream.Header.width, gifStream.Header.height, TextureFormat.ARGB32, false);
			textureFrame.SetPixels32(image.colors);
			textureFrame.Apply();
			if (option.compress)//压缩
				textureFrame.TryCompress(option.compressInHighQuality);

			gifFrameData.texture = textureFrame;
			gifFrameData.frameDelaysSeconds = image.SafeDelaySeconds;// More about SafeDelay below
			return gifFrameData;
		}
#endif

		[Serializable]
		public class DecodeOption : IDecodeOption
		{
			public static DecodeOption Default { get { return new DecodeOption(); } }

			public bool compress = false;//Compress texture at runtime to DXT/BCn or ETC formats, which will cost extra time (Will force to make texture readable!)(It take times!)
			public bool compressInHighQuality = false;//Passing true for highQuality parameter will dither the source texture during compression, which helps to reduce compression artifacts but is slightly slower. This parameter is ignored for ETC 

			public DecodeOption()
			{
			}
			public DecodeOption(bool compress = true, bool compressInHighQuality = false)
			{
				this.compress = compress;
				this.compressInHighQuality = compressInHighQuality;
			}

		}

		public class DecodeResult : DecodeResult<List<GifFrameData>>
		{
			public DecodeResult()
			{
				value = new List<GifFrameData>();//避免为null导致后续访问错误
			}
		}
	}

	[Serializable]
	public class GifFrameData : IDisposable
	{
		public Texture2D texture;
		public float frameDelaysSeconds;//DelayTime

		public void Dispose()
		{
			if (texture)
				UnityEngine.Object.Destroy(texture);
		}
	}
}