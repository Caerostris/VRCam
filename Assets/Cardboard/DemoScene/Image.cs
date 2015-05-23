namespace UnityColorFilters {
	using UnityEngine;

	public class Image {
		private Color32[] pixels;
		private int width;
		private int height;

		public Color32[] Pixels {
			get {
				return pixels;
			}

			set {
				pixels = value;
			}
		}

		public int Width {
			get {
				return width;
			}

			set {
				width = value;
			}
		}

		public int Height {
			get {
				return height;
			}

			set {
				height = value;
			}
		}

		public Image(Color32[] pixels, int width, int height) {
			this.pixels = pixels;
			this.width = width;
			this.height = height;
		}

		public static Image FromWebCamTexture(WebCamTexture tex) {
			return new Image (tex.GetPixels32 (), tex.width, tex.height);
		}

		public static Image FromTexture2D(Texture2D tex) {
			return new Image (tex.GetPixels32 (), tex.width, tex.height);
		}

		public Texture2D GetTexture2D() {
			Texture2D tex2d = new Texture2D(width, height);
			tex2d.SetPixels32 (pixels);
			tex2d.Apply ();
			return tex2d;
		}
	}
}