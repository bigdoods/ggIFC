// MIT License
// Copyright (c) 2016 Geometry Gym Pty Ltd

// Permission is hereby granted, free of charge, to any person obtaining a copy of this software 
// and associated documentation files (the "Software"), to deal in the Software without restriction, 
// including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
// subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all copies or substantial 
// portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT 
// LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Reflection;
using System.IO;

using Rhino.Collections;
using Rhino.Geometry;
using Rhino.DocObjects; 

namespace GGYM.IFC
{
	public partial class IfcCartesianPoint
	{
		internal override Point3d Coordinates
		{
			get { return new Point3d(mCoordinateX, mCoordinateY, Double.IsNaN(mCoordinateZ) ? 0 : mCoordinateZ); }
		}
		internal Point3d Coordinates3d { set { mCoordinateX = value.X; mCoordinateY = value.Y; mCoordinateZ = value.Z; } }
		internal Point2d Coordinates2d { set { mCoordinateX = value.X; mCoordinateY = value.Y; mCoordinateZ = double.NaN; } }
		internal IfcCartesianPoint(DatabaseIfc m, Point3d pt, List<int> genData) : base(m, genData) { Coordinates3d = pt; }
		internal IfcCartesianPoint(DatabaseIfc m, Point2d pt, List<int> genData) : base(m, genData) { Coordinates2d = pt; }
	}
	
	public partial class IfcCartesianPointList2D
	{
		public IfcCartesianPointList2D(DatabaseIfc m, IEnumerable<Point2d> coordList) : base(m)
		{
			List<Tuple<double, double>> pts = new List<Tuple<double, double>>();
			foreach (Point2d t in coordList)
				pts.Add(new Tuple<double, double>(t.X, t.Y));
			mCoordList = pts.ToArray();
		}
	}
	public partial class IfcConnectionPointEccentricity
	{
		internal Vector3d Eccentricity { get { return new Vector3d(mEccentricityInX, mEccentricityInY, mEccentricityInZ); } }

		internal IfcConnectionPointEccentricity(DatabaseIfc m, IfcPointOrVertexPoint v, Vector3d ecc) : base(m, v) { mEccentricityInX = ecc.X; mEccentricityInY = ecc.Y; mEccentricityInZ = ecc.Z; }
	}
}
