namespace UnityColorFilters {
	using UnityEngine;
	using System;

	public class EuclideanFilter {
		private short radius;
		private Color32 center;

		private Color32 fillColor = new Color32(0, 0, 0, 1);
		private bool fillOutside = true;

		public Color32 Center {
			get {
				return center;
			}

			set {
				center = value;
			}
		}

		public Color32 FillColor {
			get {
				return fillColor;
			}

			set {
				fillColor = value;
			}
		}

		public bool FillOutside {
			get {
				return fillOutside;
			}

			set {
				fillOutside = value;
			}
		}


		public short Radius {
			get {
				return radius;
			}

			set {
				radius = Math.Max ((short)0, Math.Min ((short)450, value));
			}
		}

		public EuclideanFilter(Color32 center, short radius) {
			this.center = center;
			this.radius = radius;
		}

		public Image Process(Image image) {
			Color32[] newPixels = new Color32[image.Pixels.Length];
			
			int cR = center.r;
			int cG = center.g;
			int cB = center.b;
			int radius2 = radius * radius;
			
			int dR, dG, dB;
			
			// for each pixel
			for (int i = 0; i < image.Pixels.Length; i++) {
				Color32 color = image.Pixels[i];
				
				dR = cR - color.r;
				dG = cG - color.g;
				dB = cB - color.b;
				
				// calculate the distance
				Color32 newColor = color;
				if(dR * dR + dG * dG + dB * dB <= radius2) {
					if(!fillOutside) {
						newColor = fillColor;
					}
				} else {
					if(fillOutside) {
						newColor = fillColor;
					}
				}
				
				newPixels[i] = newColor;
				//Debug.Log (color.ToString ());
			}

			return new Image (newPixels, image.Width, image.Height);
		}
	}
}