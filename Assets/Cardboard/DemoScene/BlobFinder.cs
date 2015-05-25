namespace UnityColorFilters {
//	using UnityEngine;
	using System.Collections.Generic;
	using System;

	class Coordinate : IEquatable<Coordinate> {
		private int x;
		private int y;

		public int X {
			get {
				return x;
			}

			set {
				x = value;
			}
		}

		public int Y {
			get {
				return y;
			}

			set {
				y = value;
			}
		}

		public Coordinate(int x, int y) {
			this.x = x;
			this.y = y;
		}

		public bool Equals(Coordinate coordinate) {
			if (coordinate.X == x && coordinate.Y == y) {
				return true;
			}

			return false;
		}
	}

	public class BlobFinder {
		private int minWidth = 0;
		private int minHeight = 0;

		public int MinWidth {
			get {
				return minWidth;
			}

			set {
				minWidth = value;
			}
		}

		public int MinHeight {
			get {
				return minHeight;
			}

			set {
				minHeight = value;
			}
		}

		public Rectangle[] Process(Image image) {
			List<Rectangle> rectangles = new List<Rectangle> ();

			// go over all rows
			for(int row = 0; row < image.Height; row++) {
				// go over all pixels
				for(int xPos = 0; xPos < image.Width; xPos++) {
					Coordinate coordinate = new Coordinate(xPos, row);

					// do not check coordinates that are inside a discovered rectangle
					// -> as a consequence, we do not detect blobs that are inside another blob's rectangle
					if(rectangles.Exists(rect => rect.ContainsCoordinate(coordinate.X, coordinate.Y))) {
						continue;
					}

					Color32 color = image.getPixel(coordinate.X, coordinate.Y);
					if(color.r != 0 && color.g != 0 && color.b != 0) {
						// breadth first search expansion
						Rectangle blob = breadthFirstSearchBlob(image, coordinate);

						// check if it has minHeight and minWidth
						if(blob.GetHeight >= minHeight && blob.GetWidth >= minWidth) {
			//				Debug.Log ("Found valid blob :) ");
							rectangles.Add(blob);
						} else {
			//				Debug.Log ("Blob " + blob.TopLeftX + " " + blob.TopLeftY + " " + blob.BottomRightX + " " + blob.BottomRightY + " invalid. Size: " + blob.GetWidth + "x" + blob.GetHeight + " checked coordinate " + coordinate.X + " " + coordinate.Y);
						}
					}
				}
			}

			return rectangles.ToArray ();
		}

		private Rectangle breadthFirstSearchBlob(Image image, Coordinate startingPoint) {
			List<Coordinate> seen = new List<Coordinate> ();
			List<Coordinate> coordinatesToCheck = new List<Coordinate> ();
			Rectangle blob = new Rectangle (startingPoint.X, startingPoint.Y, startingPoint.X, startingPoint.Y);

			coordinatesToCheck.Add (startingPoint);
			coordinatesToCheck.AddRange (getUnseenAdjacentCoordinates (image, startingPoint, seen));

			breadthFirstSearchBlob (image, coordinatesToCheck, blob, seen);

			return blob;
		}

		private void breadthFirstSearchBlob(Image image,
		                                    List<Coordinate> coordinatesToCheck,
		                                    Rectangle blob,
		                                    List<Coordinate> seen) {
			List<Coordinate> newCoordinatesToCheck = new List<Coordinate>();
			foreach (Coordinate coordinate in coordinatesToCheck) {
				if (seen.Contains (coordinate)) {
					continue;
				}
				seen.Add(coordinate);

				Color32 color = image.getPixel (coordinate.X, coordinate.Y);
				if (color.r == 0 || color.g == 0 || color.b == 0) {
					// black pixel so not part of the blob
					continue;
				}

				// also expand from this point in the next function call
				newCoordinatesToCheck.AddRange(getUnseenAdjacentCoordinates(image, coordinate, seen));

				if (coordinate.X < blob.TopLeftX) {
					blob.TopLeftX = coordinate.X;
				} else if(coordinate.X > blob.BottomRightX) {
					blob.BottomRightX = coordinate.X;
				}

				if(coordinate.Y < blob.TopLeftY) {
					blob.TopLeftY = coordinate.Y;
				} else if(coordinate.Y > blob.BottomRightY) {
					blob.BottomRightY = coordinate.Y;
				}
			}

			// only keep going if there is more to check
			if (coordinatesToCheck.Count > 0) {
				// expand to all points in the expandation list
				breadthFirstSearchBlob (image, newCoordinatesToCheck, blob, seen);
			}
		}

		private List<Coordinate> getUnseenAdjacentCoordinates(Image image, Coordinate coordinate, List<Coordinate> seen) {
			List<Coordinate> adjacentCoordinates = new List<Coordinate> ();

			// left
			if (coordinate.X > 0) {
				Coordinate coord = new Coordinate (coordinate.X - 1, coordinate.Y);
				if(!seen.Contains(coord)) {
					adjacentCoordinates.Add (coord);
				}
			}

			// right
			if (coordinate.X < image.Width - 1) {
				Coordinate coord = new Coordinate (coordinate.X + 1, coordinate.Y);
				if(!seen.Contains(coord)) {
					adjacentCoordinates.Add (coord);
				}
			}

			// down
			if (coordinate.Y < image.Height - 1) {
				Coordinate coord = new Coordinate (coordinate.X, coordinate.Y + 1);
				if(!seen.Contains(coord)) {
					adjacentCoordinates.Add (coord);
				}
			}

			// down-left
			if (coordinate.X > 0 && coordinate.Y < image.Height - 1) {
				Coordinate coord = new Coordinate (coordinate.X - 1, coordinate.Y + 1);
				if(!seen.Contains(coord)) {
					adjacentCoordinates.Add (coord);
				}
			}

			// down-right
			if (coordinate.X < image.Width - 1 && coordinate.Y < image.Height - 1) {
				Coordinate coord = new Coordinate (coordinate.X + 1, coordinate.Y + 1);
				if(!seen.Contains(coord)) {
					adjacentCoordinates.Add (coord);
				}
			}

			return adjacentCoordinates;
		}
	}
}