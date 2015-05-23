namespace UnityColorFilters {
	using UnityEngine;
	using System;

	/**
	 * Filter which conerts an image to Grayscale using the <b>average</b> method.
	 * http://www.johndcook.com/blog/2009/08/24/algorithms-convert-color-grayscale/
	 **/
	public class GrayscaleFilter {
		private static float rWeight = 0.2989f;
		private static float gWeight = 0.5870f;
		private static float bWeight = 0.1140f;

		public static Image Process(Image image) {
			Color32[] newPixels = new Color32 [image.Pixels.Length];

			// for each pixel
			for (int i = 0; i < image.Pixels.Length; i++) {
				Color32 color = image.Pixels [i];

				double value = (rWeight * color.r + gWeight * color.g + bWeight * color.b) / 3.0f;
				byte rgb = (byte)Math.Round(value, MidpointRounding.AwayFromZero);
				newPixels[i] = new Color32(rgb, rgb, rgb, color.a);
			}

			return new Image (newPixels, image.Width, image.Height);
		}
	}
}