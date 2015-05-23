namespace UnityColorFilters {
	using UnityEngine;
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
		private int minWidth;
		private int minHeight;

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
			// go over all rows
			List<Rectangle> rectangles = new List<Rectangle> ();
			List<Coordinate> seen = new List<Coordinate> ();

			for(int row = 0; row < image.Height; row++) {
				// go over all pixels
				for(int xPos = 0; xPos < image.Width; xPos++) {
					Coordinate coordinate = new Coordinate(xPos, row);

					// don't go over this pixel again
					if(seen.Contains(coordinate)) {
						continue;
					}

					Color color = image.getPixel(coordinate.X, coordinate.Y);
					if(color.r != 0 && color.g != 0 && color.b != 0) {
						// breadth first search expansion

						// construct a rectangle around the blob
						List<Coordinate> blob = breadthFirstSearchBlob(image, coordinate, seen);

						int topMost = 0;
						int bottomMost = image.Height;
						int leftMost = 0;
						int rightMost = image.Width;

						// for every coordinate in the blob
						foreach(Coordinate coord in blob) {
							if(coord.Y < topMost) {
								topMost = coord.Y;
							} else if(coord.Y > bottomMost) {
								bottomMost = coord.Y;
							}

							if(coord.X < leftMost) {
								leftMost = coord.X;
							} else if(coord.X > rightMost) {
								rightMost = coord.X;
							}
						}

						Rectangle rect = new Rectangle(leftMost, topMost, rightMost, bottomMost);
						rectangles.Add(rect);
					}
				}
			}

			return rectangles.ToArray ();
		}

		private List<Coordinate> breadthFirstSearchBlob(Image image, Coordinate startingPoint, List<Coordinate> seen) {
			List<Coordinate> ret = new List<Coordinate> ();
			breadthFirstSearchBlob (image, getAdjacentCoordinates(image, startingPoint), ret, seen);
			return ret;
		}

		private void breadthFirstSearchBlob(Image image,
		                                                List<Coordinate> coordinatesToCheck,
		                                                List<Coordinate> blob,
		                                                List<Coordinate> seen) {
			if (coordinatesToCheck.Capacity == 0) {
				return;
			}

			List<Coordinate> newCoordinatesToCheck = new List<Coordinate>();
			foreach (Coordinate coordinate in coordinatesToCheck) {
				if (seen.Contains (coordinate)) {
					continue;
				}

				Color color = image.getPixel (coordinate.X, coordinate.Y);
				if (color.r != 255 || color.g != 255 || color.b != 255) {
					// not a bright pixel so not part of the blob
					continue;
				}

				newCoordinatesToCheck.AddRange(getAdjacentCoordinates(image, coordinate));

				seen.Add(coordinate);
				blob.Add (coordinate);
			}

			breadthFirstSearchBlob (image, newCoordinatesToCheck, blob, seen);
		}

		private List<Coordinate> getAdjacentCoordinates(Image image, Coordinate coordinate) {
			List<Coordinate> adjacentCoordinates = new List<Coordinate> ();

			// left
			if (coordinate.X > 0) {
				adjacentCoordinates.Add (new Coordinate (coordinate.X - 1, coordinate.Y));
			}

			// right
			if (coordinate.X < image.Width - 1) {
				adjacentCoordinates.Add (new Coordinate (coordinate.X + 1, coordinate.Y));
			}

			// down
			if (coordinate.Y < image.Height - 1) {
				adjacentCoordinates.Add (new Coordinate (coordinate.X, coordinate.Y + 1));
			}

			// down-left
			if (coordinate.X > 0 && coordinate.Y < image.Height - 1) {
				adjacentCoordinates.Add (new Coordinate (coordinate.X - 1, coordinate.Y + 1));
			}

			// down-right
			if (coordinate.X < image.Width - 1 && coordinate.Y < image.Height - 1) {
				adjacentCoordinates.Add (new Coordinate (coordinate.X + 1, coordinate.Y + 1));
			}

			return adjacentCoordinates;
		}
	}
}