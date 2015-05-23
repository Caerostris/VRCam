namespace UnityColorFilters {
	using UnityEngine;

	public class BinaryFilter {
		private byte thresholdValue;

		public byte ThresholdValue {
			get {
				return thresholdValue;
			}

			set {
				thresholdValue = value;
			}
		}

		public BinaryFilter(byte thresholdValue) {
			this.thresholdValue = thresholdValue;
		}

		public Image Process(Image image) {
			Color32[] newPixels = new Color32[image.Pixels.Length];
			int thresholdValue3 = thresholdValue * 3;

			// for each pixel
			for (int i = 0; i < image.Pixels.Length; i++) {
				Color32 color = image.Pixels [i];

				int intensity = color.r + color.g + color.b;
				if(intensity >= thresholdValue3) {
					newPixels[i] = new Color32(255, 255, 255, color.a);
				} else {
					newPixels[i] = new Color32(0, 0, 0, color.a);
				}
			}

			return new Image (newPixels, image.Width, image.Height);
		}
	}
}