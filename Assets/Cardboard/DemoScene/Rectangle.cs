namespace UnityColorFilters {
	using UnityEngine;

	public class Rectangle {
		int topLeftX;
		int topLeftY;
		int bottomRightX;
		int bottomRightY;

		public int TopLeftX {
			get {
				return topLeftX;
			}

			set {
				topLeftX = value;
			}
		}

		public int TopLeftY {
			get {
				return topLeftY;
			}
			
			set {
				topLeftY = value;
			}
		}

		public int BottomRigtX {
			get {
				return bottomRightX;
			}
			
			set {
				bottomRightX = value;
			}
		}

		public int BottomRightY {
			get {
				return bottomRightY;
			}
			
			set {
				bottomRightY = value;
			}
		}

		public Rectangle(int topLeftX, int topLeftY, int bottomRightX, int bottomRightY) {
			this.topLeftX = topLeftX;
			this.topLeftY = topLeftY;
			this.bottomRightX = bottomRightX;
			this.bottomRightY = bottomRightY;
		}
	}
}